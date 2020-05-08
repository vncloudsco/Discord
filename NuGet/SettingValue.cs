namespace NuGet
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal class SettingValue
    {
        public SettingValue(string key, string value, bool isMachineWide, int priority = 0)
        {
            this.Key = key;
            this.Value = value;
            this.IsMachineWide = isMachineWide;
            this.Priority = priority;
        }

        public override bool Equals(object obj)
        {
            SettingValue value2 = obj as SettingValue;
            return ((value2 != null) ? ((value2.Key == this.Key) && ((value2.Value == this.Value) && (value2.IsMachineWide == value2.IsMachineWide))) : false);
        }

        public override int GetHashCode() => 
            Tuple.Create<string, string, bool>(this.Key, this.Value, this.IsMachineWide).GetHashCode();

        public string Key { get; private set; }

        public string Value { get; private set; }

        public bool IsMachineWide { get; private set; }

        public int Priority { get; private set; }
    }
}

