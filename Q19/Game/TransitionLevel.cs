using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Q19
{
    enum TransitionState
    {
        Current,
        Next,
        GameOver
    }

    class TransitionLevel : Level
    {
        private readonly TransitionState _transitionState;
        private readonly GameLevel _previousLevel;
        private Level _nextLevel;
        private SpriteFont _scoreFont;
        private readonly Dictionary<int, Vector2> _previousPlayerPositions = new Dictionary<int, Vector2>();

        private List<ScoreTicker> _playerTickers;
        private ScoreTicker _bonusTicker;
        private ScoreTicker _totalTicker;
        private List<ScoreTicker> _allTickers;
        private ScoreTicker _activeTicker;
        private SoundEffectInstance _tickSound;
        private float _tickPause;

        private bool _goToLevel;
        private readonly PuzzlesPreview _puzzlesPreview;
        private bool _enterFromTopDoor;
        private Door _entryDoor;
        private Door _exitDoor;

        public TransitionLevel(Player playerEntered, GameLevel previousLevel) : this(TransitionState.Current, previousLevel, true)
        {
            var players = Q19Game.Instance.Players;
            foreach (var player in players)
            {
                if (player != playerEntered)
                    player.IsActive = false;
            }

            foreach (var player in players)
            {
                _previousPlayerPositions.Add((int)player.Index, player.Position);
            }

            _enterFromTopDoor = true;
        }

        public TransitionLevel(TransitionState state, GameLevel previousLevel, bool enterFromTop = false)
        {
            _transitionState = state;
            _previousLevel = previousLevel;
            _enterFromTopDoor = enterFromTop;

            PlayBounds.Add(new Rectangle(-180, -100, 65, 166));
            Add(new Entity()
            {
                TexturePath = "World/transitionscreen",
                Depth = Depths.Background,
            });

            Add(new Light(180));

            _entryDoor = new Door(this, new Vector2(-142, 80), 0, p => { });
            Add(_entryDoor);

            _exitDoor = new Door(this, new Vector2(-158, -94), 2, p => _goToLevel = true);
            Add(_exitDoor);

            if (_enterFromTopDoor)
            {
                _exitDoor.Open(true);
            }
            else
            {
                _entryDoor.Open(true);
                Delay(1f, () => _entryDoor.Close());
                Delay(3f, () => _exitDoor.Open());
            }

            if (_transitionState != TransitionState.GameOver)
            {
                _puzzlesPreview = new PuzzlesPreview();
                Add(_puzzlesPreview);
            }

            _tickPause = 2.5f;
        }

        public override void OnEnter()
        {
            var players = Q19Game.Instance.Players.Where(x => x.IsActive).ToList();
            if (_enterFromTopDoor)
            {
                var entryPosition = new Vector2(-146, -114);
                players.SetAndMoveTo(0, entryPosition, new Vector2(-166, -60));
                players.SetAndMoveTo(1, entryPosition, new Vector2(-133, -58));
                players.SetAndMoveTo(2, entryPosition, new Vector2(-168, -80));
                players.SetAndMoveTo(3, entryPosition, new Vector2(-130, -83));
            }
            else
            {
                var entryPosition = new Vector2(-146, 100);
                players.SetAndMoveTo(0, entryPosition, new Vector2(-166, 25));
                players.SetAndMoveTo(1, entryPosition, new Vector2(-133, 23));
                players.SetAndMoveTo(2, entryPosition, new Vector2(-168, 45));
                players.SetAndMoveTo(3, entryPosition, new Vector2(-130, 48));
            }

            base.OnEnter();
        }

        public override void OnLeave()
        {
            foreach (var player in Q19Game.Instance.Players)
            {
                player.IsActive = true;
                if (_previousPlayerPositions.Any())
                    player.Position = _previousPlayerPositions[(int) player.Index];
            }

            base.OnLeave();
        }

        private Level GetNextLevel()
        {
            switch (_transitionState)
            {
                case TransitionState.Current:
                    return _previousLevel;
                case TransitionState.Next:
                    return new GameLevel(_previousLevel.LevelNumber + 1);
                default:
                    return new StartLevel();
            }
        }

        public override void LoadContent(ContentManager content)
        {
            _scoreFont = content.Load<SpriteFont>("Fonts/GameFont");
            _tickSound = content.Load<SoundEffect>("Audio/scoreTick").CreateInstance();
            _nextLevel = GetNextLevel();
            if (_nextLevel is GameLevel level)
            {
                _puzzlesPreview.LoadContent(content, level);
            }

            if (_transitionState != TransitionState.Current)
                CalculateScore();

            base.LoadContent(content);
        }

        public override void Update(GameTime gameTime)
        {
            if (_transitionState == TransitionState.Current)
            {
                base.Update(gameTime);
                return;
            }

            var elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (Helpers.IsPressingAnyButton())
                elapsedTime *= 4;

            foreach (var ticker in _allTickers)
            {
                ticker.Update(elapsedTime);
            }

            if (_tickPause <= 0)
            {
                if (_activeTicker == null)
                {
                    _tickPause = 0.5f;
                    ActivateTicker(_allTickers[0]);
                }
                else if (_activeTicker.Finished)
                {
                    _tickPause = 0.5f;
                    var activeIndex = _allTickers.IndexOf(_activeTicker);
                    if (_allTickers.Count > activeIndex + 1)
                        ActivateTicker(_allTickers[activeIndex + 1]);
                }
            }

            _tickPause -= elapsedTime;

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            var x = 30;
            var y = 5;

            if (_nextLevel is GameLevel gameLevel)
            {
                Helpers.DrawWithShadow(sb, _scoreFont, $"Level {gameLevel.LevelNumber + 1}", new Vector2(102 + x - 10, -116 + y + 2), 1.3f);
            }

            if (_transitionState == TransitionState.Next)
            {
                DrawScore(sb);
            }
            else if(_transitionState == TransitionState.GameOver)
            {
                DrawScore(sb);

                if (_previousLevel is GameLevel previousLevel)
                    Helpers.DrawWithShadow(sb, _scoreFont, $"Level {previousLevel.LevelNumber + 1}", new Vector2(-28, 13), 1.3f);

                Helpers.DrawWithShadow(sb, _scoreFont, "Online Highscore", new Vector2(98 + x, -116 + y), 1.3f);
            }

            base.Draw(sb, gameTime);
        }

        private void DrawScore(SpriteBatch sb)
        {
            var x = -34;
            var y = 20;
            Helpers.DrawWithShadow(sb, _scoreFont, "Players", new Vector2(x, -115 + y), 1f);
            var playerIndex = 0;
            var yPos = 0;
            foreach (var playerTicker in _playerTickers)
            {
                Helpers.DrawWithShadow(sb, _scoreFont, $"P{playerIndex + 1}: {playerTicker.Score}", new Vector2(x, -95 + yPos + y),
                    ScaleBump(playerTicker), playerTicker.Color);
                yPos += 18;
                playerIndex++;
            }

            if(_transitionState == TransitionState.Next)
            {
                Helpers.DrawWithShadow(sb, _scoreFont, "Bonus", new Vector2(x, -11 + y), 1f);
                Helpers.DrawWithShadow(sb, _scoreFont, _bonusTicker.Score.ToString(), new Vector2(x, 9 + y), ScaleBump(_bonusTicker));
            }

            Helpers.DrawWithShadow(sb, _scoreFont, "Score", new Vector2(x, 38 + y), 1.3f);
            Helpers.DrawWithShadow(sb, _scoreFont, _totalTicker.Score.ToString(), new Vector2(x, 60 + y), 1.3f * ScaleBump(_totalTicker));
        }

        private float ScaleBump(ScoreTicker scoreTicker)
        {
            var timeSinceChange = scoreTicker.TimeSinceLastChange;
            const float bumpTime = 0.1f;
            const float bumpAmount = 0.2f;
            var bump = MathHelper.Clamp(bumpAmount * (1 - timeSinceChange / bumpTime), 0, bumpAmount);
            return 1 + bump;
        }

        public override bool IsComplete()
        {
            return _goToLevel;
        }

        public override Level GetNext()
        {
            return _nextLevel;
        }

        public void CalculateScore()
        {
            var score = new ScoreCalculator().Calculate(_previousLevel);

            _allTickers = new List<ScoreTicker>();
            if (_transitionState == TransitionState.Next)
            {
                _allTickers.AddRange(_playerTickers = GetActivePlayersScores(score.PlayerScores));
                _allTickers.Add(_bonusTicker = new ScoreTicker(0, score.BonusScore, _tickSound, 3f));
                _allTickers.Add(_totalTicker = new ScoreTicker(score.StartTotalScore, score.TotalScore, _tickSound, 3f));
            }
            else
            {
                _allTickers.AddRange(_playerTickers = GetActivePlayersScores(score.PlayerTotalScores));
                _allTickers.Add(_totalTicker = new ScoreTicker(0, score.TotalScore, _tickSound, 4f));
            }

            if (_transitionState == TransitionState.GameOver)
            {
                var time = DateTime.Now - Q19Game.Instance.StartTime;
            }
        }

        private List<ScoreTicker> GetActivePlayersScores(int[] scorePlayerScores)
        {
            var scores = new List<ScoreTicker>();
            for (var index = 0; index < scorePlayerScores.Length; index++)
            {
                var player = Q19Game.Instance.Players.SingleOrDefault(x => (int) x.Index == index);
                if (player != null)
                {
                    scores.Add(new ScoreTicker(0, scorePlayerScores[index], _tickSound, 1f)
                    {
                        Color = player.PlayerColor
                    });
                }
            }
            return scores;
        }

        private void ActivateTicker(ScoreTicker ticker)
        {
            _activeTicker = ticker;
            _activeTicker.Active = true;
        }
    }
}