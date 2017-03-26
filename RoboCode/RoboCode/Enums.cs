﻿using System;
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
        KillItWithFire,
        KillingItSoftly
    }


    enum Direction { NORTH, EAST, SOUTH, WEST, UNKOWN };

    enum KillItWithFirePhase
    {
        MoveFromWall,
        Positioning,
        Dodge
    }
}