using Microsoft.Xna.Framework;

namespace Q19
{
    public static class RectangleExtensions
    {
        public static bool Contains(this Rectangle rect, Vector2 position)
        {
            return rect.Contains(new Point((int) position.X, (int) position.Y));
        }
    }
}