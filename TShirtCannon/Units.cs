using System;

namespace TShirtCannon
{
    class Units
    {
        public static double ToDegrees(double radians)
        {
            return 180.0 / Math.PI * radians;
        }

        public static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }
    }
}
