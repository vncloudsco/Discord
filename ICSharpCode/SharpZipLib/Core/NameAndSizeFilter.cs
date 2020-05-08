namespace ICSharpCode.SharpZipLib.Core
{
    using System;
    using System.IO;

    [Obsolete("Use ExtendedPathFilter instead")]
    internal class NameAndSizeFilter : PathFilter
    {
        private long minSize_;
        private long maxSize_;

        public NameAndSizeFilter(string filter, long minSize, long maxSize) : base(filter)
        {
            this.maxSize_ = 0x7fffffffffffffffL;
            this.MinSize = minSize;
            this.MaxSize = maxSize;
        }

        public override bool IsMatch(string name)
        {
            bool flag = base.IsMatch(name);
            if (flag)
            {
                long length = new FileInfo(name).Length;
                flag = (this.MinSize <= length) && (this.MaxSize >= length);
            }
            return flag;
        }

        public long MinSize
        {
            get => 
                this.minSize_;
            set
            {
                if ((value < 0L) || (this.maxSize_ < value))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.minSize_ = value;
            }
        }

        public long MaxSize
        {
            get => 
                this.maxSize_;
            set
            {
                if ((value < 0L) || (this.minSize_ > value))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.maxSize_ = value;
            }
        }
    }
}

