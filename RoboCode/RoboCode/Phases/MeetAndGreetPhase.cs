using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoloSpace.Phases
{
    class MeetAndGreetPhase : AdvancedPhase
    {
        public const double OFFSET = 60;
        public const double SMALLER_OFFSET = OFFSET - 10;

        private MeetAndGreetStep currentMeetAndGreetPhase;

        public MeetAndGreetPhase(YoloBot robot) : base(robot)
        {
        }

        public override void ActivatePhase(RoboPhase previousPhase)
        {
            base.ActivatePhase(previousPhase);
            currentMeetAndGreetPhase = MeetAndGreetStep.MoveForward;
            Robot.SetTurnLeft(Robot.Heading % 90);
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
            if (Robot.TurnRemaining == 0)
            {
                double distance = Robot.DetermineDistance(Robot.CurrentDirection);
                if (Robot.DistanceRemaining <= 0 && currentMeetAndGreetPhase == MeetAndGreetStep.DriveCurve)
                {
                    if (distance > SMALLER_OFFSET)
                    {
                        Robot.SetAhead(distance - OFFSET);
                    }
                    currentMeetAndGreetPhase = MeetAndGreetStep.MoveForward;
                }
                else if (Robot.DistanceRemaining <= 0 && currentMeetAndGreetPhase == MeetAndGreetStep.MoveForward)
                {
                    if (Robot.DetermineDistance(Robot.DetermineLeftDirection(Robot.CurrentDirection)) > SMALLER_OFFSET && distance >= SMALLER_OFFSET)
                    {
                        Robot.SetAhead(50);
                    }
                    Robot.SetTurnLeft(90);
                    currentMeetAndGreetPhase = MeetAndGreetStep.DriveCurve;
                }
            }
        }

        public override void Run()
        {
            Robot.BodyColor = System.Drawing.Color.Orange;

            if (Robot.RadarTurnRemaining == 0)
                Robot.SetTurnRadarLeft(360);

            CheckEnemies();
            Navigate();

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
