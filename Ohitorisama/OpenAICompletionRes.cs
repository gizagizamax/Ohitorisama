using System.Collections.Generic;

namespace Ohitorisama
{
    public class OpenAICompletionRes
    {
        public class Choice
        {
            public string? text { get; set; }
            public int index { get; set; }
            public string? logprobs { get; set; }
            public string? finish_reason { get; set; }
        }
        public class Usage
        {
            public int prompt_tokens { get; set; }
            public int completion_tokens { get; set; }
            public int total_tokens { get; set; }
        }
        public string? id { get; set; }
        public long created { get; set; }
        public string? model { get; set; }
        public List<Choice>? choices { get; set; }
        public Usage? usage { get; set; }
    }
}
