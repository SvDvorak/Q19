using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using jsmars.Game2D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Q19
{
    class GameLevel : Level
    {
        static float CommentTime = 4.5f;
        static float TutorialTime = 6;

        private TransitionLevel _nextLevel;
        private bool _isLeavingThisLevel;
        private readonly Door _nextLevelDoor;
        private readonly Door _returnDoor;
        public List<string> PuzzlePaths => Puzzles.Select(x => x.TexturePath).ToList();
        public int LevelNumber { get; private set; }
        public bool ReadyForNext { get; set; }
        public LevelSettings Settings { get; private set; }
        public bool GameOver { get; private set; }
        public Puzzle[] Puzzles { get; private set; }
        public Rectangle MainArea { get; private set; }
        public int RelicScore { get; private set; }
        public float LightDecreasePerSecBalanced => Settings.LightDecreasePerSec * (0.75f + (Q19Game.Instance.Players.Count / 4)); // light decreases faster for more players
        private Player _returnedPlayer;
        SoundEffectInstance levelReadySound;

        int tutNum = -2;
        float tutTime;
        string[] tutorial = new string[]
        {
            "Pick up pieces and place them on the canvas with (A) or Space or L",
            "Rotate pieces with (X) and (B) or Z/X or J/K",
            "When youve filled half the canvas the door opens",
            "You must keep the fire alive by burning things!",
            "But avoid my valuables!",
            "Bonus points for fast completion and the more you fill each puzzle",
        };

        SpritesheetEntity oldman;

        public GameLevel(int level)
        {
            LevelNumber = level;
            Add(new Entity()
            {
                TexturePath = $"World/level{Helpers.Rnd.Next(5)}",
                Depth = Depths.Background,
            });

            MainArea = new Rectangle(-192, -104, 384, 192);
            PlayBounds.Add(MainArea);

            Add(new Fire(Math.Max(25, 180 - level * 2)) { Position = GetFreePosition() });

            Settings = LevelGenerator.Generate(level);
            Puzzles = new Puzzle[Settings.Puzzles.Length];
            for (int i = 0; i < Settings.Puzzles.Length; i++)
            {
                var item = Settings.Puzzles[i];
                Add(Puzzles[i] = new Puzzle(item.Width, item.Height, item.Width * item.PieceSize, item.Height * item.PieceSize, 0, item.Image));
            }
            for (int i = 0; i < Settings.Relics; i++)
            {
                var r = new Relic();
                PlaceRelic(r);
                Add(r);
            }

            _returnDoor = new Door(this, new Vector2(10, 101), 0, p =>
            {
                SetNextLevel(new TransitionLevel(p, this));
                _returnedPlayer = p;
            });
            _returnDoor.Open(true);
            Add(_returnDoor);

            _nextLevelDoor = new Door(this, new Vector2(-10, -101), 2, p => SetNextLevel(new TransitionLevel(!GameOver ? TransitionState.Next : TransitionState.GameOver, this)));
            Add(_nextLevelDoor);

            for (int i = 1; i < Helpers.Rnd.Next(1, 5); i++)
                Add(new Rat());


            Add(new Torch() { Position = new Vector2(80, -120) });
            Add(new Torch() { Position = new Vector2(180, -120) });
            Add(new Torch() { Position = new Vector2(-80, -120), Effects = SpriteEffects.FlipHorizontally });
            Add(new Torch() { Position = new Vector2(-180, -120), Effects = SpriteEffects.FlipHorizontally });
            Add(oldman = new SpritesheetEntity() { TexturePath = "Player/oldman", FirstFrame = new Rectangle(0, 0, 32, 32), Depth = Depths.Players + 0.0001f, FrameCount = 4, FPS = 2, Position = new Vector2(MainArea.Right - 16, MainArea.Top + 20) });
        }

        private void SetNextLevel(TransitionLevel level)
        {
            _nextLevel = level;
            _isLeavingThisLevel = true;
        }

        public override void LoadContent(ContentManager content)
        {
            levelReadySound = content.Load<SoundEffect>("Audio/LevelReady").CreateInstance();
            if (!HasEventOccured("LoadedPuzzle"))
            {
                SetEvent("LoadedPuzzle");
                LevelGenerator.SetupPositions(this);
                base.LoadContent(content);

                foreach (var item in Puzzles)
                    item.PlacePuzzlePieces();
            }
            else
                base.LoadContent(content);
        }

        public override void OnEnter()
        {
            var entryPosition = new Vector2(6, 121);
            var doorExit = entryPosition + new Vector2(0, -30);
            if (LevelNumber == 0)
            {
                tutNum = -1;
                tutTime = 4;
            }

            if (!HasEventOccured("SetPlayersPositionsOnce"))
            {
                var players = Q19Game.Instance.Players;
                players.SetAndMoveTo(0, entryPosition, doorExit, new Vector2(-43, 60));
                players.SetAndMoveTo(1, entryPosition, doorExit, new Vector2(-16, 49));
                players.SetAndMoveTo(2, entryPosition, doorExit, new Vector2(16, 46));
                players.SetAndMoveTo(3, entryPosition, doorExit, new Vector2(43, 62));
                SetEvent("SetPlayersPositionsOnce");
            }
            else if(_returnedPlayer != null)
            {
                _returnedPlayer.MoveTo(doorExit);
            }

            // Default so that it doesn't crash when we skip jump to next level
            _nextLevel = new TransitionLevel(TransitionState.Next, this);

            base.OnEnter();
        }

        public override void Update(GameTime gameTime)
        {
            if((ReadyForNext || GameOver) && !HasEventOccured("OpenedNextLevelDoor"))
            {
                Delay(1f, () => _nextLevelDoor.Open());
                Delay(0.5f, () => _returnDoor.Close());
                SetEvent("OpenedNextLevelDoor");
            }

            if (commentTime > -1)
                commentTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Tutorial
            if (tutTime > 0)
            {
                tutTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (tutTime <= 0)
                {
                    tutNum++;
                    if (tutNum >= tutorial.Length)
                    {
                        tutTime = 0;
                        tutNum = -2;
                    }
                    else
                    {
                        tutTime = TutorialTime;
                        Comment(tutorial[tutNum]);
                    }
                }
            }

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            if (commentTime > 0)
            {
                var m = Q19Game.Instance.Font1.MeasureString(comment);
                m.Y -= 10;
                var r = new Rectangle((int)(oldman.Bounds.X - m.X - 4), (int)(oldman.Position.Y - m.Y - 32), (int)m.X + 16, (int)m.Y + 16);
                var s = r;
                s.X += 2;
                s.Y += 2;
                sb.Draw(Q19Game.Pixel, r, null, new Color(0, 0, 0, 100), 0, Vector2.Zero, SpriteEffects.None, Depths.Menu + 0.00002f);
                //sb.Draw(Q19Game.Pixel, s, null, Color.Black, 0, Vector2.Zero, SpriteEffects.None, Depths.Menu + 0.0001f);
                sb.DrawString(Q19Game.Instance.Font1, comment, new Vector2(r.X + 4, r.Y + 4), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, Depths.Menu);
                sb.DrawString(Q19Game.Instance.Font1, comment, new Vector2(r.X + 6, r.Y + 6), Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, Depths.Menu + 0.00001f);


                sb.DrawString(Q19Game.Instance.Font1, comment, new Vector2(r.X + 4, r.Y + 4), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, Depths.Menu);
            }
            else if (commentTime > -1 && commentScore != 0)
            {
                sb.DrawString(Q19Game.Instance.Font1, commentScore.ToString(), oldman.Position + new Vector2(-16, -16), commentScore > 0 ? Color.Green : Color.Red, 0, Vector2.Zero, 1, SpriteEffects.None, Depths.Menu + 0.00001f);
                sb.DrawString(Q19Game.Instance.Font1, commentScore.ToString(), oldman.Position + new Vector2(-15, -15), commentScore > 0 ? Color.Black : Color.Black, 0, Vector2.Zero, 1, SpriteEffects.None, Depths.Menu + 0.00002f);
            }

            base.Draw(sb, gameTime);
        }

        public override bool IsComplete()
        {
            return _isLeavingThisLevel;
        }

        public void CheckGameOver()
        {
            bool anyLight = false;
            Visit(e =>
            {
                if (e is Fire f && f.LightAmount > 0)
                    anyLight = true;
            });

            if (!anyLight)
            {
                // GAME OVER :(
                GameOver = true;
                Visit(e =>
                {
                    e.Color = Color.Red;
                    e.CanPickup = false;
                });
            }
        }

        public override Level GetNext()
        {
            _isLeavingThisLevel = false;
            return _nextLevel;
        }

        public Vector2 GetFreePosition(int Tries = 250)
        {
            Vector2 r = Helpers.Rnd.Vector2(MainArea.Left, MainArea.Right, MainArea.Top, MainArea.Bottom);
            bool collision = false;
            Visit(e =>
            {
            if (!collision)
            {
                    if (e is Puzzle p)
                    {
                        var b = p.Bounds;
                        b.X -= LevelSettings.DefaultPieceSize / 2;
                        b.Y -= LevelSettings.DefaultPieceSize / 2;
                        b.Width += LevelSettings.DefaultPieceSize / 2;
                        b.Height += LevelSettings.DefaultPieceSize / 2;
                        if (b.Contains(r))
                            collision = true;
                    }
                    else if (e is Fire f && Vector2.DistanceSquared(r, f.Position) < 50 * 50)
                        collision = true;
                }
            });
            if (collision && Tries > 0)
                return GetFreePosition(Tries - 1);
            if (collision)
                return r;
            return r;
        }

        public void CheckLevelReady()
        {
            bool ready = true;
            foreach (var item in Puzzles)
                if (item.CheckCompleteRate() < 0.5f)
                    ready = false;

            if (ready && !ReadyForNext)
            {
                ReadyForNext = true;
                levelReadySound.Play();
            }
        }

        public void PlaceRelic(Relic relic, int Tries = 20)
        {
            //TODO: Avoid other relics & doors
            var sides = Helpers.Rnd.Float() > 0.5f;
            var topleft = Helpers.Rnd.Float() > 0.5f;
            relic.Position = !sides ? new Vector2(Helpers.Rnd.Float() > 0.5f ? Helpers.Rnd.Float(MainArea.Left, -40) : Helpers.Rnd.Float(40, MainArea.Right), topleft ? MainArea.Top - 12 : MainArea.Bottom + 26) :
                                      new Vector2(topleft ? MainArea.Left - 16 : MainArea.Right + 16, Helpers.Rnd.Float(MainArea.Top, MainArea.Bottom));

            if (!sides)
            {
                if (!topleft)
                    relic.Rotate(2);
            }
            else
                relic.Rotate(topleft ? 3 : 1);

            // check collision
            bool collision = false;
            Visit(e =>
            {
                if (!collision && e != relic && (e is Relic r || e is Door) && Vector2.DistanceSquared(e.Position, relic.Position) < 30 * 30)
                    collision = true;
            });
            if (collision && Tries > 0)
                PlaceRelic(relic, Tries - 1);

        }

        public bool TryBurn(Entity entity)
        {
            bool burn = false;
            Visit(e =>
            {
                if (!burn && e is Fire f && f.CarriedBy == null && Vector2.DistanceSquared(entity.Position, f.Position) < Fire.FireTossDistance * Fire.FireTossDistance)
                {
                    f.Burn(entity);
                    burn = true;
                }
            });
            return burn;
        }

        string comment = "";
        float commentTime;
        int commentScore;

        public void Comment(string comment)
        {
            commentScore = 0;
            this.comment = comment;
            commentTime = CommentTime;
        }

        public void RelicBurned(string comment, int score)
        {
            Comment(comment);
            commentScore = score;
            RelicScore += score;
        }
    }
}
