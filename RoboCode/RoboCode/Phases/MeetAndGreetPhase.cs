using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoloSpace.Phases
{
    class MeetAndGreetPhase : IPhase
    {
        private YoloBot robot;

        public MeetAndGreetPhase(YoloBot robot)
        {
            this.robot = robot;
        }

        private void CheckEnemies()
        {
            foreach (var robot in robot.KnownEnemies)
            {
                if (robot.Value.Distance < 200)
                {
                    this.robot.TargetEnemyName = robot.Value.Name;
                    this.robot.CurrentPhase = RoboPhase.KillItWithFire;
                }
            }
        }

        private void Navigate()
        {
            if (robot.DetermineDistance(robot.CurrentDirection) < YoloBot.OFFSET)
            {
                robot.TurnLeft(1);
                robot.TurnLeft(robot.Heading % 90);
            }
            robot.SetAhead(20);
        }

        public void Run()
        {
            if (robot.Others == 1)
            {
                robot.CurrentPhase = RoboPhase.KillItWithFire;
            }
            else
            {
                robot.BodyColor = System.Drawing.Color.Orange;

                CheckEnemies();
                Navigate();
                robot.TurnRadarLeft(45);
                robot.Execute();
            }
        }
    }
}
