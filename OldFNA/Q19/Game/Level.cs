using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Q19
{
    abstract class Level : Entity
    {
        public List<Rectangle> PlayBounds { get; } = new List<Rectangle>();

        private readonly HashSet<string> _events = new HashSet<string>();

        public virtual bool IsComplete()
        {
            return false;
        }

        public virtual Level GetNext()
        {
            return null;
        }

        public virtual void OnEnter() { }
        public virtual void OnLeave() { }

        public BoundsCollision IsWithinBounds(Entity entity, Vector2 movement)
        {
            var collidingX = true;
            var collidingY = true;

            foreach (var boundary in PlayBounds)
            {
                var b = boundary;
                var pX = new Rectangle((int)(entity.Position.X + 8 + movement.X - entity.Origin.X * entity.Scale.X),
                             (int)(entity.Position.Y + 5 - entity.Origin.Y * entity.Scale.Y),
                             (int)(entity.SourceArea.Width * entity.Scale.X - 16),
                             (int)(entity.SourceArea.Height * entity.Scale.Y - 16));

                var pY = new Rectangle((int)(entity.Position.X + 8 - entity.Origin.X * entity.Scale.X),
                             (int)(entity.Position.Y + 5 + movement.Y - entity.Origin.Y * entity.Scale.Y),
                             (int)(entity.SourceArea.Width * entity.Scale.X - 16),
                             (int)(entity.SourceArea.Height * entity.Scale.Y - 16));

                if (b.Contains(pX))
                    collidingX = false;

                if (b.Contains(pY))
                    collidingY = false;
            }

            return new BoundsCollision(collidingX, collidingY);
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            if(Q19Game.Instance.DebugDrawing)
                foreach(var bound in PlayBounds)
                    sb.Draw(Q19Game.Pixel, bound, new Color(0, 100, 0, 50));

            base.Draw(sb, gameTime);
        }

        public void SetEvent(string name)
        {
            _events.Add(name);
        }

        public bool HasEventOccured(string name)
        {
            return _events.Contains(name);
        }
    }

    class BoundsCollision
    {
        public bool CollidingX { get; set; }
        public bool CollidingY { get; set; }

        public BoundsCollision(bool collidingX, bool collidingY)
        {
            CollidingX = collidingX;
            CollidingY = collidingY;
        }
    }
}