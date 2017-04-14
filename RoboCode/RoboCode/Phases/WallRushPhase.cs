using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoloSpace.Phases
{
    class WallRushPhase : AdvancedPhase
    {
        private bool turnHeading = false;
        private const double OFFSET = 60;

        public WallRushPhase(YoloBot Robot) : base(Robot)
        {
        }

        public override void ActivatePhase(RoboPhase previousPhase)
        {
            Robot.BodyColor = System.Drawing.Color.Black;
            TurnHeadingToNearestWall();
        }

        private void TurnHeadingToNearestWall()
        {
            double dEastWall = Robot.DetermineDistance(Direction.EAST);
            double dWestWall = Robot.DetermineDistance(Direction.WEST);
            double dNorthWall = Robot.DetermineDistance(Direction.NORTH);
            double dSouthWall = Robot.DetermineDistance(Direction.SOUTH);

            if (dEastWall <= dWestWall && dEastWall <= dNorthWall && dEastWall <= dSouthWall)
            {
                Robot.SetTankHeadingTo(90);
            }
            else if(dWestWall <= dEastWall && dWestWall <= dNorthWall && dWestWall <= dSouthWall)
            {
                Robot.SetTankHeadingTo(270);
            }
            else if(dNorthWall <= dEastWall && dNorthWall <= dWestWall && dNorthWall <= dSouthWall)
            {
                Robot.SetTankHeadingTo(0);
            }
            else if(dSouthWall <= dEastWall && dSouthWall <= dWestWall && dSouthWall <= dNorthWall)
            {
                Robot.SetTankHeadingTo(180);
            }

            turnHeading = true;
        }

        public override void Run()
        {
            if(turnHeading && Robot.TurnRemaining == 0)
            {
                turnHeading = false;
                double distance = Robot.DetermineDistance(Robot.CurrentDirection);
                Robot.SetAhead(distance - YoloBot.OFFSET);
            }

            if (Robot.RadarTurnRemaining == 0)
                Robot.SetTurnRadarLeft(360);

            if ((!turnHeading && Robot.DistanceRemaining <= 0) || Robot.Others == 1)
                Robot.CurrentPhase = RoboPhase.MeetAndGreet;
        }
    }
}
