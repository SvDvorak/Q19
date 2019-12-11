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
    class SpritesheetEntity : Entity
    {
        public Rectangle FirstFrame { get => firstFrame; set { firstFrame = value; SourceArea = value; } }
        public int FrameCount { get; set; } = 2;
        public int FPS { get; set; } = 15;
        public int CurrentFrame { get; private set; }
        public bool Loop { get; set; } = true;
        public bool Reverse { get; set; } = false;
        public Dictionary<int, float> DepthChanges = new Dictionary<int, float>();

        private float animTime;
        Rectangle firstFrame;

        public override void Update(GameTime gameTime)
        {
            animTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (animTime >= 1f / FPS)
            {
                animTime -= 1f / FPS;
                var nextFrame = CurrentFrame + (Reverse ? -1 : 1);
                if (Loop)
                    UpdateFrame(nextFrame % FrameCount);
                else
                    UpdateFrame(Helpers.Clamp(nextFrame, 0, FrameCount - 1));
            }

            base.Update(gameTime);
        }

        public void JumpToLastFrame()
        {
            UpdateFrame(FrameCount - 1);
        }

        private void UpdateFrame(int frame)
        {
            CurrentFrame = frame;
            if (DepthChanges.Any())
            {
                Depth = DepthChanges
                    .OrderBy(x => x.Key)
                    .FirstOrDefault(x => CurrentFrame >= x.Key)
                    .Value;
            }

            var source = FirstFrame;
            source.X += CurrentFrame * FirstFrame.Width;
            SourceArea = source;
            Origin = new Vector2(SourceArea.Width / 2, SourceArea.Height / 2);
        }
    }
}
