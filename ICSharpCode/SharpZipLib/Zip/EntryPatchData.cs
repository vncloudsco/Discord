namespace ICSharpCode.SharpZipLib.Zip
{
    using System;

    internal class EntryPatchData
    {
        private long sizePatchOffset_;
        private long crcPatchOffset_;

        public long SizePatchOffset
        {
            get => 
                this.sizePatchOffset_;
            set => 
                (this.sizePatchOffset_ = value);
        }

        public long CrcPatchOffset
        {
            get => 
                this.crcPatchOffset_;
            set => 
                (this.crcPatchOffset_ = value);
        }
    }
}

