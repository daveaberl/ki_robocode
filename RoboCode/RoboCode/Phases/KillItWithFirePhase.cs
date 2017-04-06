using Robocode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoloSpace.Phases
{
    class KillItWithFirePhase : AdvancedPhase
    {
        private KillItWithFireStep currentKillItWithFirePhase;
        private bool isAway;

        private long lastReached = 0;

        public KillItWithFirePhase(YoloBot Robot) : base(Robot)
        {
        }

        private void Navigate()
        {
            switch (currentKillItWithFirePhase)
            {
                case KillItWithFireStep.MoveFromWall:
                    Robot.TurnLeft(1);
                    Robot.TurnLeft(Robot.Heading % 90);
                    Robot.Ahead(100);
                    currentKillItWithFirePhase = KillItWithFireStep.Positioning;
                    break;

                case KillItWithFireStep.Positioning:
                    Robot.TurnRight(Robot.KnownEnemies[Robot.TargetEnemyName].Bearing + 90);
                    currentKillItWithFirePhase = KillItWithFireStep.Dodge;
                    break;
                case KillItWithFireStep.Dodge:
                    if (!isAway && Robot.DistanceRemaining == 0)
                    {
                        Robot.SetAhead(100);
                        isAway = !isAway;
                    }
                    else if (Robot.DistanceRemaining == 0)
                    {
                        Robot.SetBack(100);
                        isAway = !isAway;
                    }
                    break;
            }
        }

        private void Scan()
        {
            var pos = Robot.GetLastTargetCoordinates();
            double degrees = CoordinateHelper.GetAngle(Robot.X, Robot.Y, pos.X, pos.Y);
            Robot.SetRadarHeadingTo(degrees - 45);
            Robot.SetRadarHeadingTo(degrees + 45);
        }


        private double CalculatePower()
        {
            return 1;
        }

        private void Aim(EnemyBot target, double power)
        {
            double distance = Math.Sqrt(Math.Pow(target.X - Robot.X, 2) + Math.Pow(target.Y - Robot.Y, 2));
            double pTime = Robot.Time + distance / (20 - 3 * power);
            double diff = pTime - target.Time;

            if (target.PreviousEntry != null)
            {
                double hCPT = (target.Heading - target.PreviousEntry.Heading) / (target.Time - target.PreviousEntry.Time);
                if (Math.Abs(hCPT) > 0.00001)
                {
                    double radius = target.Velocity / hCPT;
                    double toTargetHead = diff * hCPT;
                    Robot.Target = new Point
                    {
                        X = (Math.Cos(target.Heading) * radius) - (Math.Cos(target.Heading + toTargetHead) * radius),
                        Y = (Math.Sin(target.Heading + toTargetHead) * radius) - (Math.Sin(target.Heading) * radius),
                    };
                    return;
                }
            }
            Robot.Target = new Point
            {
                X = target.X + Math.Sin(target.Heading) * target.Velocity * diff,
                Y = target.Y + Math.Cos(target.Heading) * target.Velocity * diff
            };
        }

        private void Shoot(EnemyBot target, double power)
        {
            //Point optimizedTarget = Robot.GetLastTargetCoordinates(true);

            //// TODO: Winkelfunktionen zum vorhersagen der Position :P

            //if (Math.Abs(target.X - optimizedTarget.X) > 60 ||
            //    Math.Abs(target.Y - optimizedTarget.Y) > 60)
            //{
            //    optimizedTarget.X = target.X;
            //    optimizedTarget.Y = target.Y;
            //}

            double degrees = CoordinateHelper.GetAngle(Robot.X, Robot.Y, Robot.Target.X, Robot.Target.Y);
            Robot.SetGunHeadingTo(degrees);
            Robot.Execute();

            if (Robot.GunTurnRemaining < 3)
                Robot.Fire(power);
        }

        public override void Run()
        {
            Robot.BodyColor = System.Drawing.Color.Red;
            if (Robot.Others == 1)
            {
                Robot.BodyColor = System.Drawing.Color.Transparent;
            }

            Navigate();

            var targetOfRobot = Robot.KnownEnemies.Values.FirstOrDefault(kvp => kvp.Hits > 3);
            if (targetOfRobot != null)
            {
                Robot.AttackerEnemyName = targetOfRobot.Name;
                Robot.CurrentPhase = RoboPhase.RunForestRun;
                return;
            }

            if (Robot.TargetEnemyName != null)
            {
                //EnemyBot lastScanStatus = null;
                //double? bearing = Robot.LastBulletHit?.Bearing;
                //long? time = Robot.LastBulletHit?.Time;

                //if (Robot.KnownEnemies.ContainsKey(Robot.TargetEnemyName))
                //{
                //    lastScanStatus = Robot.KnownEnemies[Robot.TargetEnemyName];

                //    if (Robot.LastBulletHit == null || lastScanStatus.Time > Robot.LastBulletHit.Time)
                //    {
                //        bearing = lastScanStatus.Bearing;
                //        time = lastScanStatus.Time;
                //    }
                //}

                //if (!bearing.HasValue)
                //{
                //    Robot.CurrentPhase = RoboPhase.WallRush;
                //    Robot.TargetEnemyName = null;
                //    return;
                //}

                //if (Robot.Time - time >= 300 && Robot.Others > 1)
                //{
                //    Robot.LastBulletHit = null;
                //    Robot.TargetEnemyName = null;
                //    Robot.CurrentPhase = RoboPhase.WallRush;
                //    return;
                //}

                //var pos = Robot.GetLastTargetCoordinates();

                //double degrees = CoordinateHelper.GetAngle(Robot.X, Robot.Y, pos.X, pos.Y);

                //Console.WriteLine(" degree: " + degrees);

                //Robot.SetRadarHeadingTo(degrees - 45);
                //Robot.SetRadarHeadingTo(degrees + 45);

                //pos = Robot.GetLastTargetCoordinates();
                //degrees = CoordinateHelper.GetAngle(Robot.X, Robot.Y, pos.X, pos.Y);

                //if (lastScanStatus != null) Shoot(lastScanStatus);
                if (Robot.KnownEnemies.ContainsKey(Robot.TargetEnemyName))
                {
                    var target = Robot.KnownEnemies[Robot.TargetEnemyName];
                    Scan();
                    double power = CalculatePower();
                    Aim(target, power);
                    Shoot(target, power);
                }

            }
            else
            {
                Robot.CurrentPhase = RoboPhase.WallRush;
                Robot.TargetEnemyName = null;
                Robot.LastBulletHit = null;
            }
        }
    }
}
