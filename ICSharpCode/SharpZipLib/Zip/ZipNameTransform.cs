namespace ICSharpCode.SharpZipLib.Zip
{
    using ICSharpCode.SharpZipLib.Core;
    using System;
    using System.IO;
    using System.Text;

    internal class ZipNameTransform : INameTransform
    {
        private string trimPrefix_;
        private static readonly char[] InvalidEntryChars;
        private static readonly char[] InvalidEntryCharsRelaxed;

        static ZipNameTransform()
        {
            char[] invalidPathChars = Path.GetInvalidPathChars();
            int num = invalidPathChars.Length + 2;
            InvalidEntryCharsRelaxed = new char[num];
            Array.Copy(invalidPathChars, 0, InvalidEntryCharsRelaxed, 0, invalidPathChars.Length);
            InvalidEntryCharsRelaxed[num - 1] = '*';
            InvalidEntryCharsRelaxed[num - 2] = '?';
            num = invalidPathChars.Length + 4;
            InvalidEntryChars = new char[num];
            Array.Copy(invalidPathChars, 0, InvalidEntryChars, 0, invalidPathChars.Length);
            InvalidEntryChars[num - 1] = ':';
            InvalidEntryChars[num - 2] = '\\';
            InvalidEntryChars[num - 3] = '*';
            InvalidEntryChars[num - 4] = '?';
        }

        public ZipNameTransform()
        {
        }

        public ZipNameTransform(string trimPrefix)
        {
            this.TrimPrefix = trimPrefix;
        }

        public static bool IsValidName(string name) => 
            ((name != null) && ((name.IndexOfAny(InvalidEntryChars) < 0) && (name.IndexOf('/') != 0)));

        public static bool IsValidName(string name, bool relaxed)
        {
            bool flag = name != null;
            if (flag)
            {
                flag = !relaxed ? ((name.IndexOfAny(InvalidEntryChars) < 0) && (name.IndexOf('/') != 0)) : (name.IndexOfAny(InvalidEntryCharsRelaxed) < 0);
            }
            return flag;
        }

        private static string MakeValidName(string name, char replacement)
        {
            int num = name.IndexOfAny(InvalidEntryChars);
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
            if (name.Length > 0xffff)
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
            if (!name.EndsWith("/"))
            {
                name = name + "/";
            }
            return name;
        }

        public string TransformFile(string name)
        {
            if (name == null)
            {
                name = string.Empty;
            }
            else
            {
                string str = name.ToLower();
                if ((this.trimPrefix_ != null) && (str.IndexOf(this.trimPrefix_) == 0))
                {
                    name = name.Substring(this.trimPrefix_.Length);
                }
                name = name.Replace(@"\", "/");
                name = WindowsPathUtils.DropPathRoot(name);
                while (true)
                {
                    if ((name.Length <= 0) || (name[0] != '/'))
                    {
                        while (true)
                        {
                            if ((name.Length <= 0) || (name[name.Length - 1] != '/'))
                            {
                                int index = name.IndexOf("//");
                                while (true)
                                {
                                    if (index < 0)
                                    {
                                        name = MakeValidName(name, '_');
                                        break;
                                    }
                                    name = name.Remove(index, 1);
                                    index = name.IndexOf("//");
                                }
                                break;
                            }
                            name = name.Remove(name.Length - 1, 1);
                        }
                        break;
                    }
                    name = name.Remove(0, 1);
                }
            }
            return name;
        }

        public string TrimPrefix
        {
            get => 
                this.trimPrefix_;
            set
            {
                this.trimPrefix_ = value;
                if (this.trimPrefix_ != null)
                {
                    this.trimPrefix_ = this.trimPrefix_.ToLower();
                }
            }
        }
    }
}

