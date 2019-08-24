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
    public static class Helpers
    {
        public static Random Rnd = new Random();
        public static float Float(this Random rnd) => (float)rnd.NextDouble();
        public static float Float(this Random rnd, float max) => (float)(rnd.NextDouble() * max);
        public static float Float(this Random rnd, float min, float max) => (float)(rnd.NextDouble() * (max - min) + min);
        public static Vector2 Vector2(this Random rnd, float minX, float maxX, float minY, float maxY) 
            => new Vector2((float)(Rnd.NextDouble() * (maxX - minX) + minX), (float)(Rnd.NextDouble() * (maxY - minY) + minY));

        public static T FromList<T>(this Random Random, List<T> List) => List[Rnd.Next(List.Count)];

        public static Point ToPoint(this Vector2 v) => new Point((int)v.X, (int)v.Y);
        public static Vector2 ToDirectionVector(this float radians) => new Vector2((float)Math.Sin(radians), (float)Math.Cos(radians));

        public static int Clamp(int value, int min, int max)
        {
            if (value < min)
                value = min;
            if (value > max)
                value = max;

            return value;
        }


        public static bool IsPressingAnyButton()
        {
            var kb = Keyboard.GetState();
            var pressedKeys = kb.GetPressedKeys();
            return pressedKeys
                       .Any() ||
                   Enumerable.Range(0, 4)
                       .Select(index => GamePad.GetState((PlayerIndex)index))
                       .Any(IsUsingGamepad);
        }

        private static bool IsUsingGamepad(GamePadState gamepad)
        {
            return
                gamepad.Buttons.A == ButtonState.Pressed ||
                gamepad.Buttons.B == ButtonState.Pressed ||
                gamepad.Buttons.X == ButtonState.Pressed ||
                gamepad.Buttons.Y == ButtonState.Pressed ||
                gamepad.Buttons.LeftShoulder == ButtonState.Pressed ||
                gamepad.Buttons.RightShoulder == ButtonState.Pressed ||
                gamepad.Buttons.Back == ButtonState.Pressed ||
                gamepad.Buttons.Start == ButtonState.Pressed ||
                gamepad.Buttons.Back == ButtonState.Pressed ||
                gamepad.Triggers.Left > 0.1 ||
                gamepad.Triggers.Right > 0.1f;
        }

        public static void DrawWithShadow(SpriteBatch sb, SpriteFont font, string text, Vector2 position, float scaling)
        {
            DrawWithShadow(sb, font, text, position, scaling, Q19Game.Instance.TextColor);
        }
        public static void DrawWithShadow(SpriteBatch sb, SpriteFont font, string text, Vector2 position, float scaling, Color color)
        {
            var scale = 1.0f * scaling;
            var origin = font.MeasureString(text) / 2;
            sb.DrawString(font, text, position + new Vector2(1), Color.Black, 0, origin, scale, SpriteEffects.None, Depths.MenuShadow);
            sb.DrawString(font, text, position, color, 0, origin, scale, SpriteEffects.None, Depths.Menu);
        }
    }

    public static class Depths
    {
        public static float ScreenFade = 0;
        public static float Menu = 0.02f;
        public static float MenuShadow = 0.025f;
        public static float PlayerCarry = 0.09f;
        public static float DoorOver = 0.09f;
        public static float Players = 0.1f;
        public static float Fire = 0.3f;
        public static float PuzzlePiece = 0.4f;
        public static float PuzzlePieceLocked = 0.5f;
        public static float PuzzleBG = 0.6f;
        public static float DoorUnder = 0.6f;
        public static float Background = 1.0f;


        public static float PuzzlePieceIncrement => ppInc -= 0.000001f;
        static float ppInc = PuzzlePiece;
    }
}
