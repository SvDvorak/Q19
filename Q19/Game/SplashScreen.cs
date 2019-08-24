using Microsoft.Xna.Framework;

namespace Q19
{
    class SplashScreen : Level
    {
        public SplashScreen()
        {
            Add(new SpritesheetEntity()
            {
                TexturePath = "Title/splash",
                Depth = Depths.Background,
                FirstFrame = new Rectangle(0, 0, 240, 135),
                FrameCount = 12,
                FPS = 6,
                Scale = new Vector2(2)
            });

            Add(new Light(800));
        }

        public override void OnEnter()
        {
            Q19Game.Instance.Players.ForEach(x => x.IsActive = false);
            base.OnEnter();
        }

        public override void Update(GameTime gameTime)
        {
            if (Helpers.IsPressingAnyButton())
                SetEvent("ExitSplash");
            base.Update(gameTime);
        }

        public override bool IsComplete()
        {
            return HasEventOccured("ExitSplash");
        }

        public override Level GetNext()
        {
            return new StartLevel();
        }
    }
}