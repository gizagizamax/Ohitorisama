using System;

namespace Ohitorisama
{
    public class OhiConfig
    {
        public string? KeyboardTrigger { get; set; }
        public string? KeyboardSkip { get; set; }
        public string? MicProductName { get; set; }
        public string? MicRecordPath { get; set; }
        public string? VoiceTextType { get; set; }
        public string? VoiceTextPort { get; set; }
        public string? VoiceTextWhisperModel { get; set; }
        public string? ChatGptPreset { get; set; }
        public string? ChatGptApiKey { get; set; }
        public string? ChatGptModel { get; set; }
        public string? ChatGptTotalToken { get; set; }
        public string? ChatGptTemperature { get; set; }
        public string? ChatGptMaxTokens { get; set; }
        public string? ChatGptTopP { get; set; }
        public string? ChatGptFrequencyPenalty { get; set; }
        public string? ChatGptPresencePenalty { get; set; }
        public string? ChatGptMe { get; set; }
        public string? ChatGptAi { get; set; }
        public string? ChatGptStop { get; set; }
        public string? VoiceVoxPort { get; set; }
        public string? VoiceVoxSpeaker { get; set; }
        public bool VTubeStudioOn { get; set; }
        public string? VTubeStudioPort { get; set; }
    }
}
