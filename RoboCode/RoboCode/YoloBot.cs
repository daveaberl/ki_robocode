﻿using Robocode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoloSpace
{
    class YoloBot : AdvancedRobot
    {
        private const double DANGER_THRESHOLD = 0x0;
        private const double OFFSET = 40;

        enum Direction { NORTH, EAST, SOUTH, WEST, UNKOWN };
        private Direction CurrentDirection
        {
            get
            {
                TurnLeft(Heading % 90);
                if (Heading == 270)
                    return Direction.WEST;
                else if (Heading == 90)
                    return Direction.EAST;
                else if (Heading == 180)
                    return Direction.SOUTH;
                else if (Heading == 0)
                    return Direction.NORTH;
                return Direction.UNKOWN;
            }
        }

        private RoboPhase currentPhase = RoboPhase.WallRush;
        private RoboPhase CurrentPhase
        {
            get
            {
                return currentPhase;
            }
            set
            {
                Console.WriteLine("* changed Phase to: " + value + " *");
                currentPhase = value;
            }
        }

        private Dictionary<ScannedRobotEvent, double> robotDanger =
            new Dictionary<ScannedRobotEvent, double>();

        private Dictionary<string, EnemyBot> robots =
            new Dictionary<string, EnemyBot>();

        private HitByBulletEvent lastBulletHit;
        private string targetName;

        private bool directionIndicator = true;

        private void FollowWall()
        {
            switch (currentPhase)
            {
                case RoboPhase.MeetAndGreet:
                    if (DetermineDistance(CurrentDirection) < OFFSET)
                    {
                        TurnLeft(1);
                        TurnLeft(Heading % 90);
                    }
                    SetAhead(20);
                    break;
                case RoboPhase.KillItWithFire:
                    if (DetermineDistance(CurrentDirection) < OFFSET)
                    {
                        if (directionIndicator)
                        {
                            Ahead(DetermineDistance(CurrentDirection));
                        } else
                        {
                            Back(DetermineDistance(DetermineOppositeDirection(CurrentDirection)));
                        }
                    }

                    break;

            }
        }

        private Direction DetermineOppositeDirection(Direction direction)
        {
            switch (direction)
            {
                case Direction.NORTH:
                    return Direction.SOUTH;
                case Direction.EAST:
                    return Direction.WEST;
                case Direction.SOUTH:
                    return Direction.NORTH;
                case Direction.WEST:
                    return Direction.EAST;
                default:
                    return Direction.UNKOWN;
            }
        }

        private double CalculateDanger(ScannedRobotEvent enemy)
            => enemy.Distance;

        public override void OnScannedRobot(ScannedRobotEvent evnt)
        {
            base.OnScannedRobot(evnt);

            switch (CurrentPhase)
            {
                case RoboPhase.WallRush:
                    changeDirection();
                    break;
            }

            robotDanger[evnt] = CalculateDanger(evnt);
            robots[evnt.Name] = evnt;

            if (CurrentPhase == RoboPhase.KillingItSoftly)
                targetName = evnt.Name;
        }

        private void ChangeDirection()
        {
            Console.WriteLine("Run away!");
            TurnLeft(1);
            WallRush();
        }

        private void MeetAndGreet()
        {
            FollowWall();
            TurnRadarLeft(45);
            Execute();
        }

        public override void OnHitByBullet(HitByBulletEvent evnt)
        {
            if (lastBulletHit == null &&
                CurrentPhase == RoboPhase.MeetAndGreet)
            {
                lastBulletHit = evnt;
                CurrentPhase = RoboPhase.KillItWithFire;
            }
            else if (lastBulletHit?.Name == evnt.Name) lastBulletHit = evnt;

            base.OnHitByBullet(evnt);
        }

        public override void OnRobotDeath(RobotDeathEvent evnt)
        {
            if (lastBulletHit?.Name == evnt.Name)
            {
                lastBulletHit = null;

                if (Others > 1)
                    CurrentPhase = RoboPhase.MeetAndGreet;
                else
                    CurrentPhase = RoboPhase.KillingItSoftly;
            }

            base.OnRobotDeath(evnt);
        }

        private void SetGunHeadingTo(double targetDir)
        {
            double absDegrees = Math.Abs(targetDir - GunHeading);
            if (targetDir > GunHeading)
            {
                if (absDegrees > 180)
                    TurnGunLeft(absDegrees - 180);
                else
                    TurnGunRight(absDegrees);
            }
            else
            {
                if (absDegrees > 180)
                    TurnGunRight(absDegrees - 180);
                else
                    TurnGunLeft(absDegrees);
            }
        }
            
        private void SetTankHeadingTo(double targetDir)
        {
            double absDegrees = Math.Abs(targetDir - Heading);
            if (targetDir > Heading)
            {
                if (absDegrees > 180)
                    TurnLeft(absDegrees - 180);
                else
                    TurnRight(absDegrees);
            }
            else
            {
                if (absDegrees > 180)
                    TurnRight(absDegrees - 180);
                else
                    TurnLeft(absDegrees);
            }
        }

        private void SetRadarHeadingTo(double targetDir)
        {
            double absDegrees = Math.Abs(targetDir - RadarHeading);
            if (targetDir > RadarHeading)
            {
                if (absDegrees > 180)
                    TurnRadarLeft(absDegrees - 180);
                else
                    TurnRadarRight(absDegrees);
            }
            else
            {
                if (absDegrees > 180)
                    TurnRadarRight(absDegrees - 180);
                else
                    TurnRadarLeft(absDegrees);
            }
        }

        private void Test(double targetDir)
        {
            double absDegrees = Math.Abs(targetDir - GunHeading);
            if (targetDir > GunHeading)
            {
                if (absDegrees > 180)
                    SetTurnGunLeft(absDegrees - 180);
                else
                    SetTurnGunRight(absDegrees);
            }
            else
            {
                if (absDegrees > 180)
                    SetTurnGunRight(absDegrees - 180);
                else
                    SetTurnGunLeft(absDegrees);
            }
        }

        private void KillItWithFire()
        {
            FollowWall();

            if (lastBulletHit != null)
            {
                EnemyBot lastScanStatus = null;
                double bearing = lastBulletHit.Bearing;

                if (robots.ContainsKey(lastBulletHit.Name))
                {
                    lastScanStatus = robots[lastBulletHit.Name];

                    if (lastScanStatus.Time > lastBulletHit.Time)
                        bearing = lastScanStatus.Bearing;
                }

                if (Time - lastBulletHit.Time >= 500)
                {
                    lastBulletHit = null;
                    CurrentPhase = RoboPhase.MeetAndGreet;
                }

                double degrees = (Heading + bearing + 360) % 360;



                SetRadarHeadingTo(degrees);

                TurnRadarLeft(45);
                TurnRadarRight(90);

                
                SetGunHeadingTo(degrees);

                SetFire(1);
            }
        }

        private void KillingItSoftly()
        {
            if(string.IsNullOrEmpty(targetName)) //Find last robot
            {

            }
            else //Kill last robot
            {
                
            }

            Execute();
        }

        public override void OnBulletHit(BulletHitEvent evnt)
        {
            base.OnBulletHit(evnt);

            if (robots.ContainsKey(evnt.VictimName))
            {
                robots[evnt.VictimName].X = evnt.Bullet.X;
                robots[evnt.VictimName].Y = evnt.Bullet.Y;
                robots[evnt.VictimName].Time = evnt.Time;
                robots[evnt.VictimName].Energy = evnt.VictimEnergy;
            }
        }

        private double DetermineDistance(Direction direction)
        {
            switch (direction)
            {
                case Direction.NORTH:
                    return BattleFieldHeight - Y;
                case Direction.EAST:
                    return BattleFieldWidth - X;
                case Direction.SOUTH:
                    return Y;
                case Direction.WEST:
                    return X;
                default:
                    return 0;
            }
        }

        private void WallRush()
        {
            double distance = DetermineDistance(CurrentDirection);
            Ahead(distance - OFFSET);
            CurrentPhase = RoboPhase.MeetAndGreet;
        }

        public override void Run()
        {
            Console.WriteLine("START");
            WallRush();
            while (true)
            {
                switch (CurrentPhase)
                {
                    case RoboPhase.WallRush:
                        WallRush();
                        break;
                    case RoboPhase.MeetAndGreet:
                        MeetAndGreet();
                        break;
                    case RoboPhase.KillItWithFire:
                        KillItWithFire();
                        break;
                    case RoboPhase.KillingItSoftly:
                        KillingItSoftly();
                        break;
                }
            }
        }
    }
}
