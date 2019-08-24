using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Runtime.InteropServices;
using Q19;

namespace jsmars.Game2D
{
    ///TODO:    
    ///     backface culling on vertices
    ///     Proper light values
    ///     Cleanup
    ///     LightObj needed?
    ///     Directional broken
    ///     hack-ish solution for circles for cheap and exact circles? (sprite + only vertexes on edges?)
    ///    
    ///     Features:
    ///         Smooth shadows (possibly with shader pass? google it)
    ///         Shadow color (with opacity) to be able to add subtle shadows
    ///         


    public class Lighting2D //TODO: Make Entity? Required for 2D lighting, or is there a way to make manager components added automatically when needed?
    {
        public delegate void PostRenderEvent(RenderTarget2D target, GraphicsDevice gd, Effect effect);

        public int Width { get; private set; }
        public int Height { get; private set; }
        public List<Light2D> Lights { get; private set; }
        public List<IVertexObject> VertexObjs { get; private set; }
        public RenderTarget2D Target { get; private set; }
        public PostRenderEvent PostRender;
        /// <summary> Adds a light to the mouse input, <see cref="UpdateDebugInput(Matrix, Keys)"/> must be called for it to work. </summary>
        public bool EnableDebugLight { get; set; } = true;
        public GraphicsDevice Graphics { get; set; }

        Matrix mProj, mView;
        Effect effect;
        EffectParameter lightPos, lightColor, lightFalloff, lightFalloff2, lightSize, lightAngle, lightAngleInner, lightDirection, world, proj, view;
        VertexPosition2[] vquad;

        #region Stencil & Blending states

        // Shadow (Stencil) drawing
        DepthStencilState stencilOnly = new DepthStencilState()
        {
            StencilEnable = true,
            StencilFunction = CompareFunction.Always,
            StencilPass = StencilOperation.Replace,
            StencilFail = StencilOperation.Keep,
            StencilDepthBufferFail = StencilOperation.Keep,
            DepthBufferEnable = false,
            ReferenceStencil = 1,
        };
        BlendState blendStencil = new BlendState()
        {
            ColorWriteChannels = ColorWriteChannels.None
        };

        // Light drawing
        DepthStencilState stencilNormal = new DepthStencilState()
        {
            StencilEnable = true,
            StencilFunction = CompareFunction.Equal,
            StencilFail = StencilOperation.Keep,
            StencilPass = StencilOperation.Keep,
            StencilDepthBufferFail = StencilOperation.Keep,
        };
        BlendState blendNormal = BlendState.Additive;

        BlendState blendOverlay = new BlendState()
        {
            AlphaSourceBlend = Blend.DestinationAlpha,
            AlphaDestinationBlend = Blend.Zero,
            AlphaBlendFunction = BlendFunction.Add,
            ColorSourceBlend = Blend.DestinationColor,
            ColorDestinationBlend = Blend.Zero,
            ColorBlendFunction = BlendFunction.Add
        };

        #endregion

        public Lighting2D(GraphicsDevice gd, int width, int height)
        {
            Graphics = gd;
            Resize(width, height);
            Lights = new List<Light2D>();
            VertexObjs = new List<IVertexObject>();
        }

