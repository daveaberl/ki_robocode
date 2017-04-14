using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoloSpace.Phases
{
    class WallRushPhase : AdvancedPhase
    {
        private const double OFFSET = 60;

        public WallRushPhase(YoloBot Robot) : base(Robot)
        {
        }

        public override void ActivatePhase(RoboPhase previousPhase)
        {
            Robot.BodyColor = System.Drawing.Color.Black;
            double distance = Robot.DetermineDistance(Robot.CurrentDirection);
            Robot.SetAhead(distance - OFFSET);
            Robot.Execute();
        }

        public override void Run()
        {
            if (Robot.RadarTurnRemaining == 0)
                Robot.SetTurnRadarLeft(360);

            if (Robot.DistanceRemaining <= 0 || Robot.Others == 1)
                Robot.CurrentPhase = RoboPhase.MeetAndGreet;
        }
    }
}
