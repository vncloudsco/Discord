namespace Microsoft.Web.XmlTransform
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal class SetTokenizedAttributeStorage
    {
        public SetTokenizedAttributeStorage() : this(4)
        {
        }

        public SetTokenizedAttributeStorage(int capacity)
        {
            this.DictionaryList = new List<Dictionary<string, string>>(capacity);
            string[] strArray = new string[] { "$(ReplacableToken_#(", SetTokenizedAttributes.ParameterAttribute, ")_#(", SetTokenizedAttributes.TokenNumber, "))" };
            this.TokenFormat = string.Concat(strArray);
            this.EnableTokenizeParameters = false;
            this.UseXpathToFormParameter = true;
        }

        public List<Dictionary<string, string>> DictionaryList { get; set; }

        public string TokenFormat { get; set; }

        public bool EnableTokenizeParameters { get; set; }

        public bool UseXpathToFormParameter { get; set; }
    }
}

