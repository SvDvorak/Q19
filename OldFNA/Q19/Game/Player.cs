using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Q19
{
    class Player : AnimatingEntity
    {
        public const float PickupDistance = 40;

        public PlayerIndex Index { get; }
        public float Speed = 100;
        private KeyboardState _previousKeyboardState;
        private GamePadState _previousGamePadState;
        public Entity Carrying { get; set; }
        public Entity TargetEntity { get; private set; }
        public int TotalScore { get; set; }
        public string PlayerColorName { get; set; }
        public Color PlayerColor { get; set; }

        private SoundEffect _spinCarriedSound;
        private SoundEffectInstance _dropItem;
        private SoundEffectInstance _pickupItem;
        private MovementSounds _movementSounds;

        private bool _disableInput;
        private List<Vector2> _autoMoveTargets;

        public Player(PlayerIndex index)
        {
            Index = index;
            TexturePath = $"Player/PlayerAnim{(int)index}";
            FrameSize = 32;
            Depth = Depths.Players + (int)index * 0.0001f;
            Position = new Vector2((int)index % 2 == 1 ? 100 : -100, (int)index < 2 ? -50 : 50);
            switch (index)
            {
                case PlayerIndex.One:
                    PlayerColorName = "Blue";
                    PlayerColor = Color.Blue;
                    break;
                case PlayerIndex.Two:
                    PlayerColorName = "Red";
                    PlayerColor = Color.Red;
                    break;
                case PlayerIndex.Three:
                    PlayerColorName = "Green";
                    PlayerColor = Color.Green;
                    break;
                case PlayerIndex.Four:
                    PlayerColorName = "Yellow";
                    PlayerColor = Color.Yellow;
                    break;
            }
        }

        public override void LoadContent(ContentManager content)
        {
            _spinCarriedSound = content.Load<SoundEffect>("Audio/SpinCarried");
            _dropItem = content.Load<SoundEffect>("Audio/DropItem").CreateInstance();
            _pickupItem = content.Load<SoundEffect>("Audio/PickupItem").CreateInstance();
            _movementSounds = new MovementSounds(content);
            base.LoadContent(content);
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsActive)
                return;

            var gamePad = GamePad.GetState(Index);
            var kb = Keyboard.GetState();

            if (JustPressed(gamePad, Buttons.A) || JustTriggered(gamePad, x => x.Right) || JustPressed(kb, Keys.L, Keys.C) || (Index == PlayerIndex.One && Input.KeyPressed(Keys.Space)))
                PickupOrDrop();

            var pressedRotateLeft = JustPressed(gamePad, Buttons.LeftShoulder) || JustPressed(gamePad, Buttons.X) || JustPressed(kb, Keys.J, Keys.V) || (Index == PlayerIndex.One && Input.KeyPressed(Keys.Z));
            var pressedRotateRight = JustPressed(gamePad, Buttons.RightShoulder) || JustPressed(gamePad, Buttons.B) || JustPressed(kb, Keys.K, Keys.B) || (Index == PlayerIndex.One && Input.KeyPressed(Keys.X));
            if (Carrying != null)
            {
                Carrying?.AnimateMove(Position + new Vector2(0, -20), 1);

                if (pressedRotateLeft)
                    RotateCarried(-1);
                if (pressedRotateRight)
                    RotateCarried(1);
            }

            if(pressedRotateLeft || pressedRotateRight)
            {
                var change = (pressedRotateLeft ? -1 : 0) + (pressedRotateRight ? 1 : 0);
                Q19Game.Instance.Scene.Visit(e =>
                {
                    if (e is PuzzlesPreview pp)
                    {
                        pp.GoTo(change);
                    }
                });
            }

            var moveInput = GetGamePadMovement(gamePad);
            if (Q19Game.DebugIsActivePlayer(Index))
                moveInput += GetKeyboardMovement(kb, PlayerIndex.One);
            else if (Index < PlayerIndex.Three)
                moveInput += GetKeyboardMovement(kb, Index);

            if (_disableInput)
            {
                var currentTarget = _autoMoveTargets[0];
                var toTarget = currentTarget - Position;
                var distanceToTarget = toTarget.Length();
                moveInput = toTarget;
                if (distanceToTarget < Speed * (float) gameTime.ElapsedGameTime.TotalSeconds)
                {
                    _autoMoveTargets.RemoveAt(0);
                    if(!_autoMoveTargets.Any())
                        _disableInput = false;
                }
            }

            var isMoving = moveInput != Vector2.Zero;
            if (isMoving)
            {
                var movement = Vector2.Normalize(moveInput) * Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

                #region level bound collision

                var result = Q19Game.Instance.Scene.Level.IsWithinBounds(this, movement);
                if(result.CollidingX)
                    movement.X = 0;
                if (result.CollidingY)
                    movement.Y = 0;

                #endregion


                Position += movement;
            }

            _movementSounds.UpdateMovementSound(isMoving, gameTime);

            var pickupEntities = Q19Game.Instance.GetAllPickupEntities();
            var closestDistance = float.MaxValue;
            TargetEntity = null;
            foreach (var pickupEntity in pickupEntities)
            {
                var distance = Vector2.DistanceSquared(pickupEntity.Position, Position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    TargetEntity = pickupEntity;
                }
            }
            if (closestDistance > PickupDistance * PickupDistance)
                TargetEntity = null;
            else
                TargetEntity.TouchFocus(PlayerColor);

            // Highlight fire and piece when close enough to burn a piece
            if (Carrying != null)
            {
                Carrying.Color = Color.White;
                Q19Game.Instance.Scene.Visit(e =>
                {
                    if (e is Fire f && f.CarriedBy == null &&
                        Vector2.DistanceSquared(Carrying.Position, f.Position) < Fire.FireTossDistance * Fire.FireTossDistance)
                    {
                        f.TouchFocus(PlayerColor);
                        Carrying.Color = Color.Red;
                    }
                });
            }

            _previousGamePadState = gamePad;
            _previousKeyboardState = kb;
            base.Update(gameTime);
        }

        private void RotateCarried(int direction)
        {
            _spinCarriedSound.Play();
            Carrying.Rotate(direction);
        }

        private void PickupOrDrop()
        {
            if (Carrying != null)
            {
                Carrying.OnDropped(this);
                if (Carrying is PuzzlePiece pl && !pl.IsPlacedOnGrid)
                {
                	Carrying.AnimateMove(Position + new Vector2(0, -10));
                    _dropItem.Play();
                }
                Carrying.Color = Color.White;
                Carrying = null;
                return;
            }

            if (TargetEntity != null)
            {
                Carrying = TargetEntity;
                Carrying.Depth = Depths.PlayerCarry;
                Carrying.OnPickedUp(this);
                _pickupItem.Play();
            }
        }

        private bool JustPressed(GamePadState current, Buttons button)
        {
            if (_disableInput)
                return false;
            return _previousGamePadState.IsButtonUp(button) && current.IsButtonDown(button);
        }

        private bool JustTriggered(GamePadState current, Func<GamePadTriggers, float> triggerRetriever)
        {
            if (_disableInput)
                return false;
            return triggerRetriever(_previousGamePadState.Triggers) < 0.2f && triggerRetriever(current.Triggers) >= 0.2f;
        }

        private bool JustPressed(KeyboardState current, Keys key1, Keys key2)
        {
            if (_disableInput)
                return false;
            if (Q19Game.DebugIsActivePlayer(Index) || Index == PlayerIndex.One)
                return _previousKeyboardState.IsKeyUp(key1) && current.IsKeyDown(key1);
            if (Index == PlayerIndex.Two)
                return _previousKeyboardState.IsKeyUp(key2) && current.IsKeyDown(key2);
            return false;
        }

        private Vector2 GetKeyboardMovement(KeyboardState kb, PlayerIndex index)
        {
            if (_disableInput)
                return Vector2.Zero;
            var movement = Vector2.Zero;
            if (index == PlayerIndex.One)
            {
                if (kb.IsKeyDown(Keys.Right)) movement += new Vector2(1, 0);
                if (kb.IsKeyDown(Keys.Left)) movement += new Vector2(-1, 0);
                if (kb.IsKeyDown(Keys.Up)) movement += new Vector2(0, -1);
                if (kb.IsKeyDown(Keys.Down)) movement += new Vector2(0, 1);
            }
            else
            {
                if (kb.IsKeyDown(Keys.D)) movement += new Vector2(1, 0);
                if (kb.IsKeyDown(Keys.A)) movement += new Vector2(-1, 0);
                if (kb.IsKeyDown(Keys.W)) movement += new Vector2(0, -1);
                if (kb.IsKeyDown(Keys.S)) movement += new Vector2(0, 1);
            }

            return movement;
        }

        private Vector2 GetGamePadMovement(GamePadState gamePad)
        {
            if (_disableInput && !gamePad.IsConnected)
                return Vector2.Zero;

            var left = gamePad.DPad.Left == ButtonState.Pressed ? -1 : 0;
            var right = gamePad.DPad.Right == ButtonState.Pressed ? 1 : 0;
            var up = gamePad.DPad.Up == ButtonState.Pressed ? -1 : 0;
            var down = gamePad.DPad.Down == ButtonState.Pressed ? 1 : 0;

            var dpadMove = new Vector2(left + right, up + down);
            var dpadMoveLength = dpadMove.Length();
            if(dpadMoveLength > 0.001f)
                dpadMove = Vector2.Normalize(dpadMove) / dpadMoveLength;
            
            var stick = gamePad.ThumbSticks.Left;
            var movement = new Vector2(stick.X, -stick.Y) + dpadMove;
            var movementLength = movement.Length();
            if(movementLength > 0.001f)
                movement = Vector2.Normalize(movement) / movementLength;

            return movement;
        }

        public void MoveTo(params Vector2[] targets)
        {
            Delay(Helpers.Rnd.Float() * 0.3f, () =>
            {
                _autoMoveTargets = targets.ToList();
                _disableInput = true;
            });
        }
    }
}
