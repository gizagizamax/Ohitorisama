namespace Ohitorisama.VTubeStudio
{
    public class AuthenticationTokenResponse
    {
        public class Data
        {
            public string authenticationToken { get; set; }
        }
        public string apiName { get; set; }
        public string apiVersion { get; set; }
        public long timestamp { get; set; }
        public string requestID { get; set; }
        public string messageType { get; set; }
        public Data data { get; set; }
    }
}
