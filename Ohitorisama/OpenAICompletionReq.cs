namespace Ohitorisama
{
    public class OpenAICompletionReq
    {
        public string? model { get; set; }
        public string? prompt { get; set; }
        public float temperature { get; set; }
        public int max_tokens { get; set; }
        public float top_p { get; set; }
        public float frequency_penalty { get; set; }
        public float presence_penalty { get; set; }
        public string[]? stop { get; set; }
    }
}
