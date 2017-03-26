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
            var xDiff = x2 - x1;
            var yDiff = y2 - y1;

            return Math.Atan2(yDiff, xDiff) * 180.0 / Math.PI;
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
