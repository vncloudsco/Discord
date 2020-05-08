namespace ICSharpCode.SharpZipLib.Core
{
    using System;
    using System.IO;

    internal class FileSystemScanner
    {
        public ProcessDirectoryHandler ProcessDirectory;
        public ProcessFileHandler ProcessFile;
        public CompletedFileHandler CompletedFile;
        public DirectoryFailureHandler DirectoryFailure;
        public FileFailureHandler FileFailure;
        private IScanFilter fileFilter_;
        private IScanFilter directoryFilter_;
        private bool alive_;

        public FileSystemScanner(IScanFilter fileFilter)
        {
            this.fileFilter_ = fileFilter;
        }

        public FileSystemScanner(string filter)
        {
            this.fileFilter_ = new PathFilter(filter);
        }

        public FileSystemScanner(IScanFilter fileFilter, IScanFilter directoryFilter)
        {
            this.fileFilter_ = fileFilter;
            this.directoryFilter_ = directoryFilter;
        }

        public FileSystemScanner(string fileFilter, string directoryFilter)
        {
            this.fileFilter_ = new PathFilter(fileFilter);
            this.directoryFilter_ = new PathFilter(directoryFilter);
        }

        private void OnCompleteFile(string file)
        {
            CompletedFileHandler completedFile = this.CompletedFile;
            if (completedFile != null)
            {
                ScanEventArgs e = new ScanEventArgs(file);
                completedFile(this, e);
                this.alive_ = e.ContinueRunning;
            }
        }

        private bool OnDirectoryFailure(string directory, Exception e)
        {
            DirectoryFailureHandler directoryFailure = this.DirectoryFailure;
            bool flag1 = directoryFailure != null;
            if (flag1)
            {
                ScanFailureEventArgs args = new ScanFailureEventArgs(directory, e);
                directoryFailure(this, args);
                this.alive_ = args.ContinueRunning;
            }
            return flag1;
        }

        private bool OnFileFailure(string file, Exception e)
        {
            bool flag1 = this.FileFailure != null;
            if (flag1)
            {
                ScanFailureEventArgs args = new ScanFailureEventArgs(file, e);
                this.FileFailure(this, args);
                this.alive_ = args.ContinueRunning;
            }
            return flag1;
        }

        private void OnProcessDirectory(string directory, bool hasMatchingFiles)
        {
            ProcessDirectoryHandler processDirectory = this.ProcessDirectory;
            if (processDirectory != null)
            {
                DirectoryEventArgs e = new DirectoryEventArgs(directory, hasMatchingFiles);
                processDirectory(this, e);
                this.alive_ = e.ContinueRunning;
            }
        }

        private void OnProcessFile(string file)
        {
            ProcessFileHandler processFile = this.ProcessFile;
            if (processFile != null)
            {
                ScanEventArgs e = new ScanEventArgs(file);
                processFile(this, e);
                this.alive_ = e.ContinueRunning;
            }
        }

        public void Scan(string directory, bool recurse)
        {
            this.alive_ = true;
            this.ScanDir(directory, recurse);
        }

        private void ScanDir(string directory, bool recurse)
        {
            try
            {
                string[] strArray2;
                int num2;
                string[] files = Directory.GetFiles(directory);
                bool hasMatchingFiles = false;
                int index = 0;
                while (true)
                {
                    if (index < files.Length)
                    {
                        if (!this.fileFilter_.IsMatch(files[index]))
                        {
                            files[index] = null;
                        }
                        else
                        {
                            hasMatchingFiles = true;
                        }
                        index++;
                        continue;
                    }
                    this.OnProcessDirectory(directory, hasMatchingFiles);
                    if (!(this.alive_ & hasMatchingFiles))
                    {
                        goto TR_000E;
                    }
                    else
                    {
                        strArray2 = files;
                        num2 = 0;
                    }
                    break;
                }
                goto TR_001A;
            TR_0012:
                num2++;
            TR_001A:
                while (true)
                {
                    if (num2 < strArray2.Length)
                    {
                        string file = strArray2[num2];
                        try
                        {
                            if (file == null)
                            {
                                goto TR_0012;
                            }
                            else
                            {
                                this.OnProcessFile(file);
                                if (this.alive_)
                                {
                                    goto TR_0012;
                                }
                            }
                        }
                        catch (Exception exception1)
                        {
                            if (!this.OnFileFailure(file, exception1))
                            {
                                throw;
                            }
                            goto TR_0012;
                        }
                    }
                    break;
                }
            }
            catch (Exception exception2)
            {
                if (!this.OnDirectoryFailure(directory, exception2))
                {
                    throw;
                }
            }
        TR_000E:
            if (this.alive_ & recurse)
            {
                try
                {
                    foreach (string str2 in Directory.GetDirectories(directory))
                    {
                        if ((this.directoryFilter_ == null) || this.directoryFilter_.IsMatch(str2))
                        {
                            this.ScanDir(str2, true);
                            if (!this.alive_)
                            {
                                break;
                            }
                        }
                    }
                }
                catch (Exception exception3)
                {
                    if (!this.OnDirectoryFailure(directory, exception3))
                    {
                        throw;
                    }
                }
            }
        }
    }
}

