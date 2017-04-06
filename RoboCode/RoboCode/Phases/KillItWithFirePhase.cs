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

                var targetPos = robot.GetLastTargetCoordinates();
                
                double degrees = CoordinateHelper.GetAngle(robot.X, robot.Y, targetPos.X, targetPos.Y);
                Console.WriteLine($"First angle (radar): {degrees}");
                robot.SetRadarHeadingTo(degrees - 45);
                robot.SetRadarHeadingTo(degrees + 45);

                //RobotStatus oldStatus = null;
                //var newStatus = robot.StatusHistory[robot.StatusHistory.Count - 1];
                //for (int i = robot.StatusHistory.Count - 2; i >= 0; i--)
                //{
                //    if(newStatus.X != robot.StatusHistory[i].X && newStatus.Y != robot.StatusHistory[i].Y)
                //    {
                //        oldStatus = robot.StatusHistory[i];
                //        break;
                //    }

                //}
                //var invertedMovementPos = new Point() { X = oldStatus.X - newStatus.X, Y = oldStatus.Y - newStatus.Y };
                targetPos = robot.GetLastTargetCoordinates();
                //var targetWithMovementPos = new Point() { X = targetPos.X + invertedMovementPos.X, Y = targetPos.Y + invertedMovementPos.Y };
                degrees = CoordinateHelper.GetAngle(robot.X, robot.Y, targetPos.X, targetPos.Y);
                Console.WriteLine($"Second angle (gun + radar): {degrees}, X/Y {robot.X}/{robot.Y}, target X/Y {targetPos.X}/{targetPos.Y}");

                robot.SetGunHeadingTo(degrees);

                robot.SetRadarHeadingTo(degrees - 10);
                robot.SetRadarHeadingTo(degrees + 10);
                targetPos = robot.GetLastTargetCoordinates();
                degrees = CoordinateHelper.GetAngle(robot.X, robot.Y, targetPos.X, targetPos.Y);
                Console.WriteLine($"Third angle (gun): {degrees}");

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
