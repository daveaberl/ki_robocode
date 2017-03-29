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
        private long previousTime;
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
                    previousTime = robot.Time;
                    robot.Ahead(100);
                    currentKillItWithFirePhase = KillItWithFireStep.Positioning;
                    break;

                case YoloSpace.KillItWithFireStep.Positioning:
                    robot.TurnRight(robot.KnownEnemies[robot.TargetEnemyName].Bearing + 90);
                    currentKillItWithFirePhase = KillItWithFireStep.Dodge;
                    break;
                case YoloSpace.KillItWithFireStep.Dodge:
                    if (!isAway && (robot.Time - previousTime) > 20)
                    {
                        robot.SetAhead(100);
                        previousTime = robot.Time;
                        isAway = !isAway;
                    }
                    else if ((robot.Time - previousTime) > 20)
                    {
                        robot.SetBack(100);
                        isAway = !isAway;
                        previousTime = robot.Time;
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

                double? x = robot.LastBulletHit?.Bullet?.X;
                double? y = robot.LastBulletHit?.Bullet?.Y;

                if (robot.KnownEnemies.ContainsKey(robot.TargetEnemyName))
                {
                    lastScanStatus = robot.KnownEnemies[robot.TargetEnemyName];

                    if (robot.LastBulletHit == null || lastScanStatus.Time > robot.LastBulletHit.Time)
                    {
                        bearing = lastScanStatus.Bearing;
                        time = lastScanStatus.Time;

                        x = lastScanStatus.X;
                        y = lastScanStatus.Y;
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

                double enemyX = lastScanStatus?.X ?? robot.LastBulletHit.Bullet.X;
                double enemyY = lastScanStatus?.Y ?? robot.LastBulletHit.Bullet.Y;
                Console.WriteLine($"enemy: {enemyX}/{enemyY}");

                double p1X = robot.X - robot.X; // Evtl. gleich auf 0 setzen? xD
                double p1Y = enemyY - robot.Y;
                double p2X = enemyX - robot.X;
                double p2Y = enemyY - robot.Y;

                //double degrees = CoordinateHelper.GetAngle(p1X, p1Y, p2X, p2Y);
                double degrees = CoordinateHelper.GetAngle(robot.X, robot.Y, enemyX, enemyY);

                Console.WriteLine(" degree: " + degrees);

                robot.SetRadarHeadingTo(degrees);

                robot.TurnRadarLeft(45);
                robot.TurnRadarRight(90);

                robot.SetGunHeadingTo(degrees);

                if (robot.GunTurnRemaining == 0) robot.SetFire(1);
            }
            else
            {
                robot.CurrentPhase = RoboPhase.WallRush;
                robot.TargetEnemyName = null;
                robot.LastBulletHit = null;
            }

            Navigate();
        }
    }
}
