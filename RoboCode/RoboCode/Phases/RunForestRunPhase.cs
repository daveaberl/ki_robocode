using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoloSpace.Phases
{
    class RunForestRunPhase : IPhase
    {
        private YoloBot robot;

        public RunForestRunPhase(YoloBot robot)
        {
            this.robot = robot;
        }

        public void Run()
        {
            robot.BodyColor = System.Drawing.Color.Pink;

            double xMiddle = robot.BattleFieldWidth / 2;
            double yMiddle = robot.BattleFieldHeight / 2;

            robot.SetTankHeadingTo(CoordinateHelper.GetAngle(robot.X, robot.Y, xMiddle, yMiddle));
            robot.Ahead(200);

            robot.CurrentPhase = RoboPhase.WallRush;
            robot.TargetEnemyName = null;
            robot.AttackerEnemyName = null;
        }
    }
}
