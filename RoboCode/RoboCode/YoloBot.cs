using Robocode;
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
        private const double OFFSET = 20;

        enum Direction { NORTH, EAST, SOUTH, WEST, UNKOWN };
        private Direction currentDirection = Direction.UNKOWN;

        private RoboPhase CurrentPhase { get; set; } = RoboPhase.ArenaObservation;
        private Dictionary<ScannedRobotEvent, double> robotDanger =
            new Dictionary<ScannedRobotEvent, double>();

        private Dictionary<string, ScannedRobotEvent> robots =
            new Dictionary<string, ScannedRobotEvent>();

        private HitByBulletEvent lastBulletHit;

        private void ArenaObservation()
        {
            CurrentPhase = RoboPhase.MeetAndGreet;
        }

        private void FollowWall()
        {
            // Check wall

            SetAhead(10);
        }

        private double CalculateDanger(ScannedRobotEvent enemy)
            => enemy.Distance;

        public override void OnScannedRobot(ScannedRobotEvent evnt)
        {
            base.OnScannedRobot(evnt);

            robotDanger[evnt] = CalculateDanger(evnt);
            robots[evnt.Name] = evnt;
        }

        private void MeetAndGreet()
        {
            FollowWall();
            // TurnRadarLeft(10);

            Execute();
        }

        public override void OnHitByBullet(HitByBulletEvent evnt)
        {
            if (lastBulletHit == null && CurrentPhase == RoboPhase.MeetAndGreet)
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
                CurrentPhase = RoboPhase.MeetAndGreet;
            }

            base.OnRobotDeath(evnt);
        }
        
        private void SetGunHeadingTo(double targetHeading)
        {
            double rel = GunHeading - targetHeading;

            if (rel > 0)
                SetTurnGunRight(rel);
            else
                SetTurnGunLeft(-rel);

        }
        private void SetTankHeadingTo(double targetHeading)
        {

        }
        private void SetRadarHeadingTo(double targetHeading)
        {

        }

        private void KillItWithFire()
        {
            FollowWall();

            if (lastBulletHit != null)
            {
                ScannedRobotEvent lastScanStatus = null;
                double bearing = lastBulletHit.Bearing;

                if (robots.ContainsKey(lastBulletHit.Name))
                {
                    lastScanStatus = robots[lastBulletHit.Name];

                    if (lastScanStatus.Time > lastBulletHit.Time)
                        bearing = lastScanStatus.Bearing;
                }


                double degrees = (Heading + bearing + 360) % 360;

                if (RadarHeading - degrees < 180)
                    SetTurnRadarLeft(RadarHeading - degrees);
                else
                    SetTurnRadarRight(RadarHeading - degrees);

                SetGunHeadingTo(degrees);

                SetFire(1);
            }

            Execute();
        }


        private Direction DetermineDirection()
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

        private void DriveToWall()
        {
            currentDirection = DetermineDirection();
            double distance = DetermineDistance(currentDirection);
            Ahead(distance - OFFSET);
            TurnLeft(1);
            TurnLeft(Heading % 90);
            TurnGunLeft(1);
            TurnGunLeft(GunHeading % 90);
        }

        public override void Run()
        {
            DriveToWall();
            while (true)
            {
                switch (CurrentPhase)
                {
                    case RoboPhase.ArenaObservation:
                        ArenaObservation();
                        break;
                    case RoboPhase.MeetAndGreet:
                        MeetAndGreet();
                        break;
                    case RoboPhase.KillItWithFire:
                        KillItWithFire();
                        break;
                }
            }
        }
    }
}
