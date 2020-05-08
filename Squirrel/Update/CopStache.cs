namespace Squirrel.Update
{
    using System;
    using System.Collections.Generic;
    using System.Security;
    using System.Text;

    public static class CopStache
    {
        public static string Render(string template, Dictionary<string, string> identifiers)
        {
            StringBuilder builder = new StringBuilder();
            char[] separator = new char[] { '\n' };
            foreach (string str in template.Split(separator))
            {
                identifiers["RandomGuid"] = Guid.NewGuid().ToString();
                foreach (string str2 in identifiers.Keys)
                {
                    builder.Replace("{{" + str2 + "}}", SecurityElement.Escape(identifiers[str2]));
                }
                builder.AppendLine(str);
            }
            return builder.ToString();
        }
    }
}

