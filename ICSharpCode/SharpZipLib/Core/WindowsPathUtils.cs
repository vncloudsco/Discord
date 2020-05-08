namespace ICSharpCode.SharpZipLib.Core
{
    using System;

    internal abstract class WindowsPathUtils
    {
        internal WindowsPathUtils()
        {
        }

        public static string DropPathRoot(string path)
        {
            string str = path;
            if ((path != null) && (path.Length > 0))
            {
                if ((path[0] != '\\') && (path[0] != '/'))
                {
                    if ((path.Length > 1) && (path[1] == ':'))
                    {
                        int count = 2;
                        if ((path.Length > 2) && ((path[2] == '\\') || (path[2] == '/')))
                        {
                            count = 3;
                        }
                        str = str.Remove(0, count);
                    }
                }
                else if ((path.Length > 1) && ((path[1] == '\\') || (path[1] == '/')))
                {
                    int startIndex = 2;
                    int num2 = 2;
                    while (true)
                    {
                        if ((startIndex > path.Length) || (((path[startIndex] == '\\') || (path[startIndex] == '/')) && (--num2 <= 0)))
                        {
                            startIndex++;
                            str = (startIndex >= path.Length) ? "" : path.Substring(startIndex);
                            break;
                        }
                        startIndex++;
                    }
                }
            }
            return str;
        }
    }
}

