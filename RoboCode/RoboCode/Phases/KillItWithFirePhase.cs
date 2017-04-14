﻿using Robocode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoloSpace.Phases
{
    class KillItWithFirePhase : AdvancedPhase
    {
        private const int MAX_DISTANCE = 400;
        private const int WALL_OFFSET = 30;

        private KillItWithFireStep currentKillItWithFirePhase;
        private bool isAway;

        public KillItWithFirePhase(YoloBot robot) : base(robot)
        {
        }

        public override void ActivatePhase(RoboPhase previousPhase)
        {
            base.ActivatePhase(previousPhase);
            currentKillItWithFirePhase = KillItWithFireStep.MoveFromWall;
        }

        private void Navigate()
        {
            switch (currentKillItWithFirePhase)
            {
                case KillItWithFireStep.MoveFromWall:
                    Robot.SetTurnLeft(Robot.Heading % 90 == 0 ? 90 : Robot.Heading % 90);
                    Robot.SetAhead(100);
                    currentKillItWithFirePhase = KillItWithFireStep.Dodge;
                    break;
                case KillItWithFireStep.Dodge:
                    if ((!isAway && Robot.DistanceRemaining == 0))
                    {
                        Robot.SetAhead(100);
                        Robot.SetTurnLeft(100);
                        isAway = !isAway;
                    }
                    else if (Robot.DistanceRemaining == 0)
                    {
                        Robot.SetBack(100);
                        Robot.SetTurnRight(100);
                        isAway = !isAway;
                    }
                    break;
            }

            if (Robot.KnownEnemies[Robot.TargetEnemyName].Distance >= MAX_DISTANCE && Robot.Others > 1)
            {
                Robot.CurrentPhase = RoboPhase.WallRush;
            }
        }

        private bool DetermineWallDanger()
        {
            if ((Robot.Heading == 0 || Robot.Heading == 180) && (Robot.Y < WALL_OFFSET || Robot.Y > Robot.BattleFieldHeight - WALL_OFFSET))
            {
                return false;
            }
            else if ((Robot.Heading == 90 || Robot.Heading == 270) && (Robot.X < WALL_OFFSET || Robot.X > Robot.BattleFieldWidth - WALL_OFFSET))
            {
                return false;
            }
            else if (Robot.Y < WALL_OFFSET || Robot.Y > Robot.BattleFieldHeight - WALL_OFFSET || Robot.X < WALL_OFFSET || Robot.X > Robot.BattleFieldWidth - WALL_OFFSET)
            {
                return false;
            }
            return true;
        }

        bool scanLeft = false;

        private void Scan()
        {
            var pos = Robot.GetLastTargetCoordinates();

            if (pos.X == 0 && pos.Y == 0) return;

            double degrees = CoordinateHelper.GetAngle(Robot.X, Robot.Y, pos.X, pos.Y);

            double diff = (degrees - Robot.RadarHeading) + 180;

            if (Robot.RadarTurnRemaining == 0)
            {
                if (scanLeft)
                {
                    Robot.SetRadarHeadingTo(degrees + 45);
                    scanLeft = false;
                }
                else
                {
                    Robot.SetRadarHeadingTo(degrees - 45);
                    scanLeft = true;
                }
            }

            //Robot.SetRadarHeadingTo(degrees - 45);
            //Robot.SetRadarHeadingTo(degrees + 45);
        }

        private double CalculatePower(EnemyBot target)
        {
            Console.WriteLine($"target distance: {target.Distance}; gun heat: {Robot.GunHeat}; max power: {Rules.MAX_BULLET_POWER}");

            if (target.Distance < 100)
                return 3;

            if (target.Distance < 180)
                return 2.5;

            if (target.Distance < 230)
                return 2;

            if (target.Distance < 270)
                return 1.5;

            return 1;
        }

        private long GetTimeOfMaxHistory(EnemyBot target, long maxHist = 30)
        {
            return maxHist;
        }

        private void Aim(EnemyBot target, double power)
        {
            Point p = new Point { X = target.X, Y = target.Y };
            for (int i = 0; i < 10; i++)
            {
                double nextTime = CoordinateHelper.GetDistance(p.X, p.Y, Robot.X, Robot.Y) / Rules.GetBulletSpeed(power);
                double time = Robot.Time + nextTime;
                p = GuessPosition(time, target);
            }
            
            Robot.Target = p;
        }

        private Point GuessPosition(double time, EnemyBot target)
        {
            double diff = time - target.Time;
            Point? p = null;
            if (target.PreviousEntry != null)
            {
                double hCPT = (target.HeadingRad - target.PreviousEntry.HeadingRad) / (target.Time - target.PreviousEntry.Time);
                if (Math.Abs(hCPT) > 0.00001)
                {
                    double radius = target.Velocity / hCPT;
                    double toTargetHead = diff * hCPT;
                    p = new Point
                    {
                        X = target.X + (Math.Cos(target.HeadingRad) * radius) - (Math.Cos(target.HeadingRad + toTargetHead) * radius),
                        Y = target.Y + (Math.Sin(target.HeadingRad + toTargetHead) * radius) - (Math.Sin(target.HeadingRad) * radius)
                    };
                }
            }

            if(p == null)
            {
                p = new Point
                {
                    X = target.X + Math.Sin(target.HeadingRad) * target.Velocity * diff,
                    Y = target.Y + Math.Cos(target.HeadingRad) * target.Velocity * diff
                };
            }

            double x = Math.Max(Robot.Width / 2, p.Value.X);
            x = Math.Min(Robot.BattleFieldWidth - Robot.Width / 2, x);
            double y = Math.Max(Robot.Height / 2, p.Value.Y);
            y = Math.Min(Robot.BattleFieldHeight - Robot.Height / 2, y);
            p = new Point
            {
                X = x,
                Y = y
            };

            return (Point)p;
        }

        private void Shoot(EnemyBot target, double power)
        {
            double degrees = CoordinateHelper.GetAngle(Robot.X, Robot.Y, Robot.Target.X, Robot.Target.Y);
            Robot.SetGunHeadingTo(degrees);

            if (Robot.GunTurnRemaining < 0.5 && Robot.GunHeat == 0)
                Robot.SetFire(power);
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

            Scan();
            if (Robot.TargetEnemyName != null)
            {
                if (Robot.KnownEnemies.ContainsKey(Robot.TargetEnemyName))
                {
                    var target = Robot.KnownEnemies[Robot.TargetEnemyName];
                    double power = CalculatePower(target);
                    Console.WriteLine($"power: {power}");
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

        public override string ToString()
        {
            return $"{base.ToString()} - {Enum.GetName(typeof(KillItWithFireStep), currentKillItWithFirePhase)}";
        }
    }
}
