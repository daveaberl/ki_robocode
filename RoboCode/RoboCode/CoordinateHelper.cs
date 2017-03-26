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

        public static double GetAngle (double x1, double y1, double x2, double y2)
        {
            double upperVal = x1 * x2 + y1 * y2;
            double lowerVal = Math.Sqrt(x1 * x1 + y1 * y1) * Math.Sqrt(x2 * x2 + y2 * y2);
            return Math.Acos(upperVal / lowerVal) * 180.0 / Math.PI;
        }
    }

    public struct AngleDistanceTuple
    {
        public double Distance { get; set; }
        public double Angle { get; set; }
    }

    public struct Point
    {
        public double X { get; set; }
        public double Y { get; set; }
    }
}
