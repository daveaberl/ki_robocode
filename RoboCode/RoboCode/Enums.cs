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
        KillingItSoftly
    }


    enum Direction { NORTH, EAST, SOUTH, WEST, UNKOWN };

    enum KillItWithFireStep
    {
        MoveFromWall,
        Positioning,
        Dodge
    }
}
