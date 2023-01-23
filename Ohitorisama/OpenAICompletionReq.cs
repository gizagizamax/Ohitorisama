namespace Ohitorisama
{
    public class OpenAICompletionReq
    {
        public string? model { get; set; }
        public string? prompt { get; set; }
        public int max_tokens { get; set; }
        public float temperature { get; set; }
    }
}
