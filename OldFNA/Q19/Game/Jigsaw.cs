using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using jsmars.Game2D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Q19
{
    static class Jigsaw
    {
        static Effect effect;
        static GraphicsDevice gd;
        static Matrix mProj, mView;
        static VertexTex2[] vquad = new VertexTex2[4];
        static EffectParameter world, view, proj;

        public static void Load(GraphicsDevice gd, ContentManager content)
        {
            Jigsaw.gd = gd;
            effect = content.Load<Effect>("Shaders/JigsawCutout");
            effect.Parameters["Male"].SetValue(content.Load<Texture2D>("Puzzles/jigsaw_male"));
            effect.Parameters["Female"].SetValue(content.Load<Texture2D>("Puzzles/jigsaw_female"));
            effect.Parameters["Edge"].SetValue(content.Load<Texture2D>("Puzzles/jigsaw_edge"));

            world = effect.Parameters["World"];
            view = effect.Parameters["View"];
            proj = effect.Parameters["Projection"];
        }

        /// <summary> Creates a jigsaw from the given piece, resolution will be twice a normal square cutout</summary>
        public static RenderTarget2D RenderJigsaw(Point2 piecePosition, Texture2D puzzleTexture, Point2 puzzlePieces, Point2 puzzleRes, int[] edgeDataNSEW)
        {
            var pieceRes = puzzleRes / puzzlePieces;
            pieceRes *= 2; // make room for jigsaw edges
            var w = pieceRes.X;
            var h = pieceRes.Y;

            var output = new RenderTarget2D(gd, pieceRes.X, pieceRes.Y, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            gd.SetRenderTarget(output);
            gd.Clear(new Color(0, 0, 100, 100));
            gd.Clear(Color.Transparent);
            gd.RasterizerState = RasterizerState.CullCounterClockwise;

            var worldMatrix = Matrix.Identity;
            mProj = Matrix.CreateOrthographic(1, 1, 0, 1);
            //mView = Matrix.CreateLookAt(new Vector3(w / 2, h / 2, -1), new Vector3(w / 2, h / 2, 0), new Vector3(0, -1, 0));
            mView = Matrix.CreateLookAt(new Vector3(0.5f, 0.5f, -1), new Vector3(0.5f, 0.5f, 0), new Vector3(0, -1, 0));
            world.SetValue(worldMatrix);
            view.SetValue(mView);
            proj.SetValue(mProj);
            
            // draw puzzle texture
            gd.BlendState = BlendState.Additive;
            effect.Parameters["Texture"].SetValue(puzzleTexture);
            effect.Parameters["Edges"].SetValue(edgeDataNSEW);
            effect.Techniques[0].Passes[0].Apply();

            var onePiece = Vector2.One / (Vector2)puzzlePieces;
            var tl = onePiece * piecePosition - new Vector2(0.5f) * onePiece;
            var br = tl + onePiece * 2;
            
            DrawQuad(tl, br, new Vector2(-0.5f), new Vector2(1.5f));
            
            gd.SetRenderTarget(null);
            return output;
        }
        
        public static void DrawQuad(Vector2 uv0tl, Vector2 uv0br, Vector2 uv1tl, Vector2 uv1br)
        {
            vquad[0] = new VertexTex2(new Vector2(0, 0), uv0tl,                         uv1tl                            ); //TL
            vquad[1] = new VertexTex2(new Vector2(1, 0), new Vector2(uv0br.X, uv0tl.Y), new Vector2(uv1br.X, uv1tl.Y)    ); //TR
            vquad[2] = new VertexTex2(new Vector2(0, 1), new Vector2(uv0tl.X, uv0br.Y), new Vector2(uv1tl.X, uv1br.Y)    ); //BL
            vquad[3] = new VertexTex2(new Vector2(1, 1), uv0br,                         uv1br                            ); //BR
            gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, vquad, 0, 2);
        }
    }
    
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct VertexTex2 : IVertexType
    {
        public Vector2 Position;
        public Vector2 UV1;
        public Vector2 UV2;

        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }
        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(new VertexElement[]
            {
                new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
                new VertexElement(8, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
                new VertexElement(16, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1),
            });

        public VertexTex2(Vector2 position, Vector2 uv1, Vector2 uv2)
        {
            Position = position;
            UV1 = uv1;
            UV2 = uv2;
        }
    }
}
