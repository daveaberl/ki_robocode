using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoloSpace.Phases
{
    class WallRushPhase : AdvancedPhase
    {
        public WallRushPhase(YoloBot Robot) : base(Robot)
        {
        }

        public override void ActivatePhase(RoboPhase previousPhase)
        {
            Robot.BodyColor = System.Drawing.Color.Black;
            double distance = Robot.DetermineDistance(Robot.CurrentDirection);
            Robot.SetAhead(distance - YoloBot.OFFSET);
            Robot.Execute();
        }

        public override void Run()
        {
            if (Robot.RadarTurnRemaining == 0)
                Robot.SetTurnRadarLeft(360);

            if (Robot.DistanceRemaining <= 0)
            {
                Robot.CurrentPhase = RoboPhase.MeetAndGreet;
            }
        }
    }
}
