using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Q19
{
    public static class Input
    {
        private static KeyboardState kbLast, kbState;

        public static void Update(GameTime gt)
        {
            kbLast = kbState;
            kbState = Keyboard.GetState();
        }

        public static bool KeyPressed(Keys key) => kbLast.IsKeyUp(key) && kbState.IsKeyDown(key);
        public static bool KeyReleased(Keys key) => kbLast.IsKeyDown(key) && kbState.IsKeyUp(key);
        public static bool KeyDown(Keys key) => kbState.IsKeyDown(key);
        public static bool KeyUp(Keys key) => kbState.IsKeyUp(key);
    }
}
