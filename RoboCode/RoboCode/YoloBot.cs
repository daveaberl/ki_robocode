using Robocode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoloSpace
{
    class YoloBot : AdvancedRobot
    {
        private const double DANGER_THRESHOLD = 0x0;

        private RoboPhase CurrentPhase { get; set; } = RoboPhase.ArenaObservation;
        private Dictionary<ScannedRobotEvent, double> robotDanger =
            new Dictionary<ScannedRobotEvent, double>();

        private Dictionary<string, ScannedRobotEvent> robots =
            new Dictionary<string, ScannedRobotEvent>();

        private HitByBulletEvent lastBulletHit;

        private void ArenaObservation()
        {
            CurrentPhase = RoboPhase.MeetAndGreet;
        }

        private void FollowWall()
        {
            // Check wall

            SetAhead(10);
        }

        private double CalculateDanger(ScannedRobotEvent enemy)
            => enemy.Distance;

        public override void OnScannedRobot(ScannedRobotEvent evnt)
        {
            base.OnScannedRobot(evnt);

            robotDanger[evnt] = CalculateDanger(evnt);
            robots[evnt.Name] = evnt;
        }

        private void MeetAndGreet()
        {
            FollowWall();
            TurnRadarLeft(10);

            Execute();
        }

        public override void OnHitByBullet(HitByBulletEvent evnt)
        {
            if (lastBulletHit == null &&
                CurrentPhase == RoboPhase.MeetAndGreet)
            {
                lastBulletHit = evnt;
                CurrentPhase = RoboPhase.KillItWithFire;
            }

            base.OnHitByBullet(evnt);
        }
        public override void OnRobotDeath(RobotDeathEvent evnt)
        {
            if (lastBulletHit?.Name == evnt.Name)
            {
                lastBulletHit = null;
                CurrentPhase = RoboPhase.MeetAndGreet;
            }

            base.OnRobotDeath(evnt);
        }

        private void KillItWithFire()
        {
            FollowWall();

            if (lastBulletHit != null)
            {
                if (lastBulletHit.Bearing < 0)
                    SetTurnRadarLeft(lastBulletHit.Bearing);
                else
                    SetTurnRadarRight(lastBulletHit.Bearing);
            }


            Execute();
        }

        public override void Run()
        {
            while (true)
            {
                switch (CurrentPhase)
                {
                    case RoboPhase.ArenaObservation:
                        ArenaObservation();
                        break;
                    case RoboPhase.MeetAndGreet:
                        MeetAndGreet();
                        break;
                    case RoboPhase.KillItWithFire:
                        KillItWithFire();
                        break;
                }
            }
        }
    }
}
