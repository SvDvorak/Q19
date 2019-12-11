using System;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Q19
{
    class DelayedAction
    {
        public float Time;
        public readonly Action Action;

        public DelayedAction(float time, Action action)
        {
            Action = action;
            Time = time;
        }
    }

    class Entity
    {
        public Texture2D Texture { get; set; }
        public string TexturePath { get; set; }

        private Vector2 _position;
        public Vector2 Position { get => _position; set { _animate = false; _position = value; } }

        public Rectangle SourceArea { get; set; }
        public Color Color { get; set; } = Color.White;
        public Vector2 Origin { get; set; }
        public float RotationRadians { get; private set; }
        public int Rotation { get; private set; }
        public float Depth { get; set; } = 0.5f;
        public Vector2 Scale { get; set; } = Vector2.One;
        public SpriteEffects Effects { get; set; }

        public Entity Parent { get; private set; }
        List<Entity> children { get; set; } = new List<Entity>();
        public bool CanPickup { get; set; } = false;
        public Player CarriedBy { get; set; }
        public bool Burned { get; set; }
        public bool Focused { get => focusTime > 0; set => focusTime = value ? 2 : 0; }
        public Color FocusColor { get; set; }
        public Rectangle Bounds => new Rectangle((int)(Position.X - (Origin.X * Scale.X)),
                                                 (int)(Position.Y - (Origin.Y * Scale.Y)),
                                                 (int)(SourceArea.Width * Scale.X),
                                                 (int)(SourceArea.Height * Scale.Y));
        public bool IsActiveSceneOrEmpty => Q19Game.Instance.Scene == null || GetLevel() == Q19Game.Instance.Scene.Level;
        public bool IsActive { get; set; } = true;

        float focusTime;
        protected float drawRotation;
        private bool _hasLoadedContent;

        private bool _animate;
        private Vector2 _animNewPosition;
        private float _animSpeed;

        private readonly List<DelayedAction> _delayedActions = new List<DelayedAction>();

        public virtual void LoadContent(ContentManager content)
        {
            if (_hasLoadedContent)
                return;

            drawRotation = RotationRadians;
            if (!string.IsNullOrEmpty(TexturePath))
            {
                Texture = content.Load<Texture2D>(TexturePath);
                if (SourceArea == Rectangle.Empty)
                    SourceArea = new Rectangle(0, 0, Texture.Width, Texture.Height);
                if (Origin == Vector2.Zero)
                    Origin = new Vector2(SourceArea.Width / 2, SourceArea.Height / 2);
            }

            foreach (var item in children)
                item.LoadContent(content);

            _hasLoadedContent = true;
        }

        public virtual void Update(GameTime gameTime)
        {
            if (!IsActive)
                return;

            if (_animate)
            {
                var amount = _animSpeed * 15 * (float)gameTime.ElapsedGameTime.TotalSeconds;
                _position = _position * (1 - amount) + _animNewPosition * amount;
            }

            foreach (var item in children)
                item.Update(gameTime);

            if (Focused)
                focusTime--;

            if (Q19Game.DebugMode && CanPickup && IsMouseHovering)
                TouchFocus(Color.White);

            if (RotationRadians != drawRotation)
            {
                drawRotation = drawRotation + (RotationRadians - drawRotation) * 0.15f;

                if (Math.Abs(RotationRadians - drawRotation) < 0.001f)
                    drawRotation = RotationRadians;
            }

            if (Burned)
            {
                if (Color.A >= 5)
                {
                    Color = new Color(Color.R, Color.G, Color.B, Color.A - 5);
                    if (Color.A == 0)
                        Color = Color.Transparent;
                }
            }

            UpdateAndRemoveActions(gameTime);
        }

        private void UpdateAndRemoveActions(GameTime gameTime)
        {
            var finishedActions = new List<DelayedAction>();
            foreach (var delayedAction in _delayedActions)
            {
                delayedAction.Time -= (float) gameTime.ElapsedGameTime.TotalSeconds;

                if (delayedAction.Time < 0)
                    finishedActions.Add(delayedAction);
            }

            foreach (var finished in finishedActions)
            {
                finished.Action();
                _delayedActions.Remove(finished);
            }
        }

        public void AnimateMove(Vector2 newPosition, float speed = 1)
        {
            _animate = true;
            _animNewPosition = newPosition;
            _animSpeed = speed;
        }

        public virtual void Draw(SpriteBatch sb, GameTime gameTime)
        {
            if (!IsActive)
                return;

            if (Texture != null)
            {
                sb.Draw(Texture, new Vector2((int)Position.X, (int)Position.Y), SourceArea, Color, drawRotation, Origin, Scale, Effects, Depth);

                // Hover
                if (Focused)
                    sb.Draw(Texture, new Vector2((int)Position.X, (int)Position.Y), SourceArea, hoverColor(), drawRotation, Origin, Scale * 1.2f, Effects, Depth + 0.001f);
                // Shadow
                if (CanPickup)
                    sb.Draw(Texture, new Vector2((int)Position.X + 3, (int)Position.Y + 3), SourceArea, new Color(0, 0, 0, 200), drawRotation, Origin, Scale, Effects, Depth + 0.0001f);
            }

            if (Q19Game.Instance.DebugDrawing)
                sb.Draw(Q19Game.Pixel, Bounds, new Color(40, 40, 40, 40));

            foreach (var item in children)
                item.Draw(sb, gameTime);
        }

        public void Rotate(int direction)
        {
            Rotation += direction;
            Rotation %= 4;
            if (Rotation < 0)
                Rotation += 4;
            RotationRadians += direction * MathHelper.PiOver2;
        }

        public void TouchFocus(Color color)
        {
            FocusColor = color;
            Focused = true;
        }
        protected virtual Color hoverColor() => Focused ? FocusColor : new Color(255, 255, 255, 255);

        public GameLevel GetGameLevel()
        {
            if (this is GameLevel l)
                return l;
            if (Parent == null)
                return null;
            return Parent.GetGameLevel();
        }
        public Level GetLevel()
        {
            if (this is Level l)
                return l;
            if (Parent == null)
                return null;
            return Parent.GetLevel();
        }

        public void Add(Entity e)
        {
            children.Add(e);
            Visit(c => c.onAdded());
            e.onParentChanged(this);
        }
        public void Remove(Entity e)
        {
            Visit(c => c.onRemoved());
            children.Remove(e);
            e.onParentChanged(null);
        }
        public void Visit(EntityEvent action)
        {
            action(this);
            foreach (var item in children)
                item.Visit(action);
        }
        public T First<T>() where T : class
        {
            T output = null;
            Visit(e =>
            {
                if (output == null && e is T)
                    output = e as T; //TODO: Break
            });
            return output;
        }
        protected virtual void onParentChanged(Entity parent)
        {
            Parent = parent;
        }
        protected virtual void onAdded()
        {
        }
        protected virtual void onRemoved()
        {

        }

        public virtual void OnPickedUp(Player carrier)
        {
            if(carrier != null)
            {
                // Carrier has arrived
                if(CarriedBy != null)
                    CarriedBy.Carrying = null;
                carrier.Carrying = this;
                CarriedBy = carrier;
            }
        }

        public virtual void OnDropped(Player player)
        {
            CarriedBy = null;
            if (CanPickup)
                Depth = Depths.PuzzlePieceIncrement;
            else
                Depth = Depths.PuzzlePieceLocked;
        }

        public bool IsMouseHovering => new Rectangle((int)(Position.X - Origin.X * Scale.X), (int)(Position.Y - Origin.Y * Scale.Y),
            (int)(SourceArea.Width * Scale.X), (int)(SourceArea.Height * Scale.Y)).Contains(Q19Game.Instance.MousePosition.ToPoint());


        public void Delay(float time, Action action)
        {
            _delayedActions.Add(new DelayedAction(time, action));
        }
    }

    delegate void EntityEvent(Entity entity);
    delegate bool EntityBoolEvent(Entity entity);
}
