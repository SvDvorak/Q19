using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Q19
{
    class PuzzlesPreview : Entity
    {
        private List<Texture2D> _textures;
        private Texture2D _activePuzzle;
        private int _activeIndex;
        private SpriteFont _font;

        public void LoadContent(ContentManager content, GameLevel level)
        {
            _textures = level.PuzzlePaths.Select(x => content.Load<Texture2D>(x)).ToList();

            if (_textures.Any())
                _activePuzzle = _textures[_activeIndex];

            Add(new Entity()
            {
                TexturePath = "World/GoLeft",
                Depth = Depths.Menu,
                Position = new Vector2(78, 120)
            });
            Add(new Entity()
            {
                TexturePath = "World/GoRight",
                Depth = Depths.Menu,
                Position = new Vector2(172, 120)
            });

            _font = content.Load<SpriteFont>("Fonts/GameFont");
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            if(_activePuzzle != null)
            {
                sb.Draw(
                    _activePuzzle,
                    new Rectangle(-Q19Game.Instance.GameWidth / 2 + 260, Q19Game.Instance.GameHeight / 2 - 230, 200, 200),
                    null,
                    Color.White,
                    0,
                    Vector2.Zero,
                    SpriteEffects.None,
                    Depths.PuzzleBG);

                Helpers.DrawWithShadow(sb, _font, $"{_activeIndex + 1} / {_textures.Count}", new Vector2(112 + 12, 121), 1f);
            }

            base.Draw(sb, gameTime);
        }

        public void GoTo(int change)
        {
            if (!_textures.Any())
                return;

            _activeIndex = (_activeIndex + change + _textures.Count) % _textures.Count;
            _activePuzzle = _textures[_activeIndex];
        }
    }
}