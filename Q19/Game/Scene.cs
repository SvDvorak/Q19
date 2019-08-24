using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Q19
{
    class Scene : Entity
    {
        public int TotalScore { get; set; }
        public string ScoreText { get; private set; } = "";
        public Level Level { get; private set; }

        private Level _nextLevel;
        private float _fadeAmount;
        private float _fadeDirection;
        private readonly float _fadeSpeed = Q19Game.DebugMode ? 10 : 2.5f;

        public Scene()
        {
            LoadPlayers();

            if (Q19Game.DebugMode && Q19Game.DebugInstantLevel > 0)
            {
                var level = (Q19Game.DebugInstantLevel - 1) / 2;
                var trans = (Q19Game.DebugInstantLevel - 1) % 2 == 0;
                if (trans)
                    Add(Level = new TransitionLevel(TransitionState.Current, new GameLevel(level)));
                else
                    Add(Level = new GameLevel(level));
            }
            else
                Add(Level = new SplashScreen());
        }

        public override void LoadContent(ContentManager content)
        {
            Level.OnEnter();
            base.LoadContent(content);
        }

        public override void Update(GameTime gameTime)
        {
            if (_nextLevel == null &&
                (Level.IsComplete() || Q19Game.DebugMode && Input.KeyPressed(Keys.P)
                                   || (Level is GameLevel gl && gl.ReadyForNext && Input.KeyPressed(Keys.Enter))))
                FadeTo(Level.GetNext());

            if (Input.KeyPressed(Keys.R) && Q19Game.DebugMode)
                FadeTo(new StartLevel());

            if (_nextLevel != null && _fadeAmount >= 1)
            {
                StartLevel(_nextLevel);
                _nextLevel = null;
                _fadeDirection = -_fadeSpeed;
            }

            _fadeAmount = MathHelper.Clamp(_fadeAmount + _fadeDirection * (float)gameTime.ElapsedGameTime.TotalSeconds, 0, 1);

            base.Update(gameTime);
        }

        private void FadeTo(Level level)
        {
            if (Level is StartLevel)
                Q19Game.Instance.StartTime = DateTime.Now;

            _fadeDirection = _fadeSpeed;
            _nextLevel = level;
        }

        public void StartLevel(Level newLevel)
        {
            Level.OnLeave();
            newLevel.LoadContent(Q19Game.Instance.Content);
            Remove(Level);
            Add(Level = newLevel);
            Level.OnEnter();
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            var color = new Color(0, 0, 0, _fadeAmount);
            sb.Draw(Q19Game.Pixel, new Rectangle(0, 0, Q19Game.Instance.GameWidth, Q19Game.Instance.GameHeight), null, color, 0, new Vector2(0.5f), SpriteEffects.None, Depths.ScreenFade);
            base.Draw(sb, gameTime);
        }

        public void LoadPlayers()
        {
            for (int i = 0; i < 4; i++)
            {
                if (!Q19Game.Instance.Players.Any(x => i == (int) x.Index))
                {
                    var player = new Player((PlayerIndex) i);
                    player.LoadContent(Q19Game.Instance.Content);

                    Q19Game.Instance.Players.Add(player);
                    Add(player);
                }
            }
        }

        public void RemovePlayer(Player player)
        {
            Remove(player);
            Q19Game.Instance.Players.Remove(player);
        }
    }
}
