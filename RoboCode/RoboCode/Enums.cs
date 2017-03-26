using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoloSpace
{
    public enum RoboPhase
    {
        ArenaObservation,
        MeetAndGreet,
        KillItWithFire
    }


    enum Direction { NORTH, EAST, SOUTH, WEST, UNKOWN };

    enum KillItWithFirePhase
    {
        MoveFromWall,
        Positioning,
        Dodge
    }
}
