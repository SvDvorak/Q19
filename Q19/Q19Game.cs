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
using jsmars;

namespace Q19
{
    class Q19Game : Game
    {
        private Model _gameModel;

        public static bool DebugMode = false;
        public static bool DebugEasyMode = true; // 10% of normal light speed, hold T for normal
        public static int DebugInstantLevel = 0 +  // 0 for start, 1 for levels
                                              0 * 2 + // levelnum
                                              0; // 0 trans, 1 level
        public static string DebugBonus = "";
        public static Q19Game Instance { get; private set; }
        public static Texture2D Pixel { get; private set; }

        public GraphicsDeviceManager Graphics { get; private set; }
        public GraphicsDevice GD => Graphics.GraphicsDevice;
        public SpriteBatch SpriteBatch { get; private set; }
        public Scene Scene { get; set; }
        public List<Player> Players { get; } = new List<Player>();
        public int GameWidth { get; set; } = 1280;
        public int GameHeight { get; set; } = 720;
        public Matrix Camera2D { get; set; }
        public Lighting2D Lighting { get; private set; }
        public SpriteFont Font1 { get; private set; }
        public Color TextColor { get; } = Color.White;
        public bool DebugDrawing { get; set; }
        public DateTime StartTime { get; set; }

        RenderTarget2D gameFrame;
        MouseState mouseLast, mouseState;
        Rectangle gameFrameLocation;
        float gameFrameScale;

        #region debug

        Entity mouseCarryPiece;
        private bool _switchingScreenMode;
        private Point _previousResolution;
        private Vector2 _viewRotation;
        private Vector3 _cameraPosition;
        private Matrix _view = Matrix.Identity;

        #endregion

        public Q19Game()
        {
            StartTime = DateTime.Now;
            Instance = this;
            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Data";
            Window.AllowUserResizing = true;
            IsMouseVisible = DebugMode;
            Lighting = new Lighting2D(GD, GameWidth, GameHeight) { EnableDebugLight = false };
            Camera2D = Matrix.CreateTranslation(new Vector3(GameWidth / 2, GameHeight / 2, 0));
            Window.Title = "Q19";

            Graphics.PreferredBackBufferWidth = GameWidth;
            Graphics.PreferredBackBufferHeight = GameHeight;
            Graphics.ApplyChanges();

            _cameraPosition = Vector3.Down * 0.5f + Vector3.Forward * 2;

            //Scene = new Scene();
            //Highscore = new Highscore("Q19", 1);
        }

        protected override void LoadContent()
        {
            _gameModel = Content.Load<Model>("World/World");

            //SpriteBatch = new SpriteBatch(GraphicsDevice);
            gameFrame = new RenderTarget2D(GD, GameWidth, GameHeight);
            Pixel = new Texture2D(GraphicsDevice, 1, 1);
            Pixel.SetData(new Color[] { Color.White });
            //Font1 = Content.Load<SpriteFont>("Fonts/Font1");
            //Lighting.LoadContent(Content);
            //Jigsaw.Load(GD, Content);
            //Scene.LoadContent(Content);
        }

        protected override void Update(GameTime gameTime)
        {
            mouseState = Mouse.GetState();
            Input.Update(gameTime);

            var scaledViewMovement = MouseDelta * 0.01f;
            _viewRotation = new Vector2(_viewRotation.X + scaledViewMovement.X, MathHelper.Clamp(_viewRotation.Y + scaledViewMovement.Y, -MathHelper.PiOver2, MathHelper.PiOver2));

            var viewRotation = Matrix.CreateRotationY(_viewRotation.X) * Matrix.CreateRotationX(_viewRotation.Y);

            //if (Input.KeyDown(Keys.S))
                //_cameraPosition += Vector3.Transform(Vector3.Forward, viewRotation);

            _cameraPosition = Vector3.Up * (float) gameTime.TotalGameTime.TotalSeconds;
            _view = Matrix.CreateTranslation(_cameraPosition) * viewRotation;

            #region Fullscreening

            if (!_switchingScreenMode && Input.KeyDown(Keys.LeftAlt) && Input.KeyDown(Keys.Enter))
            {
                _switchingScreenMode = true;
                if (Window.IsBorderlessEXT)
                {
                    Window.IsBorderlessEXT = false;
                    Graphics.PreferredBackBufferWidth = _previousResolution.X;
                    Graphics.PreferredBackBufferHeight = _previousResolution.Y;
                }
                else
                {

                    _previousResolution =
                        new Point(Graphics.PreferredBackBufferWidth, Graphics.PreferredBackBufferHeight);
                    Window.IsBorderlessEXT = true;
                    Graphics.PreferredBackBufferWidth = GraphicsDevice.Adapter.CurrentDisplayMode.Width;
                    Graphics.PreferredBackBufferHeight = GraphicsDevice.Adapter.CurrentDisplayMode.Height;
                }

                Graphics.ApplyChanges();
            }

            if (_switchingScreenMode && Input.KeyUp(Keys.LeftAlt) && Input.KeyUp(Keys.Enter))
                _switchingScreenMode = false;

            #endregion

            base.Update(gameTime);
            mouseLast = mouseState;
        }

