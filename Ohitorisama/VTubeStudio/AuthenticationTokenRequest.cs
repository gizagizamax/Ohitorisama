namespace Ohitorisama.VTubeStudio
{
    public class AuthenticationTokenRequest
    {
        public class Data
        {
            public string? pluginName { get; set; }
            public string? pluginDeveloper { get; set; }
            public object? pluginIcon { get; set; }
        }
        public string? apiName { get; set; }
        public string? apiVersion { get; set; }
        public string? requestID { get; set; }
        public string? messageType { get; set; }
        public Data? data { get; set; }
    }
}
