using NAudio.Wave;
using System.Collections.Generic;

namespace Ohitorisama
{
    public class OpenAICompletionHistory
    {
        public OpenAICompletionRes? openAICompletionRes { get; set; }
        public string? new_prompt { get; set; }
        public int new_token { get; set; }
        public List<IWaveProvider>? waveList { get; set; }
        public int playedSeq { get; set; }
    }
}
