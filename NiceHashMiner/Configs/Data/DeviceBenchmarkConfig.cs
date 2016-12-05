﻿using NiceHashMiner.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NiceHashMiner.Configs.Data {
    [Serializable]
    public class DeviceBenchmarkConfig {
        public string DeviceUUID = "";
        public string DeviceName = "";
        //public int TimeLimit { get; set; }
        public Dictionary<AlgorithmType, AlgorithmConfig> AlgorithmSettings = new Dictionary<AlgorithmType, AlgorithmConfig>();
    }
}
