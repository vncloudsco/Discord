namespace ICSharpCode.SharpZipLib.Zip
{
    using ICSharpCode.SharpZipLib.Core;
    using System;
    using System.IO;
    using System.Text;

    internal class WindowsNameTransform : INameTransform
    {
        private const int MaxPath = 260;
        private string _baseDirectory;
        private bool _trimIncomingPaths;
        private char _replacementChar;
        private static readonly char[] InvalidEntryChars;

        static WindowsNameTransform()
        {
            char[] invalidPathChars = Path.GetInvalidPathChars();
            int num = invalidPathChars.Length + 3;
            InvalidEntryChars = new char[num];
            Array.Copy(invalidPathChars, 0, InvalidEntryChars, 0, invalidPathChars.Length);
            InvalidEntryChars[num - 1] = '*';
            InvalidEntryChars[num - 2] = '?';
            InvalidEntryChars[num - 3] = ':';
        }

        public WindowsNameTransform()
        {
            this._replacementChar = '_';
        }

        public WindowsNameTransform(string baseDirectory)
        {
            this._replacementChar = '_';
            if (baseDirectory == null)
            {
                throw new ArgumentNullException("baseDirectory", "Directory name is invalid");
            }
            this.BaseDirectory = baseDirectory;
        }

        public static bool IsValidName(string name) => 
            ((name != null) && ((name.Length <= 260) && (string.Compare(name, MakeValidName(name, '_')) == 0)));

        public static string MakeValidName(string name, char replacement)
        {
            int num;
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            name = WindowsPathUtils.DropPathRoot(name.Replace("/", Path.DirectorySeparatorChar.ToString()));
            while ((name.Length > 0) && (name[0] == Path.DirectorySeparatorChar))
            {
                name = name.Remove(0, 1);
            }
            while ((name.Length > 0) && (name[name.Length - 1] == Path.DirectorySeparatorChar))
            {
                name = name.Remove(name.Length - 1, 1);
            }
            for (num = name.IndexOf(string.Format("{0}{0}", Path.DirectorySeparatorChar)); num >= 0; num = name.IndexOf(Path.DirectorySeparatorChar))
            {
                name = name.Remove(num, 1);
            }
            num = name.IndexOfAny(InvalidEntryChars);
            if (num >= 0)
            {
                StringBuilder builder = new StringBuilder(name);
                while (true)
                {
                    if (num < 0)
                    {
                        name = builder.ToString();
                        break;
                    }
                    builder[num] = replacement;
                    num = (num < name.Length) ? name.IndexOfAny(InvalidEntryChars, num + 1) : -1;
                }
            }
            if (name.Length > 260)
            {
                throw new PathTooLongException();
            }
            return name;
        }

        public string TransformDirectory(string name)
        {
            name = this.TransformFile(name);
            if (name.Length <= 0)
            {
                throw new ZipException("Cannot have an empty directory name");
            }
            while (true)
            {
                char directorySeparatorChar = Path.DirectorySeparatorChar;
                if (!name.EndsWith(directorySeparatorChar.ToString()))
                {
                    return name;
                }
                name = name.Remove(name.Length - 1, 1);
            }
        }

        public string TransformFile(string name)
        {
            if (name == null)
            {
                name = string.Empty;
            }
            else
            {
                name = MakeValidName(name, this._replacementChar);
                if (this._trimIncomingPaths)
                {
                    name = Path.GetFileName(name);
                }
                if (this._baseDirectory != null)
                {
                    name = Path.Combine(this._baseDirectory, name);
                }
            }
            return name;
        }

        public string BaseDirectory
        {
            get => 
                this._baseDirectory;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this._baseDirectory = Path.GetFullPath(value);
            }
        }

        public bool TrimIncomingPaths
        {
            get => 
                this._trimIncomingPaths;
            set => 
                (this._trimIncomingPaths = value);
        }

        public char Replacement
        {
            get => 
                this._replacementChar;
            set
            {
                for (int i = 0; i < InvalidEntryChars.Length; i++)
                {
                    if (InvalidEntryChars[i] == value)
                    {
                        throw new ArgumentException("invalid path character");
                    }
                }
                if ((value == Path.DirectorySeparatorChar) || (value == Path.AltDirectorySeparatorChar))
                {
                    throw new ArgumentException("invalid replacement character");
                }
                this._replacementChar = value;
            }
        }
    }
}

