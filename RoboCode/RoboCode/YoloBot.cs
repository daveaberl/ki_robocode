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

        private Dictionary<string, EnemyBot> robots =
            new Dictionary<string, EnemyBot>();

        private HitByBulletEvent lastBulletHit;

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
                        Back(DetermineDistance(DetermineOppositeDirection(CurrentDirection)));
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
                case RoboPhase.ArenaObservation:
                    ChangeDirection();
                    break;
            }

            robotDanger[evnt] = CalculateDanger(evnt);
            robots[evnt.Name] = new EnemyBot(evnt, X, Y);
        }

        private void ChangeDirection()
        {
            Console.WriteLine("Run away!");
            TurnLeft(1);
            ArenaObservation();
        }

        private void MeetAndGreet()
        {
            FollowWall();
            TurnRadarLeft(45);
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

                SetGunHeadingTo(degrees);
                SetTurnRadarLeft(45);

                SetFire(1);
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
