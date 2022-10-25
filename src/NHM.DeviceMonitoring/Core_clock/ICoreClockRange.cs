﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHM.DeviceMonitoring.Core_clock
{
    public interface ICoreClockRange
    {
        (int min, int max) CoreClockRange { get; }
        (int min, int max) CoreClockDeltaRange { get; }
    }
}
