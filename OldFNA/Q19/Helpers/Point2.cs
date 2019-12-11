using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using MSPoint = System.Drawing.Point;
using XNAPoint = Microsoft.Xna.Framework.Point;
using XNAColor = Microsoft.Xna.Framework.Color;

namespace jsmars.Game2D
{
    public struct Point2 : IComparable, IEqualityComparer<Point2>, IEquatable<Point2>
    {
        public static readonly Point2 Zero = new Point2();
        public static readonly Point2 One = new Point2(1, 1);
        public static readonly Point2 MinusOne = new Point2(-1, -1);
        public static readonly Point2 Null = new Point2(int.MaxValue, int.MinValue);
        public static readonly Point2 MaxValue = new Point2(int.MaxValue, int.MaxValue);
        public static readonly Point2 MinValue = new Point2(int.MinValue, int.MinValue);

        public int X;
        public int Y;

        public Point2(int xy)
        {
            X = xy;
            Y = xy;
        }
        public Point2(int x, int y)
        {
            X = x; Y = y;
        }

        public static Point2 operator *(Point2 a, Point2 b)
        {
            return new Point2(a.X * b.X, a.Y * b.Y);
        }
        public static Vector2 operator *(Point2 a, Vector2 b)
        {
            return new Vector2(a.X * b.X, a.Y * b.Y);
        }
        public static Vector2 operator *(Point2 a, float b)
        {
            return new Vector2(a.X * b, a.Y * b);
        }
        public static Point2 operator +(Point2 a, Point2 b)
        {
            return new Point2(a.X + b.X, a.Y + b.Y);
        }
        public static Point2 operator -(Point2 a, Point2 b)
        {
            return new Point2(a.X - b.X, a.Y - b.Y);
        }
        public static Point2 operator -(Point2 a)
        {
            return new Point2(-a.X, -a.Y);
        }
        public static Point2 operator *(Point2 a, int b)
        {
            return new Point2(a.X * b, a.Y * b);
        }
        public static Point2 operator /(Point2 a, int b)
        {
            return new Point2(a.X / b, a.Y / b);
        }
        public static Point2 operator /(Point2 a, Point2 b)
        {
            return new Point2(a.X / b.X, a.Y / b.Y);
        }
        public static Vector2 operator /(Point2 a, float b)
        {
            return new Vector2(a.X / b, a.Y / b);
        }
        public static bool operator ==(Point2 a, Point2 b)
        {
            return a.X == b.X && a.Y == b.Y;
        }
        public static bool operator !=(Point2 a, Point2 b)
        {
            return a.X != b.X || a.Y != b.Y;
        }

        public static int Manhattan(Point2 a, Point2 b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

        //public static implicit operator MSPoint(Point2 a)
        //{
        //    return new MSPoint(a.X, a.Y);
        //}
        //public static implicit operator Point2(MSPoint a)
        //{
        //    return new Point2(a.X, a.Y);
        //}
        public static implicit operator XNAPoint(Point2 a)
        {
            return new XNAPoint(a.X, a.Y);
        }
        public static implicit operator Point2(XNAPoint a)
        {
            return new Point2(a.X, a.Y);
        }

        public static implicit operator Vector2(Point2 a)
        {
            return new Vector2(a.X, a.Y);
        }

        public static explicit operator Point2(Vector2 a)
        {
            return new Point2((int)a.X, (int)a.Y);
        }

        public static explicit operator Vector3(Point2 a)
        {
            return new Vector3(a.X, a.Y, 0);
        }
        public static explicit operator Point2(Vector3 a)
        {
            return new Point2((int)Math.Floor(a.X), (int)Math.Floor(a.Y));
        }
        public static Point2 RoundV2(Vector2 vector)
        {
            return new Point2((int)Math.Round(vector.X, 0), (int)Math.Round(vector.Y, 0));
        }

        public override string ToString()
        {
            return string.Concat("{ ", X, ", ", Y, " }");
        }
        public string ToString(string format)
        {
            return string.Concat("{ ", X.ToString(format), ", ", Y.ToString(format), " }");
        }

        public int CompareTo(object obj)
        {
            return X.CompareTo(obj);
        }

        public bool Equals(Point2 x, Point2 y)
        {
            return x == y;
        }

        public int GetHashCode(Point2 obj)
        {
            return X.GetHashCode() + Y.GetHashCode();
        }

        public bool Equals(Point2 other)
        {
            return this == other;
        }

        /// <summary> Returns the length of this <see cref="Point2"/>. </summary>
        public float Length() => (float)Math.Sqrt((X * X) + (Y * Y));

        /// <summary> Returns the squared length of this <see cref="Point2"/>. </summary>
        public float LengthSquared() => (X * X) + (Y * Y);
    }
}
