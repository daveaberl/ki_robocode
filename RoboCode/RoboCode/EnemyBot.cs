﻿using Robocode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoloSpace
{
    class EnemyBot
    {
        public string Name { get; set; }

        public double X { get; set; }
        public double Y { get; set; }

        public double Energy { get; set; }

        public double Heading { get; set; }
        public double Bearing { get; set; }
        public double Distance { get; set; }

        public long Time { get; set; }

        public EnemyBot() { }

        public EnemyBot(ScannedRobotEvent ev, double robotX, double robotY, double robotHeading)
        {
            Name = ev.Name;
            Energy = ev.Energy;
            Heading = ev.Heading;
            Time = ev.Time;
            Bearing = ev.Bearing;
            Distance = ev.Distance;

            var relPos = CoordinateHelper.GetRelativePosition(Distance, (360 + robotHeading + Bearing - 90) % 360);

            X = relPos.X + robotX;
            Y = -relPos.Y + robotY;
        }
    }
}
