using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoloSpace
{
    class EnemyRobot
    {
        //
        // Zusammenfassung:
        //     Returns the bearing to the robot you scanned, relative to your robot's heading,
        //     in degrees (-180 <= getBearing() < 180)
        public double Bearing { get; }
        //
        // Zusammenfassung:
        //     Returns the bearing to the robot you scanned, relative to your robot's heading,
        //     in radians (-PI <= getBearingRadians() < PI)
        public double BearingRadians { get; }
        //
        // Zusammenfassung:
        //     Returns the distance to the robot (your center to his center).
        public double Distance { get; }
        //
        // Zusammenfassung:
        //     Returns the energy of the robot.
        public double Energy { get; }
        //
        // Zusammenfassung:
        //     Returns the heading of the robot, in degrees (0 <= getHeading() < 360)
        public double Heading { get; }
        //
        // Zusammenfassung:
        //     Returns the heading of the robot, in radians (0 <= getHeading() < 2 * PI)
        public double HeadingRadians { get; }
        //
        // Zusammenfassung:
        //     Returns the name of the robot.
        public string Name { get; }
        //
        // Zusammenfassung:
        //     Returns the velocity of the robot.
        public double Velocity { get; }
        //
        // Zusammenfassung:
        //     true if the scanned robot is a sentry robot; false otherwise.
        public bool IsSentryRobot { get; }
    }
}
