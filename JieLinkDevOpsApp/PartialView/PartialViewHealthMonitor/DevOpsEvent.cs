﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartialViewHealthMonitor
{
    public class DevOpsEvent
    {
        public int EventType { get; set; }

        public string RemoteAccount { get; set; }

        public string ContactName { get; set; }

        public string ContactPhone { get; set; }

        public DateTime OperatorDate { get; set; }
    }
}
