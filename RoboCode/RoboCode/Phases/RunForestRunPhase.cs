using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoloSpace.Phases
{
    class RunForestRunPhase : AdvancedPhase
    {
        public RunForestRunPhase(YoloBot robot) : base(robot)
        {
        }

        public override void Run()
        {
            Robot.BodyColor = System.Drawing.Color.Pink;

            double xMiddle = Robot.BattleFieldWidth / 2;
            double yMiddle = Robot.BattleFieldHeight / 2;

            Robot.SetTankHeadingTo(CoordinateHelper.GetAngle(Robot.X, Robot.Y, xMiddle, yMiddle));
            Robot.Ahead(200);

            Robot.CurrentPhase = RoboPhase.WallRush;
            Robot.TargetEnemyName = null;
            Robot.AttackerEnemyName = null;
        }
    }
}
