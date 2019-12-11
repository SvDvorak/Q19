using System;
using System.Collections.Generic;
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

    static class PlayersExtensions
    {
        public static void TryDo(this List<Player> players, int index, Action<Player> action)
        {
            if (players.Count > index)
                action(players[index]);
        }

        public static void SetAndMoveTo(this List<Player> players, int index, Vector2 start, params Vector2[] points)
        {
            players.TryDo(index, p =>
            {
                p.Position = start;
                p.MoveTo(points);
            });
        }
    }
}