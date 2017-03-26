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
        private const double DISTANCE_THRESHOLD = 500;
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
                foreach (var item in robots)
                {
                    item.Value.Hits = 0;
                }
                currentPhase = value;
            }
        }

        private Dictionary<string, EnemyBot> robots =
            new Dictionary<string, EnemyBot>();

        private HitByBulletEvent lastBulletHit;
        private string targetName;
        private string targetOfName;
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

        public override void OnScannedRobot(ScannedRobotEvent evnt)
        {
            base.OnScannedRobot(evnt);

            if (Others == 1) targetName = evnt.Name;

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

            double angleToEnemy = evnt.Bearing;

            double angle = Math.PI * (Heading + angleToEnemy % 360) / 180;

            // Calculate the coordinates of the robot
            double enemyX = (X + Math.Sin(angle) * evnt.Distance);
            double enemyY = (Y + Math.Cos(angle) * evnt.Distance);

            if (robots.ContainsKey(evnt.Name))
            {
                robots[evnt.Name] = new EnemyBot(evnt, enemyX, enemyY, Heading, robots[evnt.Name]?.Hits ?? 0);
            }
            else
            {
                robots[evnt.Name] = new EnemyBot(evnt, enemyX, enemyY, Heading, 0);
            }
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
            if (!robots.ContainsKey(evnt.Name)) return;

            var enemy = robots[evnt.Name];
            enemy.Hits++;

            if (enemy.Distance > DISTANCE_THRESHOLD && enemy.Time < 500) return;

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

                CurrentPhase = RoboPhase.WallRush;
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

            var targetOfRobot = robots.Values.FirstOrDefault(kvp => kvp.Hits > 3);
            if (targetOfRobot != null)
            {
                targetOfName = targetOfRobot.Name;
                CurrentPhase = RoboPhase.RunForestRun;
                return;
            }

            if (targetName != null)
            {
                EnemyBot lastScanStatus = null;
                double? bearing = lastBulletHit?.Bearing;
                long? time = lastBulletHit?.Time;

                double? x = lastBulletHit?.Bullet?.X;
                double? y = lastBulletHit?.Bullet?.Y;

                if (robots.ContainsKey(targetName))
                {
                    lastScanStatus = robots[targetName];

                    if (lastBulletHit == null || lastScanStatus.Time > lastBulletHit.Time)
                    {
                        bearing = lastScanStatus.Bearing;
                        time = lastScanStatus.Time;

                        x = lastScanStatus.X;
                        y = lastScanStatus.Y;
                    }
                }

                if (!bearing.HasValue)
                {
                    CurrentPhase = RoboPhase.WallRush;
                    targetName = null;
                    return;
                }

                if (Time - time >= 300 && Others > 1)
                {
                    lastBulletHit = null;
                    targetName = null;
                    CurrentPhase = RoboPhase.WallRush;
                    return;
                }

                double enemyX = x.Value;
                double enemyY = y.Value;

                double p1X = X - X;
                double p1Y = enemyY - Y;
                double p2X = enemyX - X;
                double p2Y = enemyY - Y;

                double degrees = CoordinateHelper.GetAngle(p1X, p1Y, p2X, p2Y);
                Console.WriteLine(" degree: " + degrees);

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

            Navigate();
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

        private void RunForestRun()
        {
            double xMiddle = BattleFieldWidth / 2;
            double yMiddle = BattleFieldHeight / 2;

            SetTankHeadingTo(Heading + CoordinateHelper.GetAngle(X, Y, xMiddle, yMiddle));
            Ahead(200);

            CurrentPhase = RoboPhase.WallRush;
            targetName = null;
            targetOfName = null;
        }

        public override void Run()
        {
            Console.WriteLine("START");
            BodyColor = System.Drawing.Color.Black;
            GunColor = System.Drawing.Color.White;
            RadarColor = System.Drawing.Color.White;
            WallRush();
            while (true)
            {
                switch (CurrentPhase)
                {
                    case RoboPhase.WallRush:
                        BodyColor = System.Drawing.Color.Black;
                        WallRush();
                        break;
                    case RoboPhase.MeetAndGreet:
                        BodyColor = System.Drawing.Color.Orange;
                        MeetAndGreet();
                        break;
                    case RoboPhase.KillItWithFire:
                        BodyColor = System.Drawing.Color.Red;
                        if(Others == 1)
                        {
                            BodyColor = System.Drawing.Color.Transparent;
                        }

                        KillItWithFire();
                        break;
                    case RoboPhase.RunForestRun:
                        BodyColor = System.Drawing.Color.Pink;
                        RunForestRun();
                        break;
                }
            }
        }
    }
}