        private void DrawModel(Model m, GameTime time)
        {
            var transforms = new Matrix[m.Bones.Count];
            var aspectRatio = Graphics.GraphicsDevice.Viewport.AspectRatio;
            m.CopyAbsoluteBoneTransformsTo(transforms);
            var projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45.0f),
                aspectRatio,
                1.0f,
                10000.0f);
            //Matrix view = Matrix.CreateLookAt(new Vector3(0.0f, 50.0f, zoom),
            //    Vector3.Zero, Vector3.Up);

            foreach (var mesh in m.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();

                    effect.View = _view;
                    effect.Projection = projection;
                    effect.World = Matrix.CreateScale(0.01f) *
                                   transforms[mesh.ParentBone.Index];
                    //Matrix.CreateTranslation(_objectPosition);
                }
                mesh.Draw();
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            //Lighting.RenderLights(Camera2D, new Rectangle(-GameWidth / 2, -GameHeight / 2, GameWidth, GameHeight));

            // Draw game to a render target
            //GD.SetRenderTarget(gameFrame);
            //DrawGame(gameTime);
            //GD.SetRenderTarget(null);
            
            #region Black bar position
            
            var screenW = GD.PresentationParameters.BackBufferWidth;
            var screenH = GD.PresentationParameters.BackBufferHeight;

            if (screenW == GameWidth && screenH == GameHeight)
            {
                gameFrameLocation = new Rectangle(0, 0, gameFrame.Width, gameFrame.Height);
                gameFrameScale = 1;
            }
            else
            {
                var aspect = (float)GameWidth / GameHeight;

                var x = (float)screenW / GameWidth;
                var y = (float)screenH / GameHeight;

                if (x < y)
                {
                    var h = (int)(screenW / aspect);
                    gameFrameLocation = new Rectangle(0, screenH / 2 - h / 2, screenW, h);
                    gameFrameScale = screenW / (float)gameFrame.Width;
                }
                else
                {
                    var w = (int)(screenH * aspect);
                    gameFrameLocation = new Rectangle(screenW / 2 - w / 2, 0, w, screenH);
                    gameFrameScale = screenH / (float)gameFrame.Height;
                }
            }

            #endregion

            // Draw render target to screen in correct scale
            GraphicsDevice.Clear(Color.Black);
            //SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
            //SpriteBatch.Draw(gameFrame, gameFrameLocation, Color.White);
            DrawModel(_gameModel, gameTime);
            //SpriteBatch.End();

            base.Draw(gameTime);
        }

        protected void DrawGame(GameTime gt)
        {
            if (!DebugDrawing)
                Lighting.Draw(SpriteBatch, 1, 1);
        }

        public static bool DebugIsActivePlayer(PlayerIndex index)
        {
            return (int) index == DebugActivePlayer; // && IsDebug
        }

        private static int DebugActivePlayer { get; set; }

        public List<Entity> GetAllPickupEntities()
        {
            var pickupEntities = new List<Entity>();
            Scene.Visit(e =>
            {
                if (e.CanPickup)
                    pickupEntities.Add(e);
            });

            return pickupEntities;
        }

        public Vector2 MousePosition => Vector2.Transform(new Vector2((mouseState.X - gameFrameLocation.X) / gameFrameScale - 0, (mouseState.Y - gameFrameLocation.Y) / gameFrameScale - 0), Matrix.Invert(Camera2D));
        public Vector2 MouseDelta => new Vector2(mouseState.X - mouseLast.X, mouseState.Y - mouseLast.Y);
    }

    #region Entry

    static class Program
    {
        static void Main(string[] args)
        {
            using (Q19Game game = new Q19Game())
            {
                game.Run();
            }
        }
    }

    #endregion
}
