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
    class Fire : SpritesheetEntity
    {
        public const int FireTossDistance = 40;
        public float LightAmount;
        private SoundEffectInstance _fireSound, _fireGoingOut, _addingToFire;
        private readonly float _lightStart;
        private bool _fireHasGoneOut;

        public Light Light { get; set; }

        public Fire(float lightStart)
        {
            TexturePath = "World/fire";
            _lightStart = lightStart;
            LightAmount = lightStart;
            CanPickup = true;
            Add(Light = new Light(_lightStart));
            Depth = Depths.Fire;
        }

        public override void LoadContent(ContentManager content)
        {
            _fireSound = content.Load<SoundEffect>("Audio/fireSound").CreateInstance();
            _fireGoingOut = content.Load<SoundEffect>("Audio/fireGoingOut").CreateInstance();
            _addingToFire = content.Load<SoundEffect>("Audio/addingToFire").CreateInstance();
            if (!Q19Game.DebugMode)
                _fireSound.IsLooped = true;
            _fireSound.Play();
            FirstFrame = new Rectangle(0, 0, 32, 35);
            FPS = 10;
            FrameCount = 4;
            base.LoadContent(content);
        }

        public override void Update(GameTime gameTime)
        {
            var level = GetGameLevel();
            if (level.GameOver)
                Light.Size -= LevelSettings.GameOverLightIncrease;
            else
            {
                var decrease = (float)(level.GameOver ? LevelSettings.GameOverLightIncrease : level.LightDecreasePerSecBalanced * gameTime.ElapsedGameTime.TotalSeconds);
                if (Q19Game.DebugMode)
                {
                    if (Keyboard.GetState().IsKeyDown(Keys.T))
                        decrease *= 10;
                    if (Q19Game.DebugEasyMode)
                        decrease *= 0.1f;
                }
                LightAmount -= decrease;

                var lightRatio = MathHelper.Clamp(LightAmount / _lightStart, 0, 1);
                if (lightRatio > 0.4f)
                    lightRatio = MathHelper.Lerp(0.7f, 1f, 1f / 0.6f * (lightRatio - 0.4f));
                else if (lightRatio > 0)
                    lightRatio = MathHelper.Lerp(0f, 0.7f, 1f / 0.4f * lightRatio);

                Light.Size = lightRatio * _lightStart;

                if (!_fireHasGoneOut && LightAmount <= 0)
                {
                    _fireHasGoneOut = true;
                    _fireGoingOut.Play();
                    GetGameLevel().CheckGameOver();
                }
            }

            Light.Position = Position;
            _fireSound.Volume = 0.2f + MathHelper.Clamp(LightAmount / _lightStart, 0, 1);
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            if (CarriedBy == null)
            {
                var full = GetGameLevel().Settings.MaxLight;
                var filled = LightAmount / full;
                var color = filled > 0.95f ? Color.Yellow : filled > 0.15f ? Color.Orange : Color.Red;
                sb.Draw(Q19Game.Pixel, (Point2)(Position + new Vector2(2, 20)), null, Color.Black, 0, new Vector2(0.5f), new Vector2((int)(filled * 32 + 2), 4 + 2), SpriteEffects.None, Depths.Fire - 0.0001f);
                sb.Draw(Q19Game.Pixel, (Point2)(Position + new Vector2(0, 18)), null, color, 0, new Vector2(0.5f), new Vector2((int)(filled * 32), 4), SpriteEffects.None, Depths.Fire - 0.0002f);
            }

            base.Draw(sb, gameTime);
        }

        protected override void onAdded()
        {
            _fireSound?.Play();
            base.onAdded();
        }

        protected override void onRemoved()
        {
            _fireSound.Stop(true);
            base.onRemoved();
        }

        public void Burn(Entity e)
        {
            e.Color = Color.Red;
            e.AnimateMove(Position);
            e.CanPickup = false;
            e.Burned = true;
            _addingToFire.Play();
            var value = GetGameLevel().Settings.LightIncreasePerBurn;
            if (e is Relic r)
                value *= r.Variant.FireValue;
            if (e is Torch)
                value = 10;
            if (e is Rat)
                value *= 2;
            LightAmount = Math.Min(GetGameLevel().Settings.MaxLight, LightAmount + value);
        }
    }
}
