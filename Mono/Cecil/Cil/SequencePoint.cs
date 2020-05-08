namespace Mono.Cecil.Cil
{
    using System;

    internal sealed class SequencePoint
    {
        private Mono.Cecil.Cil.Document document;
        private int start_line;
        private int start_column;
        private int end_line;
        private int end_column;

        public SequencePoint(Mono.Cecil.Cil.Document document)
        {
            this.document = document;
        }

        public int StartLine
        {
            get => 
                this.start_line;
            set => 
                (this.start_line = value);
        }

        public int StartColumn
        {
            get => 
                this.start_column;
            set => 
                (this.start_column = value);
        }

        public int EndLine
        {
            get => 
                this.end_line;
            set => 
                (this.end_line = value);
        }

        public int EndColumn
        {
            get => 
                this.end_column;
            set => 
                (this.end_column = value);
        }

        public Mono.Cecil.Cil.Document Document
        {
            get => 
                this.document;
            set => 
                (this.document = value);
        }
    }
}

