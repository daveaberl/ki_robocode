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

        private KillItWithFirePhase currentKillItWithFirePhase;
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
                if (value == RoboPhase.KillItWithFire)
                    currentKillItWithFirePhase = KillItWithFirePhase.MoveFromWall;
                currentPhase = value;
            }
        }

        private Dictionary<string, EnemyBot> robots =
            new Dictionary<string, EnemyBot>();

        private HitByBulletEvent lastBulletHit;
        private string targetName;
        private Random random = new Random();

        private bool isAway = false;
        private long previousTime;

        private void Navigate()
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
                    KillItWithFireNavigate();
                    break;

            }
        }

        private void KillItWithFireNavigate()
        {
            switch (currentKillItWithFirePhase)
            {
                case KillItWithFirePhase.MoveFromWall:
                    TurnLeft(1);
                    TurnLeft(Heading % 90);
                    previousTime = Time;
                    Ahead(100);
                    currentKillItWithFirePhase = KillItWithFirePhase.Positioning;
                    break;

                case KillItWithFirePhase.Positioning:
                    TurnRight(robots[targetName].Bearing + 90);
                    currentKillItWithFirePhase = KillItWithFirePhase.Dodge;
                    break;
                case KillItWithFirePhase.Dodge:
                    if (!isAway && (Time - previousTime) > 20)
                    {
                        SetAhead(100);
                        previousTime = Time;
                        isAway = !isAway;
                    }
                    else if ((Time - previousTime) > 20)
                    {
                        SetBack(100);
                        isAway = !isAway;
                        previousTime = Time;
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
                    ChangeDirection();
                    break;
            }

            if (!robots.ContainsKey(evnt.Name))
            {
                Console.WriteLine("* found new enemy: " + evnt.Name);
            }
            else
            {
                Console.WriteLine("* found enemy again: " + evnt.Name);
            }
            robots[evnt.Name] = new EnemyBot(evnt, X, Y, Heading);

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
            CheckEnemies();
            Navigate();
            TurnRadarLeft(45);
            Execute();
        }

        private void CheckEnemies()
        {
            foreach (var robot in robots)
            {
                if (robot.Value.Distance < 200)
                {
                    targetName = robot.Value.Name;
                    CurrentPhase = RoboPhase.KillItWithFire;
                }
            }
        }

        public override void OnHitByBullet(HitByBulletEvent evnt)
        {
            if (lastBulletHit == null &&
                CurrentPhase == RoboPhase.MeetAndGreet)
            {
                lastBulletHit = evnt;
                CurrentPhase = RoboPhase.KillItWithFire;
                targetName = evnt.Name;
            }
            else if (lastBulletHit?.Name == evnt.Name)
                lastBulletHit = evnt;

            base.OnHitByBullet(evnt);
        }

        public override void OnRobotDeath(RobotDeathEvent evnt)
        {
            Console.WriteLine("Died: " + evnt.Name);
            if (targetName == evnt.Name || lastBulletHit?.Name == evnt.Name)
            {
                lastBulletHit = null;
                targetName = null;

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

        private void KillItWithFire()
        {
            Navigate();

            if (targetName != null)
            {
                EnemyBot lastScanStatus = null;
                double? bearing = lastBulletHit?.Bearing;
                long? time = lastBulletHit?.Time;

                if (robots.ContainsKey(targetName))
                {
                    lastScanStatus = robots[targetName];

                    if (lastBulletHit == null || lastScanStatus.Time > lastBulletHit.Time)
                    {
                        bearing = lastScanStatus.Bearing;
                        time = lastScanStatus.Time;
                    }
                }

                if (!bearing.HasValue)
                {
                    CurrentPhase = RoboPhase.WallRush;
                    targetName = null;
                    return;
                }

                if (Time - time >= 500)
                {
                    lastBulletHit = null;
                    targetName = null;
                    CurrentPhase = RoboPhase.WallRush;
                    return;
                }

                double degrees = (Heading + bearing.Value + 360) % 360;

                //degrees = CoordinateHelper.GetAngle(X, Y,
                //    lastScanStatus?.X ?? lastBulletHit.Bullet.X,
                //    lastScanStatus?.Y ?? lastBulletHit.Bullet.Y);


                SetRadarHeadingTo(degrees);

                TurnRadarLeft(45);
                TurnRadarRight(90);


                SetGunHeadingTo(degrees);

                SetFire(1);
            }
            else
            {
                CurrentPhase = RoboPhase.WallRush;
                targetName = null;
                lastBulletHit = null;
            }
        }

        private void KillingItSoftly()
        {
            if (string.IsNullOrEmpty(targetName)) //Find last robot
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
