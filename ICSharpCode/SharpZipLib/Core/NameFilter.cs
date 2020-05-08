namespace ICSharpCode.SharpZipLib.Core
{
    using System;
    using System.Collections;
    using System.Text;
    using System.Text.RegularExpressions;

    internal class NameFilter : IScanFilter
    {
        private string filter_;
        private ArrayList inclusions_;
        private ArrayList exclusions_;

        public NameFilter(string filter)
        {
            this.filter_ = filter;
            this.inclusions_ = new ArrayList();
            this.exclusions_ = new ArrayList();
            this.Compile();
        }

        private void Compile()
        {
            if (this.filter_ != null)
            {
                string[] strArray = SplitQuoted(this.filter_);
                for (int i = 0; i < strArray.Length; i++)
                {
                    if ((strArray[i] != null) && (strArray[i].Length > 0))
                    {
                        string pattern = (strArray[i][0] != '+') ? ((strArray[i][0] != '-') ? strArray[i] : strArray[i].Substring(1, strArray[i].Length - 1)) : strArray[i].Substring(1, strArray[i].Length - 1);
                        if (strArray[i][0] != '-')
                        {
                            this.inclusions_.Add(new Regex(pattern, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase));
                        }
                        else
                        {
                            this.exclusions_.Add(new Regex(pattern, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase));
                        }
                    }
                }
            }
        }

        public bool IsExcluded(string name)
        {
            bool flag = false;
            using (IEnumerator enumerator = this.exclusions_.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (((Regex) enumerator.Current).IsMatch(name))
                    {
                        flag = true;
                        break;
                    }
                }
            }
            return flag;
        }

        public bool IsIncluded(string name)
        {
            bool flag = false;
            if (this.inclusions_.Count == 0)
            {
                flag = true;
            }
            else
            {
                using (IEnumerator enumerator = this.inclusions_.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        if (((Regex) enumerator.Current).IsMatch(name))
                        {
                            flag = true;
                            break;
                        }
                    }
                }
            }
            return flag;
        }

        public bool IsMatch(string name) => 
            (this.IsIncluded(name) && !this.IsExcluded(name));

        public static bool IsValidExpression(string expression)
        {
            bool flag = true;
            try
            {
                Regex regex1 = new Regex(expression, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            }
            catch (ArgumentException)
            {
                flag = false;
            }
            return flag;
        }

        public static bool IsValidFilterExpression(string toTest)
        {
            bool flag = true;
            try
            {
                if (toTest != null)
                {
                    string[] strArray = SplitQuoted(toTest);
                    for (int i = 0; i < strArray.Length; i++)
                    {
                        if ((strArray[i] != null) && (strArray[i].Length > 0))
                        {
                            string pattern = (strArray[i][0] != '+') ? ((strArray[i][0] != '-') ? strArray[i] : strArray[i].Substring(1, strArray[i].Length - 1)) : strArray[i].Substring(1, strArray[i].Length - 1);
                            Regex regex1 = new Regex(pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
                        }
                    }
                }
            }
            catch (ArgumentException)
            {
                flag = false;
            }
            return flag;
        }

        public static string[] SplitQuoted(string original)
        {
            char ch = '\\';
            char[] array = new char[] { ';' };
            ArrayList list = new ArrayList();
            if ((original != null) && (original.Length > 0))
            {
                int num = -1;
                StringBuilder builder = new StringBuilder();
                while (num < original.Length)
                {
                    num++;
                    if (num >= original.Length)
                    {
                        list.Add(builder.ToString());
                        continue;
                    }
                    if (original[num] != ch)
                    {
                        if (Array.IndexOf<char>(array, original[num]) < 0)
                        {
                            builder.Append(original[num]);
                            continue;
                        }
                        list.Add(builder.ToString());
                        builder.Length = 0;
                        continue;
                    }
                    num++;
                    if (num >= original.Length)
                    {
                        throw new ArgumentException("Missing terminating escape character", "original");
                    }
                    if (Array.IndexOf<char>(array, original[num]) < 0)
                    {
                        builder.Append(ch);
                    }
                    builder.Append(original[num]);
                }
            }
            return (string[]) list.ToArray(typeof(string));
        }

        public override string ToString() => 
            this.filter_;
    }
}

