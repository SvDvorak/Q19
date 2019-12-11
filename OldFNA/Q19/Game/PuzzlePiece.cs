using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using jsmars.Game2D;

namespace Q19
{
    class PuzzlePiece : Entity
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public Point2 PuzzleLocation => new Point2(X, Y);
        public Puzzle Puzzle => Parent as Puzzle;
        public Vector2 CorrectPosition => Puzzle.Position - Puzzle.Origin * Scale + localCorrectPosition + Origin * Scale;
        public bool UpsideDown { get => Color == Color.Black; set => Color = value ? Color.Black : Color.White; }
        public bool IsPlacedInPuzzle { get; set; }
        public bool IsPlacedOnGrid { get; set; }
        public bool IsExtra { get; set; }
        public Player PlacedBy { get; private set; }

        Vector2 localCorrectPosition;
        private SoundEffectInstance _piecePlaced, _pieceLocked;

        Texture2D jigsawTex, jigsawBorder;

        public PuzzlePiece(Puzzle puzzle, int x, int y)
        {
            TexturePath = puzzle.TexturePath;
            X = x;
            Y = y;
            CanPickup = true;
            Depth = Depths.PuzzlePieceIncrement;
            Rotate(Helpers.Rnd.Next(4));
        }

        public override void LoadContent(ContentManager content)
        {
            base.LoadContent(content);

            Scale = Puzzle.Scale;

            // temp square pieces
            var w = Puzzle.Texture.Width / Puzzle.PiecesX;
            var h = Puzzle.Texture.Height / Puzzle.PiecesY;
            SourceArea = new Rectangle(w * X, h * Y, w, h);
            Origin = new Vector2(w / 2, h / 2);
            var pw = Puzzle.Width / (float)Puzzle.PiecesX;
            var ph = Puzzle.Height / (float)Puzzle.PiecesY;
            localCorrectPosition = new Vector2(X * pw, Y * ph);
            //Color = new Color((float)Helpers.Rnd.NextDouble(), (float)Helpers.Rnd.NextDouble(), (float)Helpers.Rnd.NextDouble(), 255);

            Scale = new Vector2((float)Puzzle.Width / Puzzle.Texture.Width, (float)Puzzle.Height / Puzzle.Texture.Height);
            //Position = Helpers.Rnd.Float() > 0.5f ?
            //            Helpers.Rnd.Vector2(-200, -100, -100, 100) :
            //            Helpers.Rnd.Vector2(100, 200, -100, 100);
            UpsideDown = Helpers.Rnd.Float() > 0.5f;

            _piecePlaced = content.Load<SoundEffect>("Audio/PiecePlaced").CreateInstance();
            _pieceLocked = content.Load<SoundEffect>("Audio/PlaceCorrect2").CreateInstance();


            int seed = 123;
            // nsew
            // edge, male, female
            var data = new int[]
            {
                Y == 0 ? 0                  : RndXY(seed, X, Y, true, false),
                Y + 1 == Puzzle.PiecesY ? 0 : RndXY(seed, X, Y + 1, true, true),
                X + 1 == Puzzle.PiecesX ? 0 : RndXY(seed, X + 1, Y, false, true),
                X == 0 ? 0                  : RndXY(seed, X, Y, false, false),
            };
            jigsawTex = Jigsaw.RenderJigsaw(PuzzleLocation, Texture, Puzzle.PiecesSize, Puzzle.Size, data);

            // Create border texture
            //TODO: This is probably a bit slow
            jigsawBorder = new Texture2D(Q19Game.Instance.GD, jigsawTex.Width, jigsawTex.Height);
            Color[] color = new Color[jigsawTex.Width * jigsawTex.Height];
            jigsawTex.GetData(color);
            for (int i = 0; i < color.Length; i++)
            {
                var a = color[i].A;
                color[i] = new Color(a, a, a, a);
            }
            jigsawBorder.SetData(color);

            //TODO: Overwrite texture stuff when hack drawing is fixed
            //Texture = jigsawTex;
            //Origin = new Vector2(jigsawTex.Width / 2, jigsawTex.Height / 2);
            //SourceArea = new Rectangle(0, 0, jigsawTex.Width, jigsawTex.Height);
            //Scale = Vector2.One;
        }

