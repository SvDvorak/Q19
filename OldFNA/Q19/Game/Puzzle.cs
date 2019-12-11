using System;
using System.Collections.Generic;
using System.Linq;
using jsmars.Game2D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Q19
{
    class Puzzle : Entity
    {
        public int PiecesX { get; private set; }
        public int PiecesY { get; private set; }
        public Point2 PiecesSize => new Point2(PiecesX, PiecesY);
        public int PiecesCount { get; private set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Point2 Size => new Point2(Width, Height);
        public bool IsComplete { get; private set; }
        public int ExtraPieces { get; private set; }

        SoundEffectInstance puzzleCompleteSound;
        List<PuzzlePiece>[,] pieces;
        Frame frame;

        public Puzzle(int piecesX, int piecesY, int width, int height, int extraPieces, string texturePath)
        {
            TexturePath = texturePath;
            PiecesX = piecesX;
            PiecesY = piecesY;
            Width = width;
            Height = height;
            ExtraPieces = extraPieces;
            PiecesCount = piecesX * piecesY;
            pieces = new List<PuzzlePiece>[piecesX, piecesY];
            Color = Color.Black;
            Depth = Depths.PuzzleBG;
        }

        public override void LoadContent(ContentManager content)
        {
            // create pieces
            for (int y = 0; y < PiecesY; y++)
                for (int x = 0; x < PiecesX; x++)
                    createPiece(x, y);

            for (int i = 0; i < ExtraPieces; i++)
            {
                var p = createPiece(Helpers.Rnd.Next(PiecesX), Helpers.Rnd.Next(PiecesY));
                p.IsExtra = true;
            }

            PuzzlePiece createPiece(int x, int y)
            {
                if (pieces[x, y] == null)
                    pieces[x, y] = new List<PuzzlePiece>();

                var piece = new PuzzlePiece(this, x, y);
                pieces[x, y].Add(piece);
                Add(piece);
                return piece;
            }

            puzzleCompleteSound = content.Load<SoundEffect>("Audio/PuzzleComplete").CreateInstance();

            base.LoadContent(content);
            Scale = new Vector2((float)Width / Texture.Width, (float)Height / Texture.Height);
            Add(new Frame(Bounds, content));
        }

        public void PlacePuzzlePieces()
        {
            foreach (var item in pieces)
                foreach (var piece in item)
                    piece.Position = GetGameLevel().GetFreePosition();
        }

        public void OnPiecePlaced()
        {
            var r = CheckCompleteRate();
            if (r >= 0.5f && Q19Game.Instance.Scene.Level is GameLevel gl)
                gl.CheckLevelReady();

            if (!IsComplete && CheckComplete())
            {
                puzzleCompleteSound.Play();
                //foreach (var item in pieces)
                //    foreach (var piece in item)
                //        if (piece.IsPlacedInPuzzle)
                //            piece.Color = Color.LightGreen;
                IsComplete = true;
            }
        }

        public bool CheckComplete() => PlacedPieces() == PiecesCount;
        public float CheckCompleteRate() => PlacedPieces() / (float)PiecesCount;
        public int PlacedPieces()
        {
            int missing = 0;
            for (int y = 0; y < PiecesY; y++)
                for (int x = 0; x < PiecesX; x++)
                    if (!HasPlacedPiece(x, y))
                        missing++;

            return PiecesCount - missing;
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime)
        {
            //sb.Draw(Texture, new Rectangle(-Q19Game.Instance.GameWidth / 2 + 4, Q19Game.Instance.GameHeight / 2 - 104, 100, 100), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, Depth);
            Color = Color.Transparent;
            base.Draw(sb, gameTime);
        }

        public bool HasPlacedPiece(int x, int y)
        {
            foreach (var item in pieces[x, y])
                if (item.IsPlacedInPuzzle)
                    return true;
            return false;
        }

        public PuzzlePiece GetPlacedPiece(int x, int y)
        {
            foreach (var item in pieces[x, y])
                if (item.IsPlacedInPuzzle)
                    return item;
            return null;
        }

        /// <summary> Checks if the position has any other correctly placed pieces and locks them if it's true, since this should be used to lock something that is placed here</summary>
        public bool TryLockPosition(int x, int y, bool lockConnecting)
        {
            if (check(x - 1, y) ||
                check(x + 1, y) ||
                check(x, y + 1) ||
                check(x, y - 1))
            {
                if (lockConnecting)
                {
                    lockPos(x - 1, y);
                    lockPos(x + 1, y);
                    lockPos(x, y + 1);
                    lockPos(x, y - 1);
                }
                return true;
            }
            return false;

            bool check(int X, int Y) => 
                X >= 0 && Y >= 0 && X < PiecesX && Y < PiecesY && HasPlacedPiece(X, Y);
            void lockPos(int X, int Y)
            {
                if (X >= 0 && Y >= 0 && X < PiecesX && Y < PiecesY)
                {
                    var p = GetPlacedPiece(X, Y);
                    if (p != null)
                        p.CanPickup = false;
                }
            }
        }
    }
}
