using System;

namespace TShirtCannon
{
    class Vector2
    {
        public double X;
        public double Y;

        public Vector2(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double Length
        {
            get { return Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2)); }
        }

        public Vector2 Normalize()
        {
            return this / Length;
        }

        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X + b.X, a.Y + b.Y);
        }

        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X - b.X, a.Y - b.Y);
        }

        public static Vector2 operator *(Vector2 vector, double scalar)
        {
            return new Vector2(vector.X * scalar, vector.Y * scalar);
        }

        public static Vector2 operator /(Vector2 vector, double scalar)
        {
            return new Vector2(vector.X / scalar, vector.Y / scalar);
        }
    }
}
