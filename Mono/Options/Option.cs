namespace Mono.Options
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    public abstract class Option
    {
        private string prototype;
        private string description;
        private string[] names;
        private Mono.Options.OptionValueType type;
        private int count;
        private string[] separators;
        private bool hidden;
        private static readonly char[] NameTerminator = new char[] { '=', ':' };

        protected Option(string prototype, string description) : this(prototype, description, 1, false)
        {
        }

        protected Option(string prototype, string description, int maxValueCount) : this(prototype, description, maxValueCount, false)
        {
        }

        protected Option(string prototype, string description, int maxValueCount, bool hidden)
        {
            string[] textArray2;
            if (prototype == null)
            {
                throw new ArgumentNullException("prototype");
            }
            if (prototype.Length == 0)
            {
                throw new ArgumentException("Cannot be the empty string.", "prototype");
            }
            if (maxValueCount < 0)
            {
                throw new ArgumentOutOfRangeException("maxValueCount");
            }
            this.prototype = prototype;
            this.description = description;
            this.count = maxValueCount;
            if (this is OptionSet.Category)
            {
                textArray2 = new string[] { prototype + this.GetHashCode() };
            }
            else
            {
                char[] separator = new char[] { '|' };
                textArray2 = prototype.Split(separator);
            }
            this.names = textArray2;
            if (!(this is OptionSet.Category))
            {
                this.type = this.ParsePrototype();
                this.hidden = hidden;
                if ((this.count == 0) && (this.type != Mono.Options.OptionValueType.None))
                {
                    throw new ArgumentException("Cannot provide maxValueCount of 0 for OptionValueType.Required or OptionValueType.Optional.", "maxValueCount");
                }
                if ((this.type == Mono.Options.OptionValueType.None) && (maxValueCount > 1))
                {
                    throw new ArgumentException($"Cannot provide maxValueCount of {maxValueCount} for OptionValueType.None.", "maxValueCount");
                }
                if ((Array.IndexOf<string>(this.names, "<>") >= 0) && (((this.names.Length == 1) && (this.type != Mono.Options.OptionValueType.None)) || ((this.names.Length > 1) && (this.MaxValueCount > 1))))
                {
                    throw new ArgumentException("The default option handler '<>' cannot require values.", "prototype");
                }
            }
        }

        private static void AddSeparators(string name, int end, ICollection<string> seps)
        {
            int startIndex = -1;
            for (int i = end + 1; i < name.Length; i++)
            {
                char ch = name[i];
                if (ch == '{')
                {
                    if (startIndex != -1)
                    {
                        throw new ArgumentException($"Ill-formed name/value separator found in "{name}".", "prototype");
                    }
                    startIndex = i + 1;
                }
                else if (ch != '}')
                {
                    if (startIndex == -1)
                    {
                        seps.Add(name[i].ToString());
                    }
                }
                else
                {
                    if (startIndex == -1)
                    {
                        throw new ArgumentException($"Ill-formed name/value separator found in "{name}".", "prototype");
                    }
                    seps.Add(name.Substring(startIndex, i - startIndex));
                    startIndex = -1;
                }
            }
            if (startIndex != -1)
            {
                throw new ArgumentException($"Ill-formed name/value separator found in "{name}".", "prototype");
            }
        }

        public string[] GetNames() => 
            ((string[]) this.names.Clone());

        public string[] GetValueSeparators() => 
            ((this.separators != null) ? ((string[]) this.separators.Clone()) : new string[0]);

        public void Invoke(OptionContext c)
        {
            this.OnParseComplete(c);
            c.OptionName = null;
            c.Option = null;
            c.OptionValues.Clear();
        }

        protected abstract void OnParseComplete(OptionContext c);
        protected static T Parse<T>(string value, OptionContext c)
        {
            Type type = typeof(T);
            TypeConverter converter = TypeDescriptor.GetConverter(((type.IsValueType && (type.IsGenericType && !type.IsGenericTypeDefinition)) && (type.GetGenericTypeDefinition() == typeof(Nullable<>))) ? type.GetGenericArguments()[0] : typeof(T));
            T local = default(T);
            try
            {
                if (value != null)
                {
                    local = (T) converter.ConvertFromString(value);
                }
            }
            catch (Exception exception)
            {
                Type type2;
                throw new OptionException(string.Format(c.OptionSet.MessageLocalizer("Could not convert string `{0}' to type {1} for option `{2}'."), value, type2.Name, c.OptionName), c.OptionName, exception);
            }
            return local;
        }

        private Mono.Options.OptionValueType ParsePrototype()
        {
            char ch = '\0';
            List<string> seps = new List<string>();
            for (int i = 0; i < this.names.Length; i++)
            {
                string name = this.names[i];
                if (name.Length == 0)
                {
                    throw new ArgumentException("Empty option names are not supported.", "prototype");
                }
                int length = name.IndexOfAny(NameTerminator);
                if (length != -1)
                {
                    this.names[i] = name.Substring(0, length);
                    if ((ch != '\0') && (ch != name[length]))
                    {
                        throw new ArgumentException($"Conflicting option types: '{ch}' vs. '{name[length]}'.", "prototype");
                    }
                    ch = name[length];
                    AddSeparators(name, length, seps);
                }
            }
            if (ch == '\0')
            {
                return Mono.Options.OptionValueType.None;
            }
            if ((this.count <= 1) && (seps.Count != 0))
            {
                throw new ArgumentException($"Cannot provide key/value separators for Options taking {this.count} value(s).", "prototype");
            }
            if (this.count > 1)
            {
                if (seps.Count != 0)
                {
                    this.separators = ((seps.Count != 1) || (seps[0].Length != 0)) ? seps.ToArray() : null;
                }
                else
                {
                    this.separators = new string[] { ":", "=" };
                }
            }
            return ((ch == '=') ? Mono.Options.OptionValueType.Required : Mono.Options.OptionValueType.Optional);
        }

        public override string ToString() => 
            this.Prototype;

        public string Prototype =>
            this.prototype;

        public string Description =>
            this.description;

        public Mono.Options.OptionValueType OptionValueType =>
            this.type;

        public int MaxValueCount =>
            this.count;

        public bool Hidden =>
            this.hidden;

        internal string[] Names =>
            this.names;

        internal string[] ValueSeparators =>
            this.separators;
    }
}

