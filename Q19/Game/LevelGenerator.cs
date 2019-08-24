using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Q19
{
    static class LevelGenerator
    {
        static int maxCustomLevel = 16;

        public static LevelSettings Generate(int level)
        {
            var baseDecrease = 4.2f;

            if (level == 0)
                baseDecrease = 0.25f;

            return new LevelSettings(baseDecrease // Base decrease
                + level * -0.08f + // a bit easier per level since there are more pieces
                + Math.Max(0, (level - maxCustomLevel) * 0.4f)// a bit harder per level after level 14 so
                , 40, 180 - level * 3, genPuzzles(level));
        }

        static PuzzleSetup[] genPuzzles(int level)
        {
            var ps = LevelSettings.DefaultPieceSize;
            switch (level)
            {
                case  0: return new PuzzleSetup[] { new PuzzleSetup(2, 2, ps, rndPuzzlePath()) };
                case  1: return new PuzzleSetup[] { new PuzzleSetup(3, 3, ps, rndPuzzlePath()) };
                case  2: return new PuzzleSetup[] { new PuzzleSetup(3, 4, ps, rndPuzzlePath()) };
                case  3: return new PuzzleSetup[] { new PuzzleSetup(2, 2, ps, rndPuzzlePath()),
                                                    new PuzzleSetup(2, 2, ps, rndPuzzlePath()) };
                case  4: return new PuzzleSetup[] { new PuzzleSetup(5, 3, ps, rndPuzzlePath()) };
                case  5: return new PuzzleSetup[] { new PuzzleSetup(3, 3, ps, rndPuzzlePath()),
                                                    new PuzzleSetup(3, 3, ps, rndPuzzlePath()) };
                case  6: return new PuzzleSetup[] { new PuzzleSetup(4, 4, ps, rndPuzzlePath()) };
                case  7: return new PuzzleSetup[] { new PuzzleSetup(5, 5, ps, rndPuzzlePath()) };
                case  8: return new PuzzleSetup[] { new PuzzleSetup(4, 4, ps, rndPuzzlePath()),
                                                    new PuzzleSetup(4, 4, ps, rndPuzzlePath()) };
                case  9: return new PuzzleSetup[] { new PuzzleSetup(2, 2, ps, rndPuzzlePath()),
                                                    new PuzzleSetup(2, 2, ps, rndPuzzlePath()),
                                                    new PuzzleSetup(2, 2, ps, rndPuzzlePath()),
                                                    new PuzzleSetup(2, 2, ps, rndPuzzlePath()) };
                case 10: return new PuzzleSetup[] { new PuzzleSetup(5, 5, ps, rndPuzzlePath()) };
                case 11: return new PuzzleSetup[] { new PuzzleSetup(2, 2, ps, rndPuzzlePath()),
                                                    new PuzzleSetup(3, 3, ps, rndPuzzlePath()),
                                                    new PuzzleSetup(4, 4, ps, rndPuzzlePath()) };
                case 12: return new PuzzleSetup[] { new PuzzleSetup(4, 4, ps, rndPuzzlePath()),
                                                    new PuzzleSetup(4, 4, ps, rndPuzzlePath()),
                                                    new PuzzleSetup(4, 4, ps, rndPuzzlePath()) };
                case 13: return new PuzzleSetup[] { new PuzzleSetup(2, 2, ps, rndPuzzlePath()),
                                                    new PuzzleSetup(2, 2, ps, rndPuzzlePath()),
                                                    new PuzzleSetup(2, 2, ps, rndPuzzlePath()),
                                                    new PuzzleSetup(2, 2, ps, rndPuzzlePath()),
                                                    new PuzzleSetup(2, 2, ps, rndPuzzlePath()),
                                                    new PuzzleSetup(2, 2, ps, rndPuzzlePath()) };
                case 14: return new PuzzleSetup[] { new PuzzleSetup(6, 5, ps, rndPuzzlePath()) };
                case 15: return new PuzzleSetup[] { new PuzzleSetup(7, 5, ps, rndPuzzlePath()) };
                case 16: return new PuzzleSetup[] { new PuzzleSetup(8, 5, ps, rndPuzzlePath()) };

                    // update maxCustomLevel when changing number of premade levels
            }

            return genPuzzles(Helpers.Rnd.Next(8, 11));
        }

        static List<string> rndPool = new List<string>();
        static string rndPuzzlePath()
        {
            // Use a random pool so we cycle all puzzles before getting the same one again
            if (rndPool.Count == 0)
                for (int i = 1; i < 11; i++)
                    rndPool.Add($"Puzzles/serwaa{i}");
            var p = Helpers.Rnd.Next(0, rndPool.Count);
            var o = rndPool[p];
            rndPool.RemoveAt(p);
            return o;
        }

        public static void SetupPositions(GameLevel level)
        {
            var bounds = level.MainArea;

            switch (level.Puzzles.Length)
            {
                case 1:
                    level.Puzzles[0].Position = Vector2.Zero;
                    break;
                case 2:
                    level.Puzzles[0].Position = new Vector2(bounds.Width / -4, 0);
                    level.Puzzles[1].Position = new Vector2(bounds.Width / 4, 0);
                    break;
                case 3:
                    level.Puzzles[0].Position = new Vector2(bounds.Width / -3, 0);
                    level.Puzzles[1].Position = Vector2.Zero;
                    level.Puzzles[1].Position = new Vector2(bounds.Width / 3, 0);
                    break;
                case 4:
                    level.Puzzles[0].Position = new Vector2(bounds.Width / -4, bounds.Height / -5);
                    level.Puzzles[1].Position = new Vector2(bounds.Width / -4, bounds.Height / 5);
                    level.Puzzles[2].Position = new Vector2(bounds.Width / 4, bounds.Height / -5);
                    level.Puzzles[3].Position = new Vector2(bounds.Width / 4, bounds.Height / 5);
                    break;
                case 5:
                    level.Puzzles[0].Position = new Vector2(bounds.Width / -3, bounds.Height / -5);
                    level.Puzzles[1].Position = new Vector2(bounds.Width / -3, bounds.Height / 5);
                    level.Puzzles[2].Position = Vector2.Zero;
                    level.Puzzles[3].Position = new Vector2(bounds.Width / 3, bounds.Height / -5);
                    level.Puzzles[4].Position = new Vector2(bounds.Width / 3, bounds.Height / 5);
                    break;
                case 6:
                    level.Puzzles[0].Position = new Vector2(bounds.Width / -3, bounds.Height / -5);
                    level.Puzzles[1].Position = new Vector2(bounds.Width / -3, bounds.Height / 5);
                    level.Puzzles[2].Position = new Vector2(0, bounds.Height / -5);
                    level.Puzzles[3].Position = new Vector2(0, bounds.Height / 5);
                    level.Puzzles[4].Position = new Vector2(bounds.Width / 3, bounds.Height / -5);
                    level.Puzzles[5].Position = new Vector2(bounds.Width / 3, bounds.Height / 5);
                    break;

                default:
                    break;
            }
        }
    }

    class LevelSettings
    {
        public const int FireBonusFull = 15;
        public const int DefaultPieceSize = 30;
        public const int GameOverLightIncrease = 80;

        public LevelSettings(float lightDecreasePerSec, float burnIncrease, float maxLight, PuzzleSetup[] puzzles)
        {
            LightDecreasePerSec = lightDecreasePerSec; LightIncreasePerBurn = burnIncrease; MaxLight = maxLight; Puzzles = puzzles;
        }
        public float LightDecreasePerSec { get; private set; }
        public float LightIncreasePerBurn { get; private set; }
        public float MaxLight { get; private set; }
        public int Relics { get; private set; } = 6;
        public PuzzleSetup[] Puzzles { get; private set; }
    }

    class PuzzleSetup
    {
        public PuzzleSetup(int w, int h, int piecSize, string image)
        {
            Width = w; Height = h; PieceSize = piecSize; Image = image;
        }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int PieceSize { get; private set; }
        public string Image { get; private set; }
    }
}
