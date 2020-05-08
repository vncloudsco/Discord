namespace Mono.Options
{
    using System;

    public class OptionContext
    {
        private Mono.Options.Option option;
        private string name;
        private int index;
        private Mono.Options.OptionSet set;
        private OptionValueCollection c;

        public OptionContext(Mono.Options.OptionSet set)
        {
            this.set = set;
            this.c = new OptionValueCollection(this);
        }

        public Mono.Options.Option Option
        {
            get => 
                this.option;
            set => 
                (this.option = value);
        }

        public string OptionName
        {
            get => 
                this.name;
            set => 
                (this.name = value);
        }

        public int OptionIndex
        {
            get => 
                this.index;
            set => 
                (this.index = value);
        }

        public Mono.Options.OptionSet OptionSet =>
            this.set;

        public OptionValueCollection OptionValues =>
            this.c;
    }
}

