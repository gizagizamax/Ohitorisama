using NAudio.Wave;

namespace Ohitorisama
{
    public class OhiKeyboardTriggerItem
    {
        public int keyCode;
        public string keyName;

        public OhiKeyboardTriggerItem(int keyCode, string keyName)
        {
            this.keyCode = keyCode;
            this.keyName = keyName;
        }

        public override string ToString()
        {
            return $"{keyName}({keyCode})";
        }
    }
}
