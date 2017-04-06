using Robocode;
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
        public double Velocity { get; set; }

        public double Heading { get; set; }
        public double Bearing { get; set; }
        public double Distance { get; set; }

        public bool IsTarget { get; set; }

        public long Time { get; set; }

        public int Hits { get; set; }

        public EnemyBot PreviousEntry { get; set; }

        public EnemyBot() { }

        public EnemyBot(ScannedRobotEvent ev, EnemyBot previous, double robotX, double robotY, double robotHeading, int hits)
        {
            Name = ev.Name;
            Energy = ev.Energy;
            Heading = ev.Heading;
            Time = ev.Time;
            Bearing = ev.Bearing;
            Distance = ev.Distance;
            X = robotX;
            Y = robotY;
            Hits = hits;
            PreviousEntry = previous;
            Velocity = ev.Velocity;
        }
    }
}
