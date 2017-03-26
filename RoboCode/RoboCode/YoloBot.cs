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

        private RoboPhase currentPhase = RoboPhase.ArenaObservation;
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

        private Dictionary<string, ScannedRobotEvent> robots =
            new Dictionary<string, ScannedRobotEvent>();

        private HitByBulletEvent lastBulletHit;

        private void FollowWall()
        {
            if (DetermineDistance(CurrentDirection) < OFFSET)
            {
                TurnLeft(1);
                TurnLeft(Heading % 90);
            }

            SetAhead(20);
        }

        private double CalculateDanger(ScannedRobotEvent enemy)
            => enemy.Distance;

        public override void OnScannedRobot(ScannedRobotEvent evnt)
        {
            base.OnScannedRobot(evnt);

            switch (CurrentPhase)
            {
                case RoboPhase.ArenaObservation:
                    changeDirection();
                    break;
            }

            robotDanger[evnt] = CalculateDanger(evnt);
            robots[evnt.Name] = evnt;
        }

        private void changeDirection()
        {
            Console.WriteLine("Run away!");
            TurnLeft(1);
            ArenaObservation();
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

            if (GunHeading < targetHeading)
                SetTurnGunRight(Math.Abs(rel));
            else
                SetTurnGunLeft(Math.Abs(rel));

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

                //if (GunHeading - degrees < 180)
                //    SetTurnGunLeft(GunHeading - degrees);
                //else
                //    SetTurnGunRight(GunHeading - degrees);

                SetGunHeadingTo(degrees);

                SetFire(1);
            }

            Execute();
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

        private void ArenaObservation()
        {
            double distance = DetermineDistance(CurrentDirection);
            Ahead(distance - OFFSET);
            CurrentPhase = RoboPhase.MeetAndGreet;
        }

        public override void Run()
        {
            Console.WriteLine("START");
            ArenaObservation();
            while (true)
            {
                switch (CurrentPhase)
                {
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
