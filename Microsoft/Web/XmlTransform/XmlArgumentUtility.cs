namespace Microsoft.Web.XmlTransform
{
    using System;
    using System.Collections.Generic;

    internal static class XmlArgumentUtility
    {
        private static int CountParens(string str)
        {
            int num = 0;
            foreach (char ch in str)
            {
                char ch2 = ch;
                switch (ch2)
                {
                    case '(':
                        num++;
                        break;

                    case ')':
                        num--;
                        break;

                    default:
                        break;
                }
            }
            return num;
        }

        private static IList<string> RecombineArguments(IList<string> arguments, char separator)
        {
            List<string> list = new List<string>();
            string item = null;
            int num = 0;
            foreach (string str2 in arguments)
            {
                item = (item != null) ? (item + separator + str2) : str2;
                if ((num + CountParens(str2)) == 0)
                {
                    list.Add(item);
                    item = null;
                }
            }
            if (item != null)
            {
                list.Add(item);
            }
            if (arguments.Count != list.Count)
            {
                arguments = list;
            }
            return arguments;
        }

        internal static IList<string> SplitArguments(string argumentString)
        {
            if (argumentString.IndexOf(',') == -1)
            {
                return new string[] { argumentString };
            }
            List<string> arguments = new List<string>();
            arguments.AddRange(argumentString.Split(new char[] { ',' }));
            IList<string> list2 = RecombineArguments(arguments, ',');
            TrimStrings(list2);
            return list2;
        }

        private static void TrimStrings(IList<string> arguments)
        {
            for (int i = 0; i < arguments.Count; i++)
            {
                arguments[i] = arguments[i].Trim();
            }
        }
    }
}

