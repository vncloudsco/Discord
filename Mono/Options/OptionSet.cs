namespace Mono.Options
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;

    public class OptionSet : KeyedCollection<string, Option>
    {
        private Converter<string, string> localizer;
        private List<ArgumentSource> sources;
        private ReadOnlyCollection<ArgumentSource> roSources;
        private readonly Regex ValueOption;
        private const int OptionWidth = 0x1d;
        private const int Description_FirstWidth = 0x33;
        private const int Description_RemWidth = 0x31;

        public OptionSet() : this(f => f)
        {
        }

        public OptionSet(Converter<string, string> localizer)
        {
            this.sources = new List<ArgumentSource>();
            this.ValueOption = new Regex("^(?<flag>--|-|/)(?<name>[^:=]+)((?<sep>[:=])(?<value>.*))?$");
            this.localizer = localizer;
            this.roSources = new ReadOnlyCollection<ArgumentSource>(this.sources);
        }

        public OptionSet Add(ArgumentSource source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            this.sources.Add(source);
            return this;
        }

        public OptionSet Add(Option option)
        {
            base.Add(option);
            return this;
        }

        public OptionSet Add(string header)
        {
            if (header == null)
            {
                throw new ArgumentNullException("header");
            }
            this.Add(new Category(header));
            return this;
        }

        public OptionSet Add(string prototype, OptionAction<string, string> action) => 
            this.Add(prototype, null, action);

        public OptionSet Add<TKey, TValue>(string prototype, OptionAction<TKey, TValue> action) => 
            this.Add<TKey, TValue>(prototype, null, action);

        public OptionSet Add(string prototype, Action<string> action) => 
            this.Add(prototype, null, action);

        public OptionSet Add<T>(string prototype, Action<T> action) => 
            this.Add<T>(prototype, null, action);

        public OptionSet Add(string prototype, string description, OptionAction<string, string> action) => 
            this.Add(prototype, description, action, false);

        public OptionSet Add<TKey, TValue>(string prototype, string description, OptionAction<TKey, TValue> action) => 
            this.Add(new ActionOption<TKey, TValue>(prototype, description, action));

        public OptionSet Add(string prototype, string description, Action<string> action) => 
            this.Add(prototype, description, action, false);

        public OptionSet Add<T>(string prototype, string description, Action<T> action) => 
            this.Add(new ActionOption<T>(prototype, description, action));

        public OptionSet Add(string prototype, string description, OptionAction<string, string> action, bool hidden)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            Option item = new ActionOption(prototype, description, 2, delegate (OptionValueCollection v) {
                action(v[0], v[1]);
            }, hidden);
            base.Add(item);
            return this;
        }

        public OptionSet Add(string prototype, string description, Action<string> action, bool hidden)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            Option item = new ActionOption(prototype, description, 1, delegate (OptionValueCollection v) {
                action(v[0]);
            }, hidden);
            base.Add(item);
            return this;
        }

        private void AddImpl(Option option)
        {
            if (option == null)
            {
                throw new ArgumentNullException("option");
            }
            List<string> list = new List<string>(option.Names.Length);
            try
            {
                for (int i = 1; i < option.Names.Length; i++)
                {
                    base.Dictionary.Add(option.Names[i], option);
                    list.Add(option.Names[i]);
                }
            }
            catch (Exception)
            {
                foreach (string str in list)
                {
                    base.Dictionary.Remove(str);
                }
                throw;
            }
        }

        private bool AddSource(ArgumentEnumerator ae, string argument)
        {
            bool flag;
            using (List<ArgumentSource>.Enumerator enumerator = this.sources.GetEnumerator())
            {
                while (true)
                {
                    if (enumerator.MoveNext())
                    {
                        IEnumerable<string> enumerable;
                        if (!enumerator.Current.GetArguments(argument, out enumerable))
                        {
                            continue;
                        }
                        ae.Add(enumerable);
                        flag = true;
                    }
                    else
                    {
                        return false;
                    }
                    break;
                }
            }
            return flag;
        }

        protected virtual OptionContext CreateOptionContext() => 
            new OptionContext(this);

        private static string GetArgumentName(int index, int maxIndex, string description)
        {
            if (description != null)
            {
                string[] strArray;
                if (maxIndex != 1)
                {
                    strArray = new string[] { "{" + index + ":" };
                }
                else
                {
                    strArray = new string[] { "{0:", "{" };
                }
                int num = 0;
                while (num < strArray.Length)
                {
                    int startIndex = 0;
                    while (true)
                    {
                        int num2 = description.IndexOf(strArray[num], startIndex);
                        if (((num2 < 0) || (startIndex == 0)) || (description[startIndex++ - 1] != '{'))
                        {
                            if (num2 != -1)
                            {
                                int num4 = description.IndexOf("}", num2);
                                if (num4 != -1)
                                {
                                    return description.Substring(num2 + strArray[num].Length, (num4 - num2) - strArray[num].Length);
                                }
                            }
                            num++;
                            break;
                        }
                    }
                }
            }
            return ((maxIndex == 1) ? "VALUE" : ("VALUE" + (index + 1)));
        }

        private static string GetDescription(string description)
        {
            if (description == null)
            {
                return string.Empty;
            }
            StringBuilder builder = new StringBuilder(description.Length);
            int startIndex = -1;
            int num2 = 0;
            while (true)
            {
                while (true)
                {
                    if (num2 >= description.Length)
                    {
                        return builder.ToString();
                    }
                    char ch = description[num2];
                    if (ch == ':')
                    {
                        if (startIndex >= 0)
                        {
                            startIndex = num2 + 1;
                            break;
                        }
                    }
                    else
                    {
                        if (ch == '{')
                        {
                            if (num2 == startIndex)
                            {
                                builder.Append('{');
                                startIndex = -1;
                            }
                            else if (startIndex < 0)
                            {
                                startIndex = num2 + 1;
                            }
                            break;
                        }
                        if (ch == '}')
                        {
                            if (startIndex >= 0)
                            {
                                builder.Append(description.Substring(startIndex, num2 - startIndex));
                                startIndex = -1;
                            }
                            else
                            {
                                if (((num2 + 1) == description.Length) || (description[num2 + 1] != '}'))
                                {
                                    throw new InvalidOperationException("Invalid option description: " + description);
                                }
                                num2++;
                                builder.Append("}");
                            }
                            break;
                        }
                    }
                    if (startIndex < 0)
                    {
                        builder.Append(description[num2]);
                    }
                    break;
                }
                num2++;
            }
        }

        protected override string GetKeyForItem(Option item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("option");
            }
            if ((item.Names == null) || (item.Names.Length == 0))
            {
                throw new InvalidOperationException("Option has no names!");
            }
            return item.Names[0];
        }

        private static IEnumerable<string> GetLines(string description, int firstWidth, int remWidth)
        {
            int[] widths = new int[] { firstWidth, remWidth };
            return StringCoda.WrappedLines(description, widths);
        }

        private static int GetNextOptionIndex(string[] names, int i)
        {
            while ((i < names.Length) && (names[i] == "<>"))
            {
                i++;
            }
            return i;
        }

        [Obsolete("Use KeyedCollection.this[string]")]
        protected Option GetOptionForName(string option)
        {
            if (option == null)
            {
                throw new ArgumentNullException("option");
            }
            try
            {
                return base[option];
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        protected bool GetOptionParts(string argument, out string flag, out string name, out string sep, out string value)
        {
            string str;
            if (argument == null)
            {
                throw new ArgumentNullException("argument");
            }
            value = (string) (str = null);
            string text1 = str;
            string text2 = sep = text1;
            flag = name = text2;
            Match match = this.ValueOption.Match(argument);
            if (!match.Success)
            {
                return false;
            }
            flag = match.Groups["flag"].Value;
            name = match.Groups["name"].Value;
            if (match.Groups["sep"].Success && match.Groups["value"].Success)
            {
                sep = match.Groups["sep"].Value;
                value = match.Groups["value"].Value;
            }
            return true;
        }

        protected override void InsertItem(int index, Option item)
        {
            base.InsertItem(index, item);
            this.AddImpl(item);
        }

        private static void Invoke(OptionContext c, string name, string value, Option option)
        {
            c.OptionName = name;
            c.Option = option;
            c.OptionValues.Add(value);
            option.Invoke(c);
        }

        public List<string> Parse(IEnumerable<string> arguments)
        {
            if (arguments == null)
            {
                throw new ArgumentNullException("arguments");
            }
            OptionContext c = this.CreateOptionContext();
            c.OptionIndex = -1;
            bool flag = true;
            List<string> extra = new List<string>();
            Option def = base.Contains("<>") ? base["<>"] : null;
            ArgumentEnumerator ae = new ArgumentEnumerator(arguments);
            foreach (string str in ae)
            {
                int num = c.OptionIndex + 1;
                c.OptionIndex = num;
                if (str == "--")
                {
                    flag = false;
                    continue;
                }
                if (!flag)
                {
                    Unprocessed(extra, def, c, str);
                    continue;
                }
                if (!this.AddSource(ae, str) && !this.Parse(str, c))
                {
                    Unprocessed(extra, def, c, str);
                }
            }
            if (c.Option != null)
            {
                c.Option.Invoke(c);
            }
            return extra;
        }

        protected virtual bool Parse(string argument, OptionContext c)
        {
            string str;
            string str2;
            string str3;
            string str4;
            if (c.Option != null)
            {
                this.ParseValue(argument, c);
                return true;
            }
            if (!this.GetOptionParts(argument, out str, out str2, out str3, out str4))
            {
                return false;
            }
            if (!base.Contains(str2))
            {
                if (this.ParseBool(argument, str2, c))
                {
                    return true;
                }
                string[] textArray1 = new string[] { str2 + str3 + str4 };
                return this.ParseBundledValue(str, string.Concat(textArray1), c);
            }
            Option option = base[str2];
            c.OptionName = str + str2;
            c.Option = option;
            switch (option.OptionValueType)
            {
                case OptionValueType.None:
                    c.OptionValues.Add(str2);
                    c.Option.Invoke(c);
                    break;

                case OptionValueType.Optional:
                case OptionValueType.Required:
                    this.ParseValue(str4, c);
                    break;

                default:
                    break;
            }
            return true;
        }

        private bool ParseBool(string option, string n, OptionContext c)
        {
            string str;
            if (((n.Length < 1) || ((n[n.Length - 1] != '+') && (n[n.Length - 1] != '-'))) || !base.Contains(str = n.Substring(0, n.Length - 1)))
            {
                return false;
            }
            Option option2 = base[str];
            string item = (n[n.Length - 1] == '+') ? option : null;
            c.OptionName = option;
            c.Option = option2;
            c.OptionValues.Add(item);
            option2.Invoke(c);
            return true;
        }

        private bool ParseBundledValue(string f, string n, OptionContext c)
        {
            if (f != "-")
            {
                return false;
            }
            int num = 0;
            while (num < n.Length)
            {
                string optionName = f + n[num].ToString();
                string key = n[num].ToString();
                if (!base.Contains(key))
                {
                    if (num != 0)
                    {
                        throw new OptionException(string.Format(this.localizer("Cannot bundle unregistered option '{0}'."), optionName), optionName);
                    }
                    return false;
                }
                Option option = base[key];
                switch (option.OptionValueType)
                {
                    case OptionValueType.None:
                    {
                        Invoke(c, optionName, n, option);
                        num++;
                        continue;
                    }
                    case OptionValueType.Optional:
                    case OptionValueType.Required:
                    {
                        string str3 = n.Substring(num + 1);
                        c.Option = option;
                        c.OptionName = optionName;
                        this.ParseValue((str3.Length != 0) ? str3 : null, c);
                        return true;
                    }
                }
                throw new InvalidOperationException("Unknown OptionValueType: " + option.OptionValueType);
            }
            return true;
        }

        private void ParseValue(string option, OptionContext c)
        {
            if (option != null)
            {
                string[] textArray2;
                if (c.Option.ValueSeparators != null)
                {
                    textArray2 = option.Split(c.Option.ValueSeparators, c.Option.MaxValueCount - c.OptionValues.Count, StringSplitOptions.None);
                }
                else
                {
                    textArray2 = new string[] { option };
                }
                foreach (string str in textArray2)
                {
                    c.OptionValues.Add(str);
                }
            }
            if ((c.OptionValues.Count == c.Option.MaxValueCount) || (c.Option.OptionValueType == OptionValueType.Optional))
            {
                c.Option.Invoke(c);
            }
            else if (c.OptionValues.Count > c.Option.MaxValueCount)
            {
                throw new OptionException(this.localizer($"Error: Found {c.OptionValues.Count} option values when expecting {c.Option.MaxValueCount}."), c.OptionName);
            }
        }

        protected override void RemoveItem(int index)
        {
            Option option = base.Items[index];
            base.RemoveItem(index);
            for (int i = 1; i < option.Names.Length; i++)
            {
                base.Dictionary.Remove(option.Names[i]);
            }
        }

        protected override void SetItem(int index, Option item)
        {
            base.SetItem(index, item);
            this.AddImpl(item);
        }

        private static bool Unprocessed(ICollection<string> extra, Option def, OptionContext c, string argument)
        {
            if (def == null)
            {
                extra.Add(argument);
                return false;
            }
            c.OptionValues.Add(argument);
            c.Option = def;
            c.Option.Invoke(c);
            return false;
        }

        private static void Write(TextWriter o, ref int n, string s)
        {
            n += s.Length;
            o.Write(s);
        }

        private void WriteDescription(TextWriter o, string value, string prefix, int firstWidth, int remWidth)
        {
            bool flag = false;
            foreach (string str in GetLines(this.localizer(GetDescription(value)), firstWidth, remWidth))
            {
                if (flag)
                {
                    o.Write(prefix);
                }
                o.WriteLine(str);
                flag = true;
            }
        }

        public void WriteOptionDescriptions(TextWriter o)
        {
            foreach (Option option in this)
            {
                int written = 0;
                if (!option.Hidden)
                {
                    if (option is Category)
                    {
                        this.WriteDescription(o, option.Description, "", 80, 80);
                        continue;
                    }
                    if (this.WriteOptionPrototype(o, option, ref written))
                    {
                        if (written < 0x1d)
                        {
                            o.Write(new string(' ', 0x1d - written));
                        }
                        else
                        {
                            o.WriteLine();
                            o.Write(new string(' ', 0x1d));
                        }
                        this.WriteDescription(o, option.Description, new string(' ', 0x1f), 0x33, 0x31);
                    }
                }
            }
            foreach (ArgumentSource source in this.sources)
            {
                string[] names = source.GetNames();
                if ((names != null) && (names.Length != 0))
                {
                    int n = 0;
                    Write(o, ref n, "  ");
                    Write(o, ref n, names[0]);
                    int index = 1;
                    while (true)
                    {
                        if (index >= names.Length)
                        {
                            if (n < 0x1d)
                            {
                                o.Write(new string(' ', 0x1d - n));
                            }
                            else
                            {
                                o.WriteLine();
                                o.Write(new string(' ', 0x1d));
                            }
                            this.WriteDescription(o, source.Description, new string(' ', 0x1f), 0x33, 0x31);
                            break;
                        }
                        Write(o, ref n, ", ");
                        Write(o, ref n, names[index]);
                        index++;
                    }
                }
            }
        }

        private bool WriteOptionPrototype(TextWriter o, Option p, ref int written)
        {
            string[] names = p.Names;
            int nextOptionIndex = GetNextOptionIndex(names, 0);
            if (nextOptionIndex == names.Length)
            {
                return false;
            }
            if (names[nextOptionIndex].Length == 1)
            {
                Write(o, ref written, "  -");
                Write(o, ref written, names[0]);
            }
            else
            {
                Write(o, ref written, "      --");
                Write(o, ref written, names[0]);
            }
            for (nextOptionIndex = GetNextOptionIndex(names, nextOptionIndex + 1); nextOptionIndex < names.Length; nextOptionIndex = GetNextOptionIndex(names, nextOptionIndex + 1))
            {
                Write(o, ref written, ", ");
                Write(o, ref written, (names[nextOptionIndex].Length == 1) ? "-" : "--");
                Write(o, ref written, names[nextOptionIndex]);
            }
            if ((p.OptionValueType == OptionValueType.Optional) || (p.OptionValueType == OptionValueType.Required))
            {
                if (p.OptionValueType == OptionValueType.Optional)
                {
                    Write(o, ref written, this.localizer("["));
                }
                Write(o, ref written, this.localizer("=" + GetArgumentName(0, p.MaxValueCount, p.Description)));
                string str = ((p.ValueSeparators == null) || (p.ValueSeparators.Length == 0)) ? " " : p.ValueSeparators[0];
                int index = 1;
                while (true)
                {
                    if (index >= p.MaxValueCount)
                    {
                        if (p.OptionValueType == OptionValueType.Optional)
                        {
                            Write(o, ref written, this.localizer("]"));
                        }
                        break;
                    }
                    Write(o, ref written, this.localizer(str + GetArgumentName(index, p.MaxValueCount, p.Description)));
                    index++;
                }
            }
            return true;
        }

        public Converter<string, string> MessageLocalizer =>
            this.localizer;

        public ReadOnlyCollection<ArgumentSource> ArgumentSources =>
            this.roSources;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly OptionSet.<>c <>9 = new OptionSet.<>c();
            public static Converter<string, string> <>9__0_0;

            internal string <.ctor>b__0_0(string f) => 
                f;
        }

        private sealed class ActionOption : Option
        {
            private Action<OptionValueCollection> action;

            public ActionOption(string prototype, string description, int count, Action<OptionValueCollection> action) : this(prototype, description, count, action, false)
            {
            }

            public ActionOption(string prototype, string description, int count, Action<OptionValueCollection> action, bool hidden) : base(prototype, description, count, hidden)
            {
                if (action == null)
                {
                    throw new ArgumentNullException("action");
                }
                this.action = action;
            }

            protected override void OnParseComplete(OptionContext c)
            {
                this.action(c.OptionValues);
            }
        }

        private sealed class ActionOption<T> : Option
        {
            private Action<T> action;

            public ActionOption(string prototype, string description, Action<T> action) : base(prototype, description, 1)
            {
                if (action == null)
                {
                    throw new ArgumentNullException("action");
                }
                this.action = action;
            }

            protected override void OnParseComplete(OptionContext c)
            {
                this.action(Parse<T>(c.OptionValues[0], c));
            }
        }

        private sealed class ActionOption<TKey, TValue> : Option
        {
            private OptionAction<TKey, TValue> action;

            public ActionOption(string prototype, string description, OptionAction<TKey, TValue> action) : base(prototype, description, 2)
            {
                if (action == null)
                {
                    throw new ArgumentNullException("action");
                }
                this.action = action;
            }

            protected override void OnParseComplete(OptionContext c)
            {
                this.action(Parse<TKey>(c.OptionValues[0], c), Parse<TValue>(c.OptionValues[1], c));
            }
        }

        private class ArgumentEnumerator : IEnumerable<string>, IEnumerable
        {
            private List<IEnumerator<string>> sources = new List<IEnumerator<string>>();

            public ArgumentEnumerator(IEnumerable<string> arguments)
            {
                this.sources.Add(arguments.GetEnumerator());
            }

            public void Add(IEnumerable<string> arguments)
            {
                this.sources.Add(arguments.GetEnumerator());
            }

            [IteratorStateMachine(typeof(<GetEnumerator>d__3))]
            public IEnumerator<string> GetEnumerator()
            {
                <GetEnumerator>d__3 d__1 = new <GetEnumerator>d__3(0);
                d__1.<>4__this = this;
                return d__1;
            }

            IEnumerator IEnumerable.GetEnumerator() => 
                this.GetEnumerator();

            [CompilerGenerated]
            private sealed class <GetEnumerator>d__3 : IEnumerator<string>, IDisposable, IEnumerator
            {
                private int <>1__state;
                private string <>2__current;
                public OptionSet.ArgumentEnumerator <>4__this;
                private IEnumerator<string> <c>5__1;

                [DebuggerHidden]
                public <GetEnumerator>d__3(int <>1__state)
                {
                    this.<>1__state = <>1__state;
                }

                private bool MoveNext()
                {
                    int num = this.<>1__state;
                    if (num == 0)
                    {
                        this.<>1__state = -1;
                    }
                    else
                    {
                        if (num != 1)
                        {
                            return false;
                        }
                        this.<>1__state = -1;
                        goto TR_0008;
                    }
                TR_0004:
                    this.<c>5__1 = this.<>4__this.sources[this.<>4__this.sources.Count - 1];
                    if (this.<c>5__1.MoveNext())
                    {
                        this.<>2__current = this.<c>5__1.Current;
                        this.<>1__state = 1;
                        return true;
                    }
                    this.<c>5__1.Dispose();
                    this.<>4__this.sources.RemoveAt(this.<>4__this.sources.Count - 1);
                TR_0008:
                    while (true)
                    {
                        this.<c>5__1 = null;
                        if (this.<>4__this.sources.Count > 0)
                        {
                            break;
                        }
                        return false;
                    }
                    goto TR_0004;
                }

                [DebuggerHidden]
                void IEnumerator.Reset()
                {
                    throw new NotSupportedException();
                }

                [DebuggerHidden]
                void IDisposable.Dispose()
                {
                }

                string IEnumerator<string>.Current =>
                    this.<>2__current;

                object IEnumerator.Current =>
                    this.<>2__current;
            }
        }

        internal sealed class Category : Option
        {
            public Category(string description) : base("=:Category:= " + description, description)
            {
            }

            protected override void OnParseComplete(OptionContext c)
            {
                throw new NotSupportedException("Category.OnParseComplete should not be invoked.");
            }
        }
    }
}

