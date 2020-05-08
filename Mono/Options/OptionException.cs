namespace Mono.Options
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class OptionException : Exception
    {
        private string option;

        public OptionException()
        {
        }

        protected OptionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.option = info.GetString("OptionName");
        }

        public OptionException(string message, string optionName) : base(message)
        {
            this.option = optionName;
        }

        public OptionException(string message, string optionName, Exception innerException) : base(message, innerException)
        {
            this.option = optionName;
        }

        [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("OptionName", this.option);
        }

        public string OptionName =>
            this.option;
    }
}

