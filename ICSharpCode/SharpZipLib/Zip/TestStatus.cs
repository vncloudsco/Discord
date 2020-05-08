namespace ICSharpCode.SharpZipLib.Zip
{
    using System;

    internal class TestStatus
    {
        private ZipFile file_;
        private ZipEntry entry_;
        private bool entryValid_;
        private int errorCount_;
        private long bytesTested_;
        private TestOperation operation_;

        public TestStatus(ZipFile file)
        {
            this.file_ = file;
        }

        internal void AddError()
        {
            this.errorCount_++;
            this.entryValid_ = false;
        }

        internal void SetBytesTested(long value)
        {
            this.bytesTested_ = value;
        }

        internal void SetEntry(ZipEntry entry)
        {
            this.entry_ = entry;
            this.entryValid_ = true;
            this.bytesTested_ = 0L;
        }

        internal void SetOperation(TestOperation operation)
        {
            this.operation_ = operation;
        }

        public TestOperation Operation =>
            this.operation_;

        public ZipFile File =>
            this.file_;

        public ZipEntry Entry =>
            this.entry_;

        public int ErrorCount =>
            this.errorCount_;

        public long BytesTested =>
            this.bytesTested_;

        public bool EntryValid =>
            this.entryValid_;
    }
}

