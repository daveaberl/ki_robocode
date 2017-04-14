using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoloSpace
{
    public enum RoboPhase
    {
        WallRush,
        MeetAndGreet,
        RunForestRun,
        KillItWithFire,
        KillingItSoftly,
        UnknownPhase
    }

    enum Direction { NORTH, EAST, SOUTH, WEST, UNKOWN };

    enum KillItWithFireStep
    {
        MoveFromWall,
        Dodge,
        EscapeTheDangerZone
    }

    enum MeetAndGreetStep
    {
        DriveCurve,
        MoveForward
    }
}
