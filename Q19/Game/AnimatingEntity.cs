using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Q19
{
    class AnimatingEntity : Entity
    {
        public int Direction { get; private set; }

        protected int FrameSize;
        private Vector2 _lastPos;
        private float _anim;
        private int _frame;
        public int Frames { get; set; } = 8;

        public override void Update(GameTime gameTime)
        {
            var speed = 200 * (float) gameTime.ElapsedGameTime.TotalSeconds;
            Animate(speed);
            _lastPos = Position;
            base.Update(gameTime);
        }

        private void Animate(float speed)
        {
            var movement = _lastPos - Position;
            if (movement != Vector2.Zero)
            {
                const float minAnimSpeed = 2;
                const float animSpeed = 4;

                var dist = movement.Length();
                _anim += Math.Max(dist, minAnimSpeed) / speed / animSpeed;
                if (_anim > 1)
                {
                    _anim -= 1;
                    _frame++;
                    if (_frame >= Frames)
                        _frame = 0;
                }

                movement.Normalize();
                if (Math.Abs(movement.X) > Math.Abs(movement.Y))
                    Direction = movement.X > 0 ? 0 : 2;
                else
                    Direction = movement.Y > 0 ? 1 : 3;
            }
            else
                _frame = 0;
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            SourceArea = new Rectangle(_frame * FrameSize, Direction * FrameSize, FrameSize, FrameSize);
            Origin = new Vector2(FrameSize) / 2;
            base.Draw(sb, gameTime);
        }
    }
}