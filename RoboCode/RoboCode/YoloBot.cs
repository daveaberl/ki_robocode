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
        private Dictionary<Robot, double> robotDanger = new Dictionary<Robot, double>();

        private void ArenaObservation()
        {

        }

        private void MeetAndGreet()
        {

        }

        private void KillItWithFire()
        {
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