        private int RndXY(int seed, int x, int y, bool isX, bool invert)
        {
            var r = new Random(seed + (isX ? y : x));
            for (int i = 0; i < (isX ? x : y); i++)
                r.Next();

            var o = r.Next(1, 3);
            if (invert)
                return o == 1 ? 2 : 1;
            return o;
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            //TODO: Hack drawing for now, we need the original puzzle values for placement to be correct at the moment. 
            var color = IsExtra && !Burned ? Color.Red : UpsideDown ? Color.DarkGray : Color;
            var origin = new Vector2(jigsawTex.Width / 2, jigsawTex.Height / 2);
            sb.Draw(UpsideDown ? jigsawBorder : jigsawTex, Position, null, color, drawRotation, origin, 1, SpriteEffects.None, Depth);

            // Hover
            if (CanPickup)
            {
                sb.Draw(jigsawBorder, Position, null, hoverColor(), drawRotation, origin, 1 * 1.15f, SpriteEffects.None, Depth + 0.0000001f);
                sb.Draw(jigsawBorder, Position, null, new Color(0, 0, 0, 80), drawRotation, origin, 1 * 1.3f, SpriteEffects.None, Depth + 0.0000002f);
            }
            // Shadow
            //if (CanPickup)
            //    sb.Draw(jigsawTex, Position + new Vector2(3), null, new Color(0, 0, 0, 200), drawRotation, origin, 1, SpriteEffects.None, Depth + 0.0001f);

            //base.Draw(sb, gameTime);
        }

        protected override void onParentChanged(Entity parent)
        {
            if (parent is Puzzle == false)
                throw new Exception("Can only be child of Puzzle");
            base.onParentChanged(parent);
        }

        public override void OnPickedUp(Player player)
        {
            UpsideDown = false;
            IsPlacedInPuzzle = false;
            IsPlacedOnGrid = false;
            PlacedBy = null;
            base.OnPickedUp(player);
        }

        public override void OnDropped(Player player)
        {
            if (Puzzle.Bounds.Contains(Position))
            {
                IsPlacedOnGrid = true;

                // Calc closest puzzle grid position
                var pieceSize = Puzzle.Size / Puzzle.PiecesSize;
                var b = (AABB)Puzzle.Bounds;
                Vector2 p = (Point2)((Position - b.Min) / pieceSize);
                var snapGridPos = b.Min + p * pieceSize + pieceSize / 2;

                if (Rotation == 0 && !UpsideDown && Vector2.DistanceSquared(snapGridPos, CorrectPosition) < 5 * 5) // probably exact same position but could differ slightly
                {
                    AnimateMove(CorrectPosition);
                    IsPlacedInPuzzle = true;
                    PlacedBy = player;

                    if (Puzzle.TryLockPosition(X, Y, true))
                    {
                        _pieceLocked.Play();
                        CanPickup = false;
                        if (Helpers.Rnd.Float() < 0.07f)
                        {

                            var name = player == null ? "Mouse-Man" : player.PlayerColorName;
                            var list = new List<string>()
                            {
                                $"Nice one {name}!",
                                $"Good job {name}!",
                                $"Sweet!",
                                $"Score!",
                                $"Kaching {name} does it again!",
                            };
                            GetGameLevel().Comment(Helpers.Rnd.FromList(list));
                        }
                    }
                    else
                        _piecePlaced.Play();
                }
                else
                {
                    AnimateMove(snapGridPos);
                    _piecePlaced.Play();

                    if (Helpers.Rnd.Float() < 0.07f)
                    {
                        var name = player == null ? "Mouse-Man" : player.PlayerColorName;
                        var list = new List<string>()
                        {
                            $"What were you thinking {name}?",
                            $"Um, really {name}?",
                            $"Thats just wrong {name}!",
                            $"Everyone else saw that it wouldnt fit {name}",
                            $"Are you even trying {name}?",
                            $"Try again {name}",
                            $"I dont think thats correct {name}",
                            $"Not sure about that one {name}",
                        };
                        GetGameLevel().Comment(Helpers.Rnd.FromList(list));
                    }
                }
            }
            else
            {
                GetGameLevel().TryBurn(this);
            }
            Puzzle.OnPiecePlaced();
            base.OnDropped(player);
        }
    }
}
