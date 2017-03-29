using Robocode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoloSpace.Phases
{
    class KillItWithFirePhase : IPhase
    {
        private KillItWithFireStep currentKillItWithFirePhase;
        private YoloBot robot;
        private bool isAway;

        public KillItWithFirePhase(YoloBot robot)
        {
            this.robot = robot;
        }

        private void Navigate()
        {
            switch (currentKillItWithFirePhase)
            {
                case KillItWithFireStep.MoveFromWall:
                    robot.TurnLeft(1);
                    robot.TurnLeft(robot.Heading % 90);
                    robot.Ahead(100);
                    currentKillItWithFirePhase = KillItWithFireStep.Positioning;
                    break;

                case KillItWithFireStep.Positioning:
                    robot.TurnRight(robot.KnownEnemies[robot.TargetEnemyName].Bearing + 90);
                    currentKillItWithFirePhase = KillItWithFireStep.Dodge;
                    break;
                case KillItWithFireStep.Dodge:
                    if (!isAway && robot.DistanceRemaining == 0)
                    {
                        robot.SetAhead(100);
                        isAway = !isAway;
                    }
                    else if (robot.DistanceRemaining == 0)
                    {
                        robot.SetBack(100);
                        isAway = !isAway;
                    }
                    break;
            }
        }

        public void Run()
        {
            robot.BodyColor = System.Drawing.Color.Red;
            if (robot.Others == 1)
            {
                robot.BodyColor = System.Drawing.Color.Transparent;
            }

            Navigate();

            var targetOfRobot = robot.KnownEnemies.Values.FirstOrDefault(kvp => kvp.Hits > 3);
            if (targetOfRobot != null)
            {
                robot.AttackerEnemyName = targetOfRobot.Name;
                robot.CurrentPhase = RoboPhase.RunForestRun;
                return;
            }

            if (robot.TargetEnemyName != null)
            {
                EnemyBot lastScanStatus = null;
                double? bearing = robot.LastBulletHit?.Bearing;
                long? time = robot.LastBulletHit?.Time;
                
                if (robot.KnownEnemies.ContainsKey(robot.TargetEnemyName))
                {
                    lastScanStatus = robot.KnownEnemies[robot.TargetEnemyName];

                    if (robot.LastBulletHit == null || lastScanStatus.Time > robot.LastBulletHit.Time)
                    {
                        bearing = lastScanStatus.Bearing;
                        time = lastScanStatus.Time;
                    }
                }

                if (!bearing.HasValue)
                {
                    robot.CurrentPhase = RoboPhase.WallRush;
                    robot.TargetEnemyName = null;
                    return;
                }

                if (robot.Time - time >= 300 && robot.Others > 1)
                {
                    robot.LastBulletHit = null;
                    robot.TargetEnemyName = null;
                    robot.CurrentPhase = RoboPhase.WallRush;
                    return;
                }

                var pos = robot.GetLastTargetCoordinates();
                
                double degrees = CoordinateHelper.GetAngle(robot.X, robot.Y, pos.X, pos.Y);

                Console.WriteLine(" degree: " + degrees);

                robot.SetRadarHeadingTo(degrees - 45);
                robot.SetRadarHeadingTo(degrees + 45);

                pos = robot.GetLastTargetCoordinates();
                degrees = CoordinateHelper.GetAngle(robot.X, robot.Y, pos.X, pos.Y);
                
                robot.SetGunHeadingTo(degrees);

                robot.SetFire(1);
            }
            else
            {
                robot.CurrentPhase = RoboPhase.WallRush;
                robot.TargetEnemyName = null;
                robot.LastBulletHit = null;
            }
        }
    }
}
