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
                X = Math.Cos(angle) * distance,
                Y = Math.Sin(angle) * distance
            };
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
