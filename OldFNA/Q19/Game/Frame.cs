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
    class Frame : Entity
    {
        Texture2D edge, edgeh, corner, canvas;

        Rectangle[] rects = new Rectangle[8];
        SpriteEffects[] effects = new SpriteEffects[8];
        Rectangle canvasr;
        int w = 10;

        public Frame(Rectangle bounds, ContentManager content)
        {
            rects[0] = new Rectangle(bounds.Left - w, bounds.Top - w, w, w);
            rects[1] = new Rectangle(bounds.Left - w, bounds.Bottom, w, w);
            rects[2] = new Rectangle(bounds.Right, bounds.Top - w, w, w);
            rects[3] = new Rectangle(bounds.Right, bounds.Bottom, w, w);
            rects[4] = new Rectangle(bounds.Left, bounds.Top - w, bounds.Width, w);
            rects[5] = new Rectangle(bounds.Left, bounds.Bottom, bounds.Width, w);
            rects[6] = new Rectangle(bounds.Left - w, bounds.Top, w, bounds.Height);
            rects[7] = new Rectangle(bounds.Right, bounds.Top, w, bounds.Height);

            effects[1] = SpriteEffects.FlipVertically;
            effects[2] = SpriteEffects.FlipHorizontally;
            effects[3] = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
            effects[5] = SpriteEffects.FlipVertically;
            effects[7] = SpriteEffects.FlipHorizontally;

            canvasr = bounds;
            LoadContent(content);
        }

        public override void LoadContent(ContentManager content)
        {
            edge = content.Load<Texture2D>("World/frameedge");
            edgeh = content.Load<Texture2D>("World/frameedgeh");
            corner = content.Load<Texture2D>("World/framecorner");
            canvas = content.Load<Texture2D>("World/canvas");
            base.LoadContent(content);
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            for (int i = 0; i < rects.Length; i++)
                sb.Draw(i < 4 ? corner : i < 6 ? edge : edgeh, rects[i], null, Color.White, 0, Vector2.Zero, effects[i], Depths.PuzzleBG + 0.0001f);
            var rr = canvasr;
            rr.X += rects[0].Right;
            rr.Y += rects[0].Bottom;
            sb.Draw(canvas, Parent.Bounds, new Rectangle(0, 0, canvasr.Width, canvasr.Height), Color.White, 0, Vector2.Zero, SpriteEffects.None, Depths.PuzzleBG + 0.0001f);
            base.Draw(sb, gameTime);
        }
    }
}