        public void Resize(int width, int height)
        {
            Width = width;
            Height = height;

            mProj = Matrix.CreateOrthographic(Width, Height, 0, 1);
            mView = Matrix.CreateLookAt(new Vector3(Width / 2, Height / 2, -1), new Vector3(Width / 2, Height / 2, 0), new Vector3(0, -1, 0));

            if (Target != null)
                Target.Dispose();
            Target = new RenderTarget2D(Graphics, Width, Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
        }

        public void LoadContent(ContentManager content)
        {
            vquad = new VertexPosition2[4];

            // Effect & Parameters
            effect = content.Load<Effect>("Shaders/Lighting2D");
            lightPos = effect.Parameters["lightPos"];
            lightColor = effect.Parameters["lightColor"];
            lightFalloff = effect.Parameters["lightFalloff"];
            lightFalloff2 = effect.Parameters["lightFalloff2"];
            lightSize = effect.Parameters["lightSize"];
            lightAngle = effect.Parameters["lightAngle"];
            lightAngleInner = effect.Parameters["lightAngleInner"];
            lightDirection = effect.Parameters["lightDirection"];
            world = effect.Parameters["World"];
            proj = effect.Parameters["Projection"];
            view = effect.Parameters["View"];
        }

        public void RenderLights(Matrix worldMatrix, AABB lightQuad)
        {
            var gd = Graphics;
            shadowcasts = shadowcastQuads = 0;

            // Setup rendertarget
            gd.SetRenderTarget(Target);
            gd.Clear(Color.Transparent);
            gd.RasterizerState = RasterizerState.CullNone;

            // Set matrices
            world.SetValue(worldMatrix);
            view.SetValue(mView);
            proj.SetValue(mProj);

            // Draw lights
            for (int i = Lights.Count - 1; i >= 0; i--)
                drawLight(Lights[i], ref lightQuad);
            
            // Add postprocess to shadows if there is some
            if (PostRender != null)
                PostRender(Target, gd, effect);

            // render the lights to the rendertarget
            gd.SetRenderTarget(null);

            //Ward.Watch("Lighting2D.Lights/VertexObjs", string.Concat(Lights.Count, "/", VertexObjs.Count));
            //Ward.Watch("Lighting2D.Shadowcasts/DrawQuads (quadsPer)", string.Concat(shadowcasts, " / ", shadowcastQuads, " (", (shadowcastQuads / (float)shadowcasts).ToString("0.0"), ")"));
        }

        int shadowcasts;
        int shadowcastQuads;
        private void drawLight(Light2D light, ref AABB lightQuad)
        {
            //TODO: Move vertex calculation to seperate update and draw when using for split screen
            if (!light.Enabled) return;
            var gd = Graphics;

            // Light parameters
            lightPos.SetValue(light.Position);
            lightColor.SetValue(light.Color);
            lightFalloff.SetValue(light.Falloff);
            lightFalloff2.SetValue(light.Falloff2);
            lightSize.SetValue(light.Size);
            lightDirection.SetValue(light.Direction.ToDirectionVector());
            lightAngle.SetValue(light.AngleOuter / 360f);
            lightAngleInner.SetValue(light.AngleInner / 360f);


            if (light.CastShadows)
            {
                // Calculate shadows from vertices and draw to stencil
                gd.DepthStencilState = stencilOnly;
                gd.BlendState = blendStencil;
                
                effect.Techniques[0].Passes[0].Apply(); // Stencil only simple pixelshader

                foreach (var obj in VertexObjs)
                {
                    shadowcasts++;
                    Vector2[] vertices = obj.GetVertices();
                    for (int v = 0; v < vertices.Length; v++)
                    {
                        Vector2 currentVertex = vertices[v];
                        Vector2 nextVertex = vertices[(v + 1) % vertices.Length];
                        /// Doubles the amount of vertices draw, but below code doesnt work to remove backfaced vertices
                        /// Maybe its cheaper to skip all these checks and just send double count of vertices?
                        /// Should probably find proper solution to cull backsides (test by placing light within object)
                        //Vector2 edge = Vector2.Subtract(nextVertex, currentVertex);
                        //Vector2 normal = new Vector2(edge.Y, -edge.X);
                        //Vector2 normal = Vector2.Normalize(edge);
                        //Vector2 lightToCurrent = Vector2.Subtract(currentVertex, light.Position);
                        //if (Vector2.Dot(normal, lightToCurrent) > 0)
                            DrawQuad(Vector2.Add(currentVertex, Vector2.Subtract(currentVertex, light.Position) * Width * 10),
                                     currentVertex,
                                     Vector2.Add(nextVertex, Vector2.Subtract(nextVertex, light.Position) * Width * 10),
                                     nextVertex);
                    }
                }
            }

            // Disable stencil buffer and draw to color
            gd.DepthStencilState = stencilNormal;
            gd.BlendState = blendNormal;

            // Draw light on quad where stencil is empty
            effect.Techniques[0].Passes[(int)light.Type + 1].Apply(); // Choose light pixel shader (based on light type)
            DrawQuad(new Vector2(lightQuad.Left, lightQuad.Top), new Vector2(lightQuad.Left, lightQuad.Bottom), new Vector2(lightQuad.Right, lightQuad.Top), new Vector2(lightQuad.Right, lightQuad.Bottom));

            // Clear stencil for next light
            gd.Clear(ClearOptions.Stencil, Color.Transparent, 0, 0);
        }

        public void DrawQuad(Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3)
        {
            shadowcastQuads++;
            vquad[0] = new VertexPosition2(v0);
            vquad[1] = new VertexPosition2(v2); // rearrange order to suit top-left positioning with proper order and cullmode
            vquad[2] = new VertexPosition2(v1);
            vquad[3] = new VertexPosition2(v3);
            Graphics.DrawUserPrimitives<VertexPosition2>(PrimitiveType.TriangleStrip, vquad, 0, 2);
        }

        /// <summary>
        /// Draws the RenderTarget map twice using spritebatch, first using overlay for shadows, then addative for lights. Disable either by setting value to 0
        /// </summary>
        public void Draw(SpriteBatch sb, float lightValue, float shadowValue)
        {
            if (shadowValue > 0)
            {
                sb.Begin(SpriteSortMode.Immediate, blendOverlay, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null);
                sb.Draw(Target, Vector2.Zero, null, new Color(shadowValue, shadowValue, shadowValue, shadowValue), 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                sb.End();
            }

            if (lightValue > 0)
            {
                lightValue = 0.1f;
                sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null);
                sb.Draw(Target, Vector2.Zero, null, new Color(lightValue, lightValue, lightValue, lightValue), 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                sb.End();
            }
        }

        #region Debug Input

        Light2D debugLight;
        public void UpdateDebugInput(Matrix worldMatrix, Keys toggleKey = Keys.L)
        {
            //if (!Input.GameIsActive) return;

            //if (Input.KeyPressed(toggleKey))
            //    EnableDebugLight = !EnableDebugLight;

            if (EnableDebugLight)
            {
                if (debugLight == null)
                    debugLight = new Light2D();

                if (!Lights.Contains(debugLight))
                    Lights.Add(debugLight);

                debugLight.Position = Q19.Q19Game.Instance.MousePosition;// Vector2.Transform(Game.Frames.Main.TransformScreenToViewport(Input.MousePos), Matrix.Invert(worldMatrix));  //TODO: Should use the current frame
            }
            else 
            {
                if (Lights.Contains(debugLight))
                    Lights.Remove(debugLight);
                return;
            }

            //Ward.Watch("Lighting2D.lights", Lights.Count);
            //Ward.Watch("Lighting2D.objs", VertexObjs.Count);
            //Ward.Watch("Lighting2D.debuglight", debugLight.ToString());

            //var scrollDelta = Input.States.MouseScrollDelta;
            //if (scrollDelta != 0)
            //{
                //var hsl = new Color(debugLight.Color);
                //hsl.SetBrightness(0.5f);
                //if (Input.KeyDown(Keys.LeftAlt))
                //    hsl.SetSaturation(hsl.GetSaturation() + scrollDelta * 0.025f);
                //else if (Input.KeyDown(Keys.LeftShift))
                //    hsl.SetHue(hsl.GetHue() + scrollDelta * 0.025f);
                //else if (Input.KeyDown(Keys.LeftControl))
                //    debugLight.Falloff = debugLight.Falloff * (1 + scrollDelta * 0.1f);
                //else if (Input.KeyDown(Keys.Z))
                //    debugLight.AngleOuter += scrollDelta * 2f;
                //else if (Input.KeyDown(Keys.X))
                //    debugLight.AngleInner += scrollDelta * 2f;
                //else if (Input.KeyDown(Keys.C))
                //    debugLight.Direction += scrollDelta * 0.05f;
                //else
                //    debugLight.Size = debugLight.Size * (1 + scrollDelta * 0.1f);

            //    debugLight.Color = hsl.ToVector4();
            //}

            //if (Input.KeyPressed(Keys.K))
            //{
            //    if (Input.KeyPressed(Keys.D1)) debugLight.Type = Light2D.LightType.Omni;
            //    if (Input.KeyPressed(Keys.D2)) debugLight.Type = Light2D.LightType.Cone;
            //    if (Input.KeyPressed(Keys.D3)) debugLight.Type = Light2D.LightType.Directional;
            //}

            //if (Input.KeyPressed(MouseKeys.Left))
            //    Lights.Insert(0, debugLight.Clone());
            //if (lights.Count > 0)
            //    lights[0].Position = (Point2)Input.MousePos;


        }

        #endregion
    }
    
    public class Light2D //TODO: Make Entity2D
    {
        public enum LightType { Omni, Cone, Directional }

        public bool Enabled = true;
        public bool CastShadows = true;
        public Vector2 Position;
        public Vector4 Color = new Vector4(1, 1, 1, 1);
        public float Falloff = 0.3f;
        public float Falloff2 = 0;
        public float Size = 3;
        public float Direction;
        public float AngleOuter { get { return angleOuter; } set { angleOuter = MathHelper.Clamp(value, 0, 360); angleInner = MathHelper.Clamp(angleInner, 0, angleOuter); } }
        public float AngleInner { get { return angleInner; } set { angleInner = MathHelper.Clamp(value, 0, 360); angleOuter = MathHelper.Clamp(angleOuter, angleInner, 360); } }
        public LightType Type;
        public Light2D Clone() { return (Light2D)this.MemberwiseClone(); }
        public object Tag;

        private float angleInner = 45, angleOuter = 65;

        public override string ToString() { return string.Concat("Light2D Type: ", Type, " Falloff: ", Falloff, " Size: ", Size, " Direction: ", Direction, " Angle: ", AngleOuter, " AngleInner: ", AngleInner); }
    }

    public interface IVertexObject
    {
        Vector2[] GetVertices();
    }

    #region Prebuilt Vertex objects

    public class VertexRectangle : IVertexObject
    {
        public Vector2 Position
        {
            get { return position; }
            set
            {
                if (value != position)
                {
                    dirtyVerts = true;
                    position = value;
                }
            }
        }
        public Vector2 Size
        {
            get { return size; }
            set
            {
                if (value != size)
                {
                    dirtyVerts = true;
                    size = value;
                    origin = size / 2;
                }
            }
        }
        public float Rotation
        {
            get { return rotation; }
            set
            {
                if (value != rotation)
                {
                    dirtyVerts = true;
                    rotation = value;
                }
            }
        }

        private float rotation;
        private Vector2 origin;
        private Vector2 size;
        private Vector2 position;
        private Vector2[] verts;
        private bool dirtyVerts = true;

        public Vector2[] GetVertices()
        {
            if (dirtyVerts)
            {
                if (verts == null)
                    verts = new Vector2[4];

                if (rotation == 0)
                {
                    verts[0] = new Vector2(Position.X - origin.X, Position.Y - origin.Y);
                    verts[1] = new Vector2(Position.X - origin.X, Position.Y + Size.Y - origin.Y);
                    verts[2] = new Vector2(Position.X + Size.X - origin.X, Position.Y + Size.Y - origin.Y);
                    verts[3] = new Vector2(Position.X + Size.X - origin.X, Position.Y - origin.Y);
                }
                else
                {
                    var rot = Matrix.CreateRotationZ(rotation);
                    verts[0] = Position + Vector2.Transform(-origin, rot);
                    verts[1] = Position + Vector2.Transform(new Vector2(-origin.X, -origin.Y + Size.Y), rot);
                    verts[2] = Position + Vector2.Transform(new Vector2(-origin.X + Size.X, -origin.Y + Size.Y), rot);
                    verts[3] = Position + Vector2.Transform(new Vector2(-origin.X + Size.X, -origin.Y), rot);
                }
            }
            return verts;
        }
    }

    public class VertexCircle : IVertexObject
    {
        public Vector2 Position
        {
            get { return position; }
            set { 
                if (value != position) 
                {
                    dirtyVerts = true;
                    position = value; 
                }
            }
        }
        public float Radius
        {
            get { return radius; }
            set { 
                if (value != radius)
                {
                    dirtyVerts = true;
                    radius = value; 
                }
            }
        }
        public int Segments
	    {
		    get { return segments;}
            set { 
                if (value != segments)
                {
                    dirtyVerts = true;
                    segments = value; 
                }
            }
	    }
        public float Rotation
        {
            get { return rotation; }
            set
            {
                if (value != rotation)
                {
                    dirtyVerts = true;
                    rotation = value;
                }
            }
        }

        private float radius = 1;
        private Vector2 position;
        private Vector2[] verts;
        private bool dirtyVerts = true;
        private int segments = 8;
        private float rotation;

        public Vector2[] GetVertices()
        {
            if (dirtyVerts)
            {
                if (verts == null || segments != verts.Length)
                    verts = new Vector2[segments];

                var f = MathHelper.TwoPi / segments;
                if (rotation == 0)
                {
                    for (int i = 0; i < segments; i++)
                    {
                        var r = f * i;
                        verts[i] = new Vector2(position.X + (float)Math.Sin(r) * radius, position.Y + (float)Math.Cos(r) * radius);
                    }
                }
                else
                {
                    var rot = Matrix.CreateRotationZ(rotation);
                    for (int i = 0; i < segments; i++)
                    {
                        var r = f * i;
                        verts[i] = Position + Vector2.Transform(new Vector2((float)Math.Sin(r) * radius, (float)Math.Cos(r) * radius), rot);
                    }
                }
                dirtyVerts = false;
            }

            return verts;
        }
    }

    public class VertexPolygon : IVertexObject
    {
        public Vector2 Position
        {
            get { return position; }
            set
            {
                if (value != position)
                {
                    dirtyVerts = true;
                    position = value;
                }
            }
        }
        public Vector2[] LocalVertices
        {
            get { return vertsLocal; }
            set
            {
                if (value != vertsLocal)
                {
                    dirtyVerts = true;
                    vertsLocal = value;
                }
            }
        }
        public float Rotation
        {
            get { return rotation; }
            set
            {
                if (value != rotation)
                {
                    dirtyVerts = true;
                    rotation = value;
                }
            }
        }

        private Vector2 position;
        private Vector2[] vertsLocal;
        private Vector2[] verts;
        private bool dirtyVerts = true;
        private float rotation;

        public Vector2[] GetVertices()
        {
            if (dirtyVerts)
            {
                if (verts == null || vertsLocal.Length != verts.Length)
                    verts = new Vector2[vertsLocal.Length];

                if (rotation == 0)
                {
                    for (int i = 0; i < vertsLocal.Length; i++)
                        verts[i] = new Vector2(Position.X + vertsLocal[i].X, position.Y + vertsLocal[i].Y);
                }
                else
                {
                    var rot = Matrix.CreateRotationZ(rotation);
                    for (int i = 0; i < vertsLocal.Length; i++)
                        verts[i] = Position + Vector2.Transform(vertsLocal[i], rot);
                }

                dirtyVerts = false;
            }

            return verts;
        }

        public static VertexPolygon CreateStar(Vector2 position, int points, float radiusInner, float radiusOuter)
        {
            if (points < 2) throw new Exception("Must have atleast 2 points");

            Vector2[] verts = new Vector2[points * 2];

            var r = MathHelper.TwoPi / (points * 2);
            for (int i = 0; i < points * 2; i++)
            {
                verts[i] = new Vector2((float)Math.Sin(i * r) * radiusOuter, (float)Math.Cos(i * r) * radiusOuter);
                i++;
                verts[i] = new Vector2((float)Math.Sin(i * r) * radiusInner, (float)Math.Cos(i * r) * radiusInner);
            }
            return new VertexPolygon()
            {
                Position = position,
                LocalVertices = verts,
            };
        }

        public static VertexPolygon CreateGear(Vector2 position, int gears, float radiusInner, float radiusOuter, float gearRatio = 0.5f, float gearBevel = 0.15f)
        {
            if (gears < 2) throw new Exception("Must have atleast 2 points");

            Vector2[] verts = new Vector2[gears * 4];

            var r = MathHelper.TwoPi / gears;
            var r2 = r * gearRatio;
            var g1 = r2 * gearBevel;
            var g2 = r2 - g1;
            var v = 0;
            for (int i = 0; i < gears; i++)
            {
                verts[v++] = new Vector2((float)Math.Sin(r * i) * radiusInner, (float)Math.Cos(r * i) * radiusInner);
                verts[v++] = new Vector2((float)Math.Sin(r * i + g1) * radiusOuter, (float)Math.Cos(r * i + g1) * radiusOuter);
                verts[v++] = new Vector2((float)Math.Sin(r * i + g2) * radiusOuter, (float)Math.Cos(r * i + g2) * radiusOuter);
                verts[v++] = new Vector2((float)Math.Sin(r * i + r2) * radiusInner, (float)Math.Cos(r * i + r2) * radiusInner);
            }
            return new VertexPolygon()
            {
                Position = position,
                LocalVertices = verts,
            };
        }
    }

    /// <summary>
    /// VertexPosition2 is a very small vertex type holding only a Vector2 value for position
    /// </summary>
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct VertexPosition2 : IVertexType
    {
        public Vector2 Position;

        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }
        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(new VertexElement[]
            {
                new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
            });

        public VertexPosition2(Vector2 position)
        {
            this.Position = position;
        }

        public override string ToString()
        {
            return "VertexPosition2: " + Position;
        }

        public static bool operator ==(VertexPosition2 left, VertexPosition2 right) { return left.Position == right.Position; }
        public static bool operator !=(VertexPosition2 left, VertexPosition2 right) { return !(left == right); }
        public override int GetHashCode() { return Position.GetHashCode(); } //TODO: Correct?
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != base.GetType()) return false;
            return (this == ((VertexPosition2)obj));
        }
    }

    #endregion

}