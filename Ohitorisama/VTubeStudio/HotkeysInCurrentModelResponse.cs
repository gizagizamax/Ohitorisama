using System.Collections.Generic;

namespace Ohitorisama.VTubeStudio
{
    public class HotkeysInCurrentModelResponse
    {
        public class Data
        {
            public bool modelLoaded { get; set; }
            public string modelName { get; set; }
            public string modelID { get; set; }
            public AvailableHotkey[] availableHotkeys { get; set; }
        }
        public class AvailableHotkey
        {
            public string name { get; set; }
            public string type { get; set; }
            public string description { get; set; }
            public string file { get; set; }
            public string hotkeyID { get; set; }
            public string[] keyCombination { get; set; }
            public int onScreenButtonID { get; set; }
        }
        public string apiName { get; set; }
        public string apiVersion { get; set; }
        public long timestamp { get; set; }
        public string requestID { get; set; }
        public string messageType { get; set; }
        public Data data { get; set; }
    }
}
