﻿using System;
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
                var target = robot.Heading % 90;

                if (target == 0) target = 90;

                robot.TurnLeft(target);
            }
            robot.SetAhead(100);
        }

        public void Run()
        {
            robot.BodyColor = System.Drawing.Color.Orange;

            CheckEnemies();
            Navigate();
            robot.TurnRadarLeft(45);
            robot.Execute();

            if (robot.Others == 1)
            {
                robot.TargetEnemyName = robot.KnownEnemies.FirstOrDefault().Value?.Name;

                if (robot.TargetEnemyName != null)
                    robot.CurrentPhase = RoboPhase.KillItWithFire;
            }
        }
    }
}
