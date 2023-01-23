namespace Ohitorisama.VTubeStudio
{
    public class AuthenticationResponse
    {
        public class Data
        {
        }
        public string apiName { get; set; }
        public string apiVersion { get; set; }
        public string requestID { get; set; }
        public string messageType { get; set; }
        public Data data { get; set; }
    }
}
