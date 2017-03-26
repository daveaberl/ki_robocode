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
        private Dictionary<Robot, double> robotDanger = new Dictionary<Robot, double>();

        private void ArenaObservation()
        {
            
        }

        private void MeetAndGreet()
        {
            
        }

        private void KillItWithFire()
    {
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
