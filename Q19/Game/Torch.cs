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
    class Torch : SpritesheetEntity
    {
        public Light Light { get; set; }

        public Torch()
        {
            Add(Light = new Light(15, 1, new Color(255, 100, 100, 255)));
            Light.LightSource.CastShadows = true;
            Depth = Depths.Fire;
            FirstFrame = new Rectangle(0, 0, 18, 32);
            FPS = 10;
            FrameCount = 3;
            TexturePath = "World/WallTorch";
            CanPickup = true;
        }

        public override void Update(GameTime gameTime)
        {
            Light.Position = Position;
            base.Update(gameTime);
        }

        public override void OnDropped(Player player)
        {
            if (GetGameLevel().TryBurn(this))
                Remove(Light);
            base.OnDropped(player);
        }
    }
}
