namespace ICSharpCode.SharpZipLib.Zip
{
    using System;
    using System.IO;

    internal abstract class BaseArchiveStorage : IArchiveStorage
    {
        private FileUpdateMode updateMode_;

        protected BaseArchiveStorage(FileUpdateMode updateMode)
        {
            this.updateMode_ = updateMode;
        }

        public abstract Stream ConvertTemporaryToFinal();
        public abstract void Dispose();
        public abstract Stream GetTemporaryOutput();
        public abstract Stream MakeTemporaryCopy(Stream stream);
        public abstract Stream OpenForDirectUpdate(Stream stream);

        public FileUpdateMode UpdateMode =>
            this.updateMode_;
    }
}

