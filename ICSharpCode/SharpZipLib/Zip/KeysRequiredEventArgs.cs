namespace ICSharpCode.SharpZipLib.Zip
{
    using System;

    internal class KeysRequiredEventArgs : EventArgs
    {
        private string fileName;
        private byte[] key;

        public KeysRequiredEventArgs(string name)
        {
            this.fileName = name;
        }

        public KeysRequiredEventArgs(string name, byte[] keyValue)
        {
            this.fileName = name;
            this.key = keyValue;
        }

        public string FileName =>
            this.fileName;

        public byte[] Key
        {
            get => 
                this.key;
            set => 
                (this.key = value);
        }
    }
}

