namespace Mono.Cecil.PE
{
    using Mono.Cecil;
    using System;

    internal sealed class TextMap
    {
        private readonly Range[] map = new Range[0x10];

        public void AddMap(TextSegment segment, Range range)
        {
            this.map[(int) segment] = range;
        }

        public void AddMap(TextSegment segment, int length)
        {
            this.map[(int) segment] = new Range(this.GetStart(segment), (uint) length);
        }

        public void AddMap(TextSegment segment, int length, int align)
        {
            align--;
            this.AddMap(segment, (int) ((length + align) & ~align));
        }

        private uint ComputeStart(int index)
        {
            index--;
            return (this.map[index].Start + this.map[index].Length);
        }

        public DataDirectory GetDataDirectory(TextSegment segment)
        {
            Range range = this.map[(int) segment];
            return new DataDirectory((range.Length == 0) ? 0 : range.Start, range.Length);
        }

        public uint GetLength()
        {
            Range range = this.map[15];
            return ((range.Start - 0x2000) + range.Length);
        }

        public int GetLength(TextSegment segment) => 
            ((int) this.map[(int) segment].Length);

        public uint GetNextRVA(TextSegment segment)
        {
            int index = (int) segment;
            return (this.map[index].Start + this.map[index].Length);
        }

        public Range GetRange(TextSegment segment) => 
            this.map[(int) segment];

        public uint GetRVA(TextSegment segment) => 
            this.map[(int) segment].Start;

        private uint GetStart(TextSegment segment)
        {
            int index = (int) segment;
            return ((index == 0) ? 0x2000 : this.ComputeStart(index));
        }
    }
}

