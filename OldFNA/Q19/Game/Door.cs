using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Q19
{
    class Door : SpritesheetEntity
    {
        public bool IsOpen { get; private set; }
        private readonly Rectangle _triggerArea;
        private readonly Rectangle _walkArea;
        private readonly Level _parent;
        private readonly Action<Player> _onEnter;
        private readonly Entity _doorFrameUnder;
        private readonly Entity _doorFrameOver;
        private SoundEffectInstance _doorOpen;
        private SoundEffectInstance _doorClose;
        private bool _onTriggerLastFrame;

        public Door(Level parent, Vector2 position, int rotation, Action<Player> onEnter)
        {
            TexturePath = "World/Door";
            DepthChanges.Add(0, Depths.DoorUnder - 0.001f);
            DepthChanges.Add(1, Depths.DoorOver);
            Rotate(rotation + 2);
            FirstFrame = new Rectangle(0, 0, 64, 64);
            FPS = 9;
            Reverse = true;
            FrameCount = 3;
            Loop = false;
            Position = position;

            _doorFrameUnder = new Entity()
            {
                TexturePath = "World/DoorFrameUnder",
                Depth = Depths.DoorUnder,
                Position = position
            };
            _doorFrameUnder.Rotate(rotation + 2);

            _doorFrameOver = new Entity()
            {
                TexturePath = "World/DoorFrameOver",
                Depth = Depths.DoorOver,
                Position = position
            };
            _doorFrameOver.Rotate(rotation + 2);

            var topCover = new Entity()
            {
                TexturePath = "World/DoorTopCover",
                Depth = Depths.DoorOver + 0.001f,
                Position = position + (rotation == 0 ? -1 : 1)*new Vector2(10, -37)
            };
            topCover.Rotate(rotation + 2);

            Add(_doorFrameUnder);
            Add(_doorFrameOver);
            Add(topCover);

            _parent = parent;
            var p = new Point((int)position.X, (int)position.Y);
            if(rotation == 2)
            {
                _walkArea = new Rectangle(p.X - 4, p.Y - 37, 27, 67);
                _triggerArea = new Rectangle(p.X - 4, p.Y - 60, 27, 37);
            }
            else
            {
                _walkArea = new Rectangle(p.X - 23, p.Y - 30, 27, 70);
                _triggerArea = new Rectangle(p.X - 23, p.Y + 23, 27, 37);
            }

            _onEnter = onEnter;
        }

        public void Open(bool immediate = false)
        {
            IsOpen = true;
            _parent.PlayBounds.Add(_walkArea);

            if (immediate)
                JumpToLastFrame();
            else
                _doorOpen.Play();
            Reverse = false;
        }

        public void Close()
        {
            IsOpen = false;
            _parent.PlayBounds.Remove(_walkArea);
            Reverse = true;

            _doorClose.Play();
        }

        public override void LoadContent(ContentManager content)
        {
            _doorOpen = content.Load<SoundEffect>("Audio/doorOpen").CreateInstance();
            _doorClose = content.Load<SoundEffect>("Audio/doorClose").CreateInstance();
            base.LoadContent(content);
        }

        public override void Update(GameTime gameTime)
        {
            var playersOnTrigger = Q19Game.Instance.Players.Where(player => _triggerArea.Contains(player.Position));
            var anyPlayersOnTrigger = playersOnTrigger.Any();
            if (!_onTriggerLastFrame && anyPlayersOnTrigger)
            {
                _onTriggerLastFrame = true;
                _onEnter(playersOnTrigger.First());
            }

            _onTriggerLastFrame = anyPlayersOnTrigger;
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            if (Q19Game.Instance.DebugDrawing)
            {
                sb.Draw(Q19Game.Pixel, _triggerArea, new Color(30, 30, 100, 50));
            }

            base.Draw(sb, gameTime);
        }
    }
}