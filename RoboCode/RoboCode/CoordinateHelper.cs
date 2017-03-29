using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoloSpace
{
    public static class CoordinateHelper
    {
        public static Point GetRelativePosition(double distance, double angle)
            => new Point
            {
                X = Math.Cos((angle) * Math.PI / 180.0) * distance,
                Y = Math.Sin((angle) * Math.PI / 180.0) * distance
            };

        public static double GetAngle(double x1, double y1, double x2, double y2)
        {
            double p1X = 0;
            double p1Y = y2 - y1;
            double p2X = x2 - x1;
            double p2Y = y2 - y1;

            double upperVal = p1X * p2X + p1Y * p2Y;
            double lowerVal = Math.Sqrt(p1X * p1X + p1Y * p1Y) * Math.Sqrt(p2X * p2X + p2Y * p2Y);
            double degrees = Math.Acos(upperVal / lowerVal) * 180.0 / Math.PI;


            if (x2 < x1 && y2 > y1) // 4. Quadrant
            {
                degrees = 360 - degrees;
            }
            else if (x2 > x1 && y2 < y1) //2. Quadrant
            {
                degrees = 180 - degrees;
            }
            else if (x2 < x1 && y2 < y1) //3. Quadrant
            {
                degrees += 180;
            }

            return degrees;
        }
    }

    public struct Point
    {
        public double X { get; set; }
        public double Y { get; set; }
    }
}
