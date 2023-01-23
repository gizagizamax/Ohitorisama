using NAudio.Wave;

namespace Ohitorisama
{
    public class OhiComboItem
    {
        public string text;
        public WaveInCapabilities val;

        public OhiComboItem(string text, WaveInCapabilities val)
        {
            this.text = text;
            this.val = val;
        }

        public override string ToString()
        {
            return text;
        }
    }
}
