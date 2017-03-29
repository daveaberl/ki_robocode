using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoloSpace.Phases
{
    class WallRushPhase : IPhase
    {
        private YoloBot robot;

        public WallRushPhase(YoloBot robot)
        {
            this.robot = robot;
        }

        public void Run()
        {
            robot.BodyColor = System.Drawing.Color.Black;

            double distance = robot.DetermineDistance(robot.CurrentDirection);
            robot.Ahead(distance - YoloBot.OFFSET);
            robot.CurrentPhase = RoboPhase.MeetAndGreet;
        }
    }
}
