using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoloSpace.Phases
{
    class RunForestRunPhase : AdvancedPhase
    {
        private bool isDriving = false;
        private bool isTurning = false;

        public RunForestRunPhase(YoloBot robot) : base(robot)
        {
        }

        public override void ActivatePhase(RoboPhase previousPhase)
        {
            isDriving = false;
            isTurning = false;
        }

        public override void Run()
        {
            Robot.BodyColor = System.Drawing.Color.Pink;

            if (Robot.RadarTurnRemaining == 0)
                Robot.SetTurnRadarLeft(360);

            double xMiddle = Robot.BattleFieldWidth / 2;
            double yMiddle = Robot.BattleFieldHeight / 2;

            if (!isTurning && !isDriving)
            {
                Robot.SetTankHeadingTo(CoordinateHelper.GetAngle(Robot.X, Robot.Y, xMiddle, yMiddle));
                isTurning = true;
            }
            else if(Robot.TurnRemaining == 0 && !isDriving)
            {
                Robot.SetAhead(200);
                isDriving = true;
            }
            else if (Robot.DistanceRemaining == 0 && Robot.TurnRemaining == 0)
            {
                Robot.CurrentPhase = RoboPhase.WallRush;
                Robot.TargetEnemyName = null;
                Robot.AttackerEnemyName = null;
            }
        }
    }
}
