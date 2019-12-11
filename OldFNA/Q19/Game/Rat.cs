using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Q19
{
    class Rat : AnimatingEntity
    {
        Vector2 target;
        float wait;

        SoundEffectInstance dieSound;

        public Rat()
        {
            TexturePath = "World/rataimation";
            Frames = 3;
            FrameSize = 32;
            Depth = Depths.Players + 0.001f;
            CanPickup = true;
        }

        public override void LoadContent(ContentManager content)
        {
            dieSound = content.Load<SoundEffect>("Audio/RatDie").CreateInstance();
            base.LoadContent(content);
            Position = RandomLevelPos();
            SetNewTarget();
            wait = Helpers.Rnd.Float(0, 3);
        }

        public override void Update(GameTime gameTime)
        {
            var speed = 1;

            if (CarriedBy == null)
            {
                var dir = target - Position;
                if (dir != Vector2.Zero)
                    Position += Vector2.Normalize(dir) * speed;

                if (Vector2.DistanceSquared(Position, target) < 1)
                {
                    wait = Helpers.Rnd.Float(0, 6);
                    Position = target;
                }

                if (wait > 0)
                {
                    wait -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (wait <= 0)
                        SetNewTarget();
                }

                Q19Game.Instance.Scene.Visit(e =>
                {
                    if (e is Player p && Vector2.DistanceSquared(Position, p.Position) < 32 * 32)
                    {
                        var dr = p.Position - Position;
                        if (dr != Vector2.Zero)
                        {
                            var n = -Vector2.Normalize(dr);
                            Position += n * 1;
                            target = Position + n * 5;
                        }
                    //SetNewTarget();
                }
                });
            }

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            base.Draw(sb, gameTime);
        }

        public void SetNewTarget()
        {
            target = RandomLevelPos();
        }

        public Vector2 RandomLevelPos()
        {
            var b = GetGameLevel().MainArea;
            var edge = 20;
            return Helpers.Rnd.Vector2(b.Left + edge, b.Right - edge, b.Top + edge, b.Bottom - edge);
        }

        public override void OnDropped(Player player)
        {
            if (GetGameLevel().TryBurn(this))
                dieSound.Play();
            base.OnDropped(player);
        }
    }
}
