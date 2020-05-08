namespace ICSharpCode.SharpZipLib.Tar
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;

    internal class TarArchive : IDisposable
    {
        [CompilerGenerated]
        private ProgressMessageHandler ProgressMessageEvent;
        private bool keepOldFiles;
        private bool asciiTranslate;
        private int userId;
        private string userName;
        private int groupId;
        private string groupName;
        private string rootPath;
        private string pathPrefix;
        private bool applyUserInfoOverrides;
        private TarInputStream tarIn;
        private TarOutputStream tarOut;
        private bool isDisposed;

        public event ProgressMessageHandler ProgressMessageEvent
        {
            [CompilerGenerated] add
            {
                ProgressMessageHandler progressMessageEvent = this.ProgressMessageEvent;
                while (true)
                {
                    ProgressMessageHandler a = progressMessageEvent;
                    ProgressMessageHandler handler3 = (ProgressMessageHandler) Delegate.Combine(a, value);
                    progressMessageEvent = Interlocked.CompareExchange<ProgressMessageHandler>(ref this.ProgressMessageEvent, handler3, a);
                    if (ReferenceEquals(progressMessageEvent, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                ProgressMessageHandler progressMessageEvent = this.ProgressMessageEvent;
                while (true)
                {
                    ProgressMessageHandler source = progressMessageEvent;
                    ProgressMessageHandler handler3 = (ProgressMessageHandler) Delegate.Remove(source, value);
                    progressMessageEvent = Interlocked.CompareExchange<ProgressMessageHandler>(ref this.ProgressMessageEvent, handler3, source);
                    if (ReferenceEquals(progressMessageEvent, source))
                    {
                        return;
                    }
                }
            }
        }

        protected TarArchive()
        {
            this.userName = string.Empty;
            this.groupName = string.Empty;
        }

        protected TarArchive(TarInputStream stream)
        {
            this.userName = string.Empty;
            this.groupName = string.Empty;
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            this.tarIn = stream;
        }

        protected TarArchive(TarOutputStream stream)
        {
            this.userName = string.Empty;
            this.groupName = string.Empty;
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            this.tarOut = stream;
        }

        public virtual void Close()
        {
            this.Dispose(true);
        }

        [Obsolete("Use Close instead")]
        public void CloseArchive()
        {
            this.Close();
        }

        public static TarArchive CreateInputTarArchive(Stream inputStream)
        {
            if (inputStream == null)
            {
                throw new ArgumentNullException("inputStream");
            }
            TarInputStream stream = inputStream as TarInputStream;
            return ((stream == null) ? CreateInputTarArchive(inputStream, 20) : new TarArchive(stream));
        }

        public static TarArchive CreateInputTarArchive(Stream inputStream, int blockFactor)
        {
            if (inputStream == null)
            {
                throw new ArgumentNullException("inputStream");
            }
            if (inputStream is TarInputStream)
            {
                throw new ArgumentException("TarInputStream not valid");
            }
            return new TarArchive(new TarInputStream(inputStream, blockFactor));
        }

        public static TarArchive CreateOutputTarArchive(Stream outputStream)
        {
            if (outputStream == null)
            {
                throw new ArgumentNullException("outputStream");
            }
            TarOutputStream stream = outputStream as TarOutputStream;
            return ((stream == null) ? CreateOutputTarArchive(outputStream, 20) : new TarArchive(stream));
        }

        public static TarArchive CreateOutputTarArchive(Stream outputStream, int blockFactor)
        {
            if (outputStream == null)
            {
                throw new ArgumentNullException("outputStream");
            }
            if (outputStream is TarOutputStream)
            {
                throw new ArgumentException("TarOutputStream is not valid");
            }
            return new TarArchive(new TarOutputStream(outputStream, blockFactor));
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                this.isDisposed = true;
                if (disposing)
                {
                    if (this.tarOut != null)
                    {
                        this.tarOut.Flush();
                        this.tarOut.Close();
                    }
                    if (this.tarIn != null)
                    {
                        this.tarIn.Close();
                    }
                }
            }
        }

        private static void EnsureDirectoryExists(string directoryName)
        {
            if (!Directory.Exists(directoryName))
            {
                try
                {
                    Directory.CreateDirectory(directoryName);
                }
                catch (Exception exception)
                {
                    throw new TarException("Exception creating directory '" + directoryName + "', " + exception.Message, exception);
                }
            }
        }

        public void ExtractContents(string destinationDirectory)
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException("TarArchive");
            }
            while (true)
            {
                TarEntry nextEntry = this.tarIn.GetNextEntry();
                if (nextEntry == null)
                {
                    return;
                }
                this.ExtractEntry(destinationDirectory, nextEntry);
            }
        }

        private void ExtractEntry(string destDir, TarEntry entry)
        {
            this.OnProgressMessageEvent(entry, null);
            string name = entry.Name;
            if (Path.IsPathRooted(name))
            {
                name = name.Substring(Path.GetPathRoot(name).Length);
            }
            name = name.Replace('/', Path.DirectorySeparatorChar);
            string directoryName = Path.Combine(destDir, name);
            if (entry.IsDirectory)
            {
                EnsureDirectoryExists(directoryName);
            }
            else
            {
                EnsureDirectoryExists(Path.GetDirectoryName(directoryName));
                bool flag = true;
                FileInfo info = new FileInfo(directoryName);
                if (info.Exists)
                {
                    if (this.keepOldFiles)
                    {
                        this.OnProgressMessageEvent(entry, "Destination file already exists");
                        flag = false;
                    }
                    else if ((info.Attributes & FileAttributes.ReadOnly) != 0)
                    {
                        this.OnProgressMessageEvent(entry, "Destination file already exists, and is read-only");
                        flag = false;
                    }
                }
                if (flag)
                {
                    bool flag2 = false;
                    Stream stream = File.Create(directoryName);
                    if (this.asciiTranslate)
                    {
                        flag2 = !IsBinary(directoryName);
                    }
                    StreamWriter writer = null;
                    if (flag2)
                    {
                        writer = new StreamWriter(stream);
                    }
                    byte[] buffer = new byte[0x8000];
                    while (true)
                    {
                        int count = this.tarIn.Read(buffer, 0, buffer.Length);
                        if (count <= 0)
                        {
                            if (!flag2)
                            {
                                stream.Close();
                                break;
                            }
                            writer.Close();
                            return;
                        }
                        if (!flag2)
                        {
                            stream.Write(buffer, 0, count);
                            continue;
                        }
                        int index = 0;
                        for (int i = 0; i < count; i++)
                        {
                            if (buffer[i] == 10)
                            {
                                writer.WriteLine(Encoding.ASCII.GetString(buffer, index, i - index));
                                index = i + 1;
                            }
                        }
                    }
                }
            }
        }

        ~TarArchive()
        {
            this.Dispose(false);
        }

        private static bool IsBinary(string filename)
        {
            bool flag;
            using (FileStream stream = File.OpenRead(filename))
            {
                int count = Math.Min(0x1000, (int) stream.Length);
                byte[] buffer = new byte[count];
                int num2 = stream.Read(buffer, 0, count);
                int index = 0;
                while (true)
                {
                    if (index < num2)
                    {
                        byte num4 = buffer[index];
                        if (((num4 >= 8) && ((num4 <= 13) || (num4 >= 0x20))) && (num4 != 0xff))
                        {
                            index++;
                            continue;
                        }
                        flag = true;
                    }
                    else
                    {
                        return false;
                    }
                    break;
                }
            }
            return flag;
        }

        public void ListContents()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException("TarArchive");
            }
            while (true)
            {
                TarEntry nextEntry = this.tarIn.GetNextEntry();
                if (nextEntry == null)
                {
                    return;
                }
                this.OnProgressMessageEvent(nextEntry, null);
            }
        }

        protected virtual void OnProgressMessageEvent(TarEntry entry, string message)
        {
            ProgressMessageHandler progressMessageEvent = this.ProgressMessageEvent;
            if (progressMessageEvent != null)
            {
                progressMessageEvent(this, entry, message);
            }
        }

        [Obsolete("Use the AsciiTranslate property")]
        public void SetAsciiTranslation(bool translateAsciiFiles)
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException("TarArchive");
            }
            this.asciiTranslate = translateAsciiFiles;
        }

        public void SetKeepOldFiles(bool keepExistingFiles)
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException("TarArchive");
            }
            this.keepOldFiles = keepExistingFiles;
        }

        public void SetUserInfo(int userId, string userName, int groupId, string groupName)
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException("TarArchive");
            }
            this.userId = userId;
            this.userName = userName;
            this.groupId = groupId;
            this.groupName = groupName;
            this.applyUserInfoOverrides = true;
        }

        public void WriteEntry(TarEntry sourceEntry, bool recurse)
        {
            if (sourceEntry == null)
            {
                throw new ArgumentNullException("sourceEntry");
            }
            if (this.isDisposed)
            {
                throw new ObjectDisposedException("TarArchive");
            }
            try
            {
                if (recurse)
                {
                    TarHeader.SetValueDefaults(sourceEntry.UserId, sourceEntry.UserName, sourceEntry.GroupId, sourceEntry.GroupName);
                }
                this.WriteEntryCore(sourceEntry, recurse);
            }
            finally
            {
                if (recurse)
                {
                    TarHeader.RestoreSetValues();
                }
            }
        }

        private void WriteEntryCore(TarEntry sourceEntry, bool recurse)
        {
            string path = null;
            string str3;
            string file = sourceEntry.File;
            TarEntry entry = (TarEntry) sourceEntry.Clone();
            if (this.applyUserInfoOverrides)
            {
                entry.GroupId = this.groupId;
                entry.GroupName = this.groupName;
                entry.UserId = this.userId;
                entry.UserName = this.userName;
            }
            this.OnProgressMessageEvent(entry, null);
            if (this.asciiTranslate && (!entry.IsDirectory && !IsBinary(file)))
            {
                path = Path.GetTempFileName();
                using (StreamReader reader = File.OpenText(file))
                {
                    Stream stream = File.Create(path);
                    while (true)
                    {
                        try
                        {
                            while (true)
                            {
                                string s = reader.ReadLine();
                                if (s == null)
                                {
                                    stream.Flush();
                                    entry.Size = new FileInfo(path).Length;
                                    file = path;
                                    goto TR_0022;
                                }
                                else
                                {
                                    byte[] bytes = Encoding.ASCII.GetBytes(s);
                                    stream.Write(bytes, 0, bytes.Length);
                                    stream.WriteByte(10);
                                }
                                break;
                            }
                        }
                        finally
                        {
                            if (stream != null)
                            {
                                stream.Dispose();
                            }
                        }
                    }
                }
            }
        TR_0022:
            str3 = null;
            if ((this.rootPath != null) && entry.Name.StartsWith(this.rootPath, StringComparison.OrdinalIgnoreCase))
            {
                str3 = entry.Name.Substring(this.rootPath.Length + 1);
            }
            if (this.pathPrefix != null)
            {
                str3 = (str3 == null) ? (this.pathPrefix + "/" + entry.Name) : (this.pathPrefix + "/" + str3);
            }
            if (str3 != null)
            {
                entry.Name = str3;
            }
            this.tarOut.PutNextEntry(entry);
            if (entry.IsDirectory)
            {
                if (recurse)
                {
                    TarEntry[] directoryEntries = entry.GetDirectoryEntries();
                    for (int i = 0; i < directoryEntries.Length; i++)
                    {
                        this.WriteEntryCore(directoryEntries[i], recurse);
                    }
                }
            }
            else
            {
                using (Stream stream2 = File.OpenRead(file))
                {
                    byte[] buffer = new byte[0x8000];
                    while (true)
                    {
                        int count = stream2.Read(buffer, 0, buffer.Length);
                        if (count <= 0)
                        {
                            break;
                        }
                        this.tarOut.Write(buffer, 0, count);
                    }
                }
                if ((path != null) && (path.Length > 0))
                {
                    File.Delete(path);
                }
                this.tarOut.CloseEntry();
            }
        }

        public bool AsciiTranslate
        {
            get
            {
                if (this.isDisposed)
                {
                    throw new ObjectDisposedException("TarArchive");
                }
                return this.asciiTranslate;
            }
            set
            {
                if (this.isDisposed)
                {
                    throw new ObjectDisposedException("TarArchive");
                }
                this.asciiTranslate = value;
            }
        }

        public string PathPrefix
        {
            get
            {
                if (this.isDisposed)
                {
                    throw new ObjectDisposedException("TarArchive");
                }
                return this.pathPrefix;
            }
            set
            {
                if (this.isDisposed)
                {
                    throw new ObjectDisposedException("TarArchive");
                }
                this.pathPrefix = value;
            }
        }

        public string RootPath
        {
            get
            {
                if (this.isDisposed)
                {
                    throw new ObjectDisposedException("TarArchive");
                }
                return this.rootPath;
            }
            set
            {
                if (this.isDisposed)
                {
                    throw new ObjectDisposedException("TarArchive");
                }
                char[] trimChars = new char[] { '/' };
                this.rootPath = value.Replace('\\', '/').TrimEnd(trimChars);
            }
        }

        public bool ApplyUserInfoOverrides
        {
            get
            {
                if (this.isDisposed)
                {
                    throw new ObjectDisposedException("TarArchive");
                }
                return this.applyUserInfoOverrides;
            }
            set
            {
                if (this.isDisposed)
                {
                    throw new ObjectDisposedException("TarArchive");
                }
                this.applyUserInfoOverrides = value;
            }
        }

        public int UserId
        {
            get
            {
                if (this.isDisposed)
                {
                    throw new ObjectDisposedException("TarArchive");
                }
                return this.userId;
            }
        }

        public string UserName
        {
            get
            {
                if (this.isDisposed)
                {
                    throw new ObjectDisposedException("TarArchive");
                }
                return this.userName;
            }
        }

        public int GroupId
        {
            get
            {
                if (this.isDisposed)
                {
                    throw new ObjectDisposedException("TarArchive");
                }
                return this.groupId;
            }
        }

        public string GroupName
        {
            get
            {
                if (this.isDisposed)
                {
                    throw new ObjectDisposedException("TarArchive");
                }
                return this.groupName;
            }
        }

        public int RecordSize
        {
            get
            {
                if (this.isDisposed)
                {
                    throw new ObjectDisposedException("TarArchive");
                }
                return ((this.tarIn == null) ? ((this.tarOut == null) ? 0x2800 : this.tarOut.RecordSize) : this.tarIn.RecordSize);
            }
        }

        public bool IsStreamOwner
        {
            set
            {
                if (this.tarIn != null)
                {
                    this.tarIn.IsStreamOwner = value;
                }
                else
                {
                    this.tarOut.IsStreamOwner = value;
                }
            }
        }
    }
}

