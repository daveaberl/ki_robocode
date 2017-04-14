using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoloSpace.Phases
{
    class MeetAndGreetPhase : AdvancedPhase
    {

        private MeetAndGreetStep currentMeetAndGreetPhase;

        public MeetAndGreetPhase(YoloBot robot) : base(robot)
        {
        }

        public override void ActivatePhase(RoboPhase previousPhase)
        {
            base.ActivatePhase(previousPhase);
            currentMeetAndGreetPhase = MeetAndGreetStep.MoveForward;
        }

        private void CheckEnemies()
        {
            foreach (var robot in Robot.KnownEnemies)
            {
                if (robot.Value.Distance < 200)
                {
                    this.Robot.TargetEnemyName = robot.Value.Name;
                    this.Robot.CurrentPhase = RoboPhase.KillItWithFire;
                }
            }
        }

        private void Navigate()
        {
            double distance = Robot.DetermineDistance(Robot.CurrentDirection);
            if (Robot.DistanceRemaining <= 0 && currentMeetAndGreetPhase == MeetAndGreetStep.DriveCurve)
            {
                if (distance > YoloBot.OFFSET)
                {
                    Robot.SetAhead(distance - YoloBot.OFFSET);
                }
                currentMeetAndGreetPhase = MeetAndGreetStep.MoveForward;
            }
            else if (Robot.DistanceRemaining <= 0 && currentMeetAndGreetPhase == MeetAndGreetStep.MoveForward)
            {
                if (Robot.DetermineDistance(Robot.DetermineLeftDirection(Robot.CurrentDirection)) > YoloBot.OFFSET)
                {
                    Robot.SetAhead(100);
                }
                Robot.SetTurnLeft(45);
                currentMeetAndGreetPhase = MeetAndGreetStep.DriveCurve;
            }
        }

        public override void Run()
        {
            Robot.BodyColor = System.Drawing.Color.Orange;

            CheckEnemies();
            Navigate();
            Robot.SetTurnRadarLeft(45);

            if (Robot.Others == 1)
            {
                Robot.TargetEnemyName = Robot.KnownEnemies.FirstOrDefault().Value?.Name;

                if (Robot.TargetEnemyName != null)
                    Robot.CurrentPhase = RoboPhase.KillItWithFire;
            }
        }

        public override string ToString()
        {
            return $"{base.ToString()} - {Enum.GetName(typeof(MeetAndGreetStep), currentMeetAndGreetPhase)}";
        }
    }
}
