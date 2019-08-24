using jsmars.Game2D;
using Microsoft.Xna.Framework;

namespace Q19
{
    class Light : Entity
    {
        public Light2D LightSource { get; }

        public float Size
        {
            get => LightSource.Size;
            set => LightSource.Size = value;
        }

        public Light(float size) : this(size, 2, Color.White) { }
        public Light(float size, float falloff, Color color)
        {
            LightSource = new Light2D()
            {
                Falloff = falloff,
                Size = size,
                Color = color.ToVector4(),
            };
        }

        public override void Update(GameTime gameTime)
        {
            LightSource.Position = Position;
            base.Update(gameTime);
        }

        protected override void onAdded()
        {
            if (!Q19Game.Instance.Lighting.Lights.Contains(LightSource) && IsActiveSceneOrEmpty)
                Q19Game.Instance.Lighting.Lights.Add(LightSource);
            base.onAdded();
        }

        protected override void onRemoved()
        {
            Q19Game.Instance.Lighting.Lights.Remove(LightSource);
            base.onRemoved();
        }
    }
}