namespace Mono.Options
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public class ResponseFileSource : ArgumentSource
    {
        public override bool GetArguments(string value, out IEnumerable<string> replacement)
        {
            if (!string.IsNullOrEmpty(value) && value.StartsWith("@"))
            {
                replacement = GetArgumentsFromFile(value.Substring(1));
                return true;
            }
            replacement = null;
            return false;
        }

        public override string[] GetNames() => 
            new string[] { "@file" };

        public override string Description =>
            "Read response file for more options.";
    }
}

