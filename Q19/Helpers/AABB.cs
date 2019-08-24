using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace jsmars.Game2D
{
    #region AABB

    public struct AABB
    {
        public Vector2 Position { get { return Min; } set { var s = Size; Min = value; Max = Min + s; } }
        public Vector2 Min;
        public Vector2 Max;

        public float Left { get { return Min.X; } }
        public float Right { get { return Max.X; } }
        public float Top { get { return Min.Y; } }
        public float Bottom { get { return Max.Y; } }
        public float Width { get { return Max.X - Min.X; } set { Max.X = Min.X + value; } }
        public float Height { get { return Max.Y - Min.Y; } set { Max.Y = Min.Y + value; } }
        public Vector2 Size { get { return new Vector2(Max.X - Min.X, Max.Y - Min.Y); } set { Max = new Vector2(Min.X + value.X, Min.Y + value.Y); } }
        public float X { get { return Min.X; } }
        public float Y { get { return Min.Y; } }
        public Vector2 Center { get { return new Vector2(Min.X + Width / 2, Min.Y + Height / 2); } }
        public float MaxEdgeLength => Width > Height ? Width : Height;

        public Vector2 TopRight { get { return new Vector2(Max.X, Min.Y); } }
        public Vector2 BottomLeft { get { return new Vector2(Min.X, Max.Y); } }
        public Vector2 LeftCenter { get { return Min + new Vector2(0, Height / 2); } }
        public Vector2 RightCenter { get { return Max + new Vector2(0, -Height / 2); } }
        public Vector2 TopCenter { get { return Min + new Vector2(Width / 2, 0); } }
        public Vector2 BottomCenter { get { return Max + new Vector2(-Width / 2, 0); } }

        public float AbsWidth { get { var w = Width; if (w < 0) return -w; return w; } }
        public float AbsHeight { get { var h = Height; if (h < 0) return -h; return h; } }

        //private AABB(Vector2 min, Vector2 max, bool Imgay)
        //{
        //    Min = min;
        //    Max = max;
        //}

        public AABB(Vector2 pos, Vector2 size)
        {
            Min = pos;
            Max = pos + size;
        }
        public AABB(float x, float y, float W, float H)
        {
            Min = new Vector2(x, y);
            Max = new Vector2(x + W, y + H);
        }

        /// <summary> Makes sure the width and height are positive values. </summary>
        public AABB Validate()
        {
            if (Width < 0)
            {
                var t = Min.X;
                Min.X = Max.X;
                Max.X = t;
            }
            if (Height < 0)
            {
                var t = Min.Y;
                Min.Y = Max.Y;
                Max.Y = t;
            }
            return this;
        }

        public override string ToString()
        {
            return string.Concat("AABB { x: ", Min.X, ", y: ", Min.Y, ", w: ", Width, ", h: ", Height, " }");
        }

        public bool Contains(Vector2 point)
        {
            if (point.X < Left) return false;
            if (point.X > Right) return false;
            if (point.Y < Top) return false;
            if (point.Y > Bottom) return false;
            return true;
        }
        public bool Contains(AABB rectangle)
        {
            if (rectangle.Left < Left) return false;
            if (rectangle.Right > Right) return false;
            if (rectangle.Top < Top) return false;
            if (rectangle.Bottom > Bottom) return false;
            return true;
        }
        public bool Contains(Rectangle rectangle)
        {
            if (rectangle.Left < Left) return false;
            if (rectangle.Right > Right) return false;
            if (rectangle.Top < Top) return false;
            if (rectangle.Bottom > Bottom) return false;
            return true;
        }
        //public bool Contains(Line line)
        //{
        //    return Contains(line.Start) && Contains(line.End);
        //}

        public bool Intersects(Vector2 point)
        {
            if (point.X < Left) return false;
            if (point.X > Right) return false;
            if (point.Y < Top) return false;
            if (point.Y > Bottom) return false;
            return true;
        }
        //public bool Intersects(Vector2 lineA, Vector2 lineB)
        //{
        //    //TODO: This probably isn't the cheapest way? If SimpleLine is still reference type it's gotta go!
        //    if (Contains(lineA) || Contains(lineB)) return true;
        //    Vector2 col;
        //    if (Line.LineCollision(lineA, lineB, Min, TopRight, out col)) return true;
        //    if (Line.LineCollision(lineA, lineB, Min, BottomLeft, out col)) return true;
        //    if (Line.LineCollision(lineA, lineB, BottomLeft, Max, out col)) return true;
        //    if (Line.LineCollision(lineA, lineB, TopRight, Max, out col)) return true;
        //    return false;
        //}
        public bool Intersects(AABB rectangle)
        {
            if (rectangle.Right < Left) return false;
            if (rectangle.Left > Right) return false;
            if (rectangle.Bottom < Top) return false;
            if (rectangle.Top > Bottom) return false;
            return true;
        }
        public static bool Intersects(ref AABB one, ref AABB two)
        {
            if (two.Right < one.Left) return false;
            if (two.Left > one.Right) return false;
            if (two.Bottom < one.Top) return false;
            if (two.Top > one.Bottom) return false;
            return true;
        }
        public bool Intersects(Rectangle rectangle)
        {
            if (rectangle.Right < Left) return false;
            if (rectangle.Left > Right) return false;
            if (rectangle.Bottom < Top) return false;
            if (rectangle.Top > Bottom) return false;
            return true;
        }
        //public bool Intersects(Line line)
        //{
        //    var b = new AABB() { Min = line.Start, Max = line.End };
        //    b.Validate();
        //    return Intersects(b);
        //}

        public AABB Clamp(AABB bounds) => new AABB() { Min = Vector2.Max(Min, bounds.Min), Max = Vector2.Min(Max, bounds.Max) };
        public AABB Combine(AABB target) => new AABB() { Min = Vector2.Min(Min, target.Min), Max = Vector2.Max(Max, target.Max) };
        public AABB Combine(Vector2 point) => new AABB() { Min = Vector2.Min(Min, point), Max = Vector2.Max(Max, point) };
        public AABB Expand(float amount) => new AABB(Min.X - amount, Min.Y - amount, Width + amount * 2, Height + amount * 2);

        public static explicit operator Rectangle(AABB value)
        {
            return new Rectangle((int)value.Min.X, (int)value.Min.Y, (int)(value.Max.X - value.Min.X), (int)(value.Max.Y - value.Min.Y));
        }

        public static implicit operator AABB(Rectangle rect)
        {
            return new AABB(rect.X, rect.Y, rect.Width, rect.Height);
        }

        // position
        public static AABB operator +(AABB value, Vector2 moveOffset)
        {
            AABB output = new AABB();
            output.Min = value.Min + moveOffset;
            output.Max = value.Max + moveOffset;
            return output;
        }

        // scale
        public static AABB operator *(AABB value, float amount)
        {
            return new AABB(value.Min.X * amount, value.Min.Y * amount, value.Max.X * amount, value.Max.Y * amount);
        }

        public AABB Scale(Vector2 scale)
        {
            return new AABB(Min.X * scale.X, Min.Y * scale.Y, Width * scale.X, Height * scale.Y);
        }
        public AABB ScaleDivide(Vector2 scale)
        {
            return new AABB(Min.X / scale.X, Min.Y / scale.Y, Width / scale.X, Height / scale.Y);
        }

        /// <summary> Which NSEW direction this <see cref="AABB"/> is jumping at another <see cref="AABB"/>. Will be north/south if aboev or below target. If jumping on top, will return South. If they are intersecting, will return None.</summary>
        //public Direction PlatformerJumpDirection(AABB target)
        //{
        //    if (Bottom <= target.Top)
        //        return Direction.South;
        //    if (Top >= target.Bottom)
        //        return Direction.North;
        //    if (Right <= target.Left)
        //        return Direction.East;
        //    if (Left >= target.Right)
        //        return Direction.West;
        //    return Direction.None;
        //}

        public static AABB CreateBounds(List<Vector2> points)
        {
            Vector2 min = points[0], max = min;
            for (int i = 1; i < points.Count; i++)
            {
                var item = points[i];
                if (item.X < min.X) min.X = item.X;
                if (item.X > max.X) max.X = item.X;
                if (item.Y < min.Y) min.Y = item.Y;
                if (item.Y > max.Y) max.Y = item.Y;
            }
            return new AABB() { Min = min, Max = max };
        }
        public static AABB CreateBounds(Vector2[] points)
        {
            Vector2 min = points[0], max = min;
            for (int i = 1; i < points.Length; i++)
            {
                var item = points[i];
                if (item.X < min.X) min.X = item.X;
                if (item.X > max.X) max.X = item.X;
                if (item.Y < min.Y) min.Y = item.Y;
                if (item.Y > max.Y) max.Y = item.Y;
            }
            return new AABB() { Min = min, Max = max };
        }

        public static implicit operator Vector4(AABB b) => new Vector4(b.X, b.Y, b.Width, b.Height);
    }

    #endregion
}
