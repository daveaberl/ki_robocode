using Robocode;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoloSpace.Phases;

namespace YoloSpace
{
    class YoloBot : AdvancedRobot
    {
        public const double DISTANCE_THRESHOLD = 500;
        public const double OFFSET = 40;

        private Dictionary<RoboPhase, IPhase> phases = new Dictionary<RoboPhase, IPhase>();

        public Direction CurrentDirection
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

        private KillItWithFireStep currentKillItWithFirePhase;
        private RoboPhase currentPhase = RoboPhase.WallRush;
        public RoboPhase CurrentPhase
        {
            get
            {
                return currentPhase;
            }
            set
            {
                Console.WriteLine("* changed Phase to: " + value + " *");
                if (value == RoboPhase.KillItWithFire)
                    currentKillItWithFirePhase = KillItWithFireStep.MoveFromWall;
                foreach (var item in robots)
                {
                    item.Value.Hits = 0;
                }
                currentPhase = value;
            }
        }

        private Dictionary<string, EnemyBot> robots =
            new Dictionary<string, EnemyBot>();

        public IDictionary<string, EnemyBot> KnownEnemies
            => robots;
        public string TargetEnemyName
        {
            get => targetName;
            set => targetName = value;
        }
        public string AttackerEnemyName
        {
            get => targetOfName;
            set => targetOfName = value;
        }

        public HitByBulletEvent LastBulletHit
        {
            get => lastBulletHit;
            set => lastBulletHit = value;
        }

        private HitByBulletEvent lastBulletHit;
        private string targetName;
        private string targetOfName;
        private Random random = new Random();

        private bool isAway = false;
        private long previousTime;

        private void KillItWithFireNavigate()
        {
            switch (currentKillItWithFirePhase)
            {
                case KillItWithFireStep.MoveFromWall:
                    TurnLeft(1);
                    TurnLeft(Heading % 90);
                    previousTime = Time;
                    Ahead(100);
                    currentKillItWithFirePhase = KillItWithFireStep.Positioning;
                    break;

                case KillItWithFireStep.Positioning:
                    TurnRight(robots[targetName].Bearing + 90);
                    currentKillItWithFirePhase = KillItWithFireStep.Dodge;
                    break;
                case KillItWithFireStep.Dodge:
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

        private void ChangeDirection()
        {
            Console.WriteLine("Run away!");
            TurnLeft(1);
            CurrentPhase = RoboPhase.WallRush;
        }

        public void SetGunHeadingTo(double targetDir)
        {
            double absDegrees = Math.Abs(targetDir - GunHeading);
            if (targetDir > GunHeading)
            {
                if (absDegrees > 180)
                    SetTurnGunLeft(absDegrees - 180);
                else
                    SetTurnGunRight(absDegrees);
            }
            else if (targetDir < GunHeading)
            {
                if (absDegrees > 180)
                    SetTurnGunRight(absDegrees - 180);
                else
                    SetTurnGunLeft(absDegrees);
            }
        }

        public void SetTankHeadingTo(double targetDir)
        {
            double absDegrees = Math.Abs(targetDir - Heading);
            if (targetDir > Heading)
            {
                if (absDegrees > 180)
                    TurnLeft(absDegrees - 180);
                else
                    TurnRight(absDegrees);
            }
            else if (targetDir < Heading)
            {
                if (absDegrees > 180)
                    TurnRight(absDegrees - 180);
                else
                    TurnLeft(absDegrees);
            }
        }

        public void SetRadarHeadingTo(double targetDir)
        {
            double absDegrees = Math.Abs(targetDir - RadarHeading);
            if (targetDir > RadarHeading)
            {
                if (absDegrees > 180)
                    TurnRadarLeft(absDegrees - 180);
                else
                    TurnRadarRight(absDegrees);
            }
            else if (targetDir < RadarHeading)
            {
                if (absDegrees > 180)
                    TurnRadarRight(absDegrees - 180);
                else
                    TurnRadarLeft(absDegrees);
            }
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
                robots[evnt.Name] = new EnemyBot(evnt, robots[evnt.Name], enemyX, enemyY, Heading, robots[evnt.Name]?.Hits ?? 0);
            }
            else
            {
                robots[evnt.Name] = new EnemyBot(evnt, null, enemyX, enemyY, Heading, 0);
            }
        }

        public List<RobotStatus> StatusHistory { get; set; } = new List<RobotStatus>();
        public override void OnStatus(StatusEvent e)
        {
            StatusHistory.Add(e.Status);
            base.OnStatus(e);
        }

        public double DetermineDistance(Direction direction)
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

        public YoloBot()
        {
            phases.Add(RoboPhase.KillItWithFire, new KillItWithFirePhase(this));
            phases.Add(RoboPhase.MeetAndGreet, new MeetAndGreetPhase(this));
            phases.Add(RoboPhase.WallRush, new WallRushPhase(this));
            phases.Add(RoboPhase.RunForestRun, new RunForestRunPhase(this));
        }

        public Point GetLastTargetCoordinates()
        {
            if (TargetEnemyName == null || !KnownEnemies.ContainsKey(TargetEnemyName)) return new Point();

            var lastScanStatus = KnownEnemies[TargetEnemyName];

            double? x = LastBulletHit?.Bullet?.X;
            double? y = LastBulletHit?.Bullet?.Y;

            if (KnownEnemies.ContainsKey(TargetEnemyName))
            {
                lastScanStatus = KnownEnemies[TargetEnemyName];

                if (LastBulletHit == null || lastScanStatus.Time > LastBulletHit.Time)
                {
                    x = lastScanStatus.X;
                    y = lastScanStatus.Y;
                }
            }

            return new Point { X = x ?? 0, Y = y ?? 0 };
        }

        public override void Run()
        {
            Console.WriteLine("START");
            BodyColor = System.Drawing.Color.Black;
            GunColor = System.Drawing.Color.White;
            RadarColor = System.Drawing.Color.White;
            CurrentPhase = RoboPhase.WallRush;

            while (true)
            {
                if (phases.ContainsKey(CurrentPhase))
                    phases[CurrentPhase].Run();
            }
        }

        public override void OnPaint(IGraphics graphics)
        {
            base.OnPaint(graphics);

            foreach (var enemy in robots.Values)
            {
                graphics.DrawEllipse(Pens.Green, new RectangleF
                {
                    Height = 50,
                    Width = 50,
                    X = Convert.ToSingle(enemy.X - 25),
                    Y = Convert.ToSingle(enemy.Y - 25)
                });
            }

            if (TargetEnemyName != null)
            {
                var target = GetLastTargetCoordinates();
                graphics.DrawEllipse(Pens.Red, new RectangleF
                {
                    Height = 50,
                    Width = 50,
                    X = Convert.ToSingle(target.X - 25),
                    Y = Convert.ToSingle(target.Y - 25)
                });

                graphics.DrawLine(Pens.Red,
                    Convert.ToSingle(X),
                    Convert.ToSingle(Y),
                    Convert.ToSingle(target.X),
                    Convert.ToSingle(target.Y));
            }
        }
    }
}
