namespace ICSharpCode.SharpZipLib.Zip
{
    using System;
    using System.IO;

    internal class DiskArchiveStorage : BaseArchiveStorage
    {
        private Stream temporaryStream_;
        private string fileName_;
        private string temporaryName_;

        public DiskArchiveStorage(ZipFile file) : this(file, FileUpdateMode.Safe)
        {
        }

        public DiskArchiveStorage(ZipFile file, FileUpdateMode updateMode) : base(updateMode)
        {
            if (file.Name == null)
            {
                throw new ZipException("Cant handle non file archives");
            }
            this.fileName_ = file.Name;
        }

        public override Stream ConvertTemporaryToFinal()
        {
            if (this.temporaryStream_ == null)
            {
                throw new ZipException("No temporary stream has been created");
            }
            Stream stream = null;
            string tempFileName = GetTempFileName(this.fileName_, false);
            bool flag = false;
            try
            {
                this.temporaryStream_.Close();
                File.Move(this.fileName_, tempFileName);
                File.Move(this.temporaryName_, this.fileName_);
                flag = true;
                File.Delete(tempFileName);
                stream = File.Open(this.fileName_, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (Exception)
            {
                stream = null;
                if (!flag)
                {
                    File.Move(tempFileName, this.fileName_);
                    File.Delete(this.temporaryName_);
                }
                throw;
            }
            return stream;
        }

        public override void Dispose()
        {
            if (this.temporaryStream_ != null)
            {
                this.temporaryStream_.Close();
            }
        }

        private static string GetTempFileName(string original, bool makeTempFile)
        {
            string tempFileName = null;
            if (original == null)
            {
                tempFileName = Path.GetTempFileName();
            }
            else
            {
                int num = 0;
                int second = DateTime.Now.Second;
                while (tempFileName == null)
                {
                    num++;
                    string path = $"{original}.{second}{num}.tmp";
                    if (!File.Exists(path))
                    {
                        if (!makeTempFile)
                        {
                            tempFileName = path;
                            continue;
                        }
                        try
                        {
                            using (File.Create(path))
                            {
                            }
                            tempFileName = path;
                        }
                        catch
                        {
                            second = DateTime.Now.Second;
                        }
                    }
                }
            }
            return tempFileName;
        }

        public override Stream GetTemporaryOutput()
        {
            if (this.temporaryName_ != null)
            {
                this.temporaryName_ = GetTempFileName(this.temporaryName_, true);
                this.temporaryStream_ = File.Open(this.temporaryName_, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            }
            else
            {
                this.temporaryName_ = Path.GetTempFileName();
                this.temporaryStream_ = File.Open(this.temporaryName_, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            }
            return this.temporaryStream_;
        }

        public override Stream MakeTemporaryCopy(Stream stream)
        {
            stream.Close();
            this.temporaryName_ = GetTempFileName(this.fileName_, true);
            File.Copy(this.fileName_, this.temporaryName_, true);
            this.temporaryStream_ = new FileStream(this.temporaryName_, FileMode.Open, FileAccess.ReadWrite);
            return this.temporaryStream_;
        }

        public override Stream OpenForDirectUpdate(Stream stream)
        {
            Stream stream2;
            if ((stream != null) && stream.CanWrite)
            {
                stream2 = stream;
            }
            else
            {
                if (stream != null)
                {
                    stream.Close();
                }
                stream2 = new FileStream(this.fileName_, FileMode.Open, FileAccess.ReadWrite);
            }
            return stream2;
        }
    }
}

