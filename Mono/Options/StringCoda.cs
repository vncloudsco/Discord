namespace Mono.Options
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    internal static class StringCoda
    {
        [IteratorStateMachine(typeof(<CreateWrappedLinesIterator>d__2))]
        private static IEnumerable<string> CreateWrappedLinesIterator(string self, IEnumerable<int> widths)
        {
            IEnumerator<int> enumerator;
            bool? <hw>5__5;
            int <width>5__4;
            int num2;
            if (!string.IsNullOrEmpty(self))
            {
                enumerator = widths.GetEnumerator();
                <hw>5__5 = null;
                <width>5__4 = GetNextWidth(enumerator, 0x7fffffff, ref <hw>5__5);
                num2 = 0;
            }
            else
            {
                yield return string.Empty;
            }
            do
            {
                int num3;
                int <end>5__1 = GetLineEnd(num2, <width>5__4, self);
                char c = self[<end>5__1 - 1];
                if (char.IsWhiteSpace(c))
                {
                    num3 = <end>5__1 - 1;
                    <end>5__1 = num3;
                }
                string str = "";
                if ((<end>5__1 != self.Length) && !IsEolChar(c))
                {
                    num3 = <end>5__1 - 1;
                    <end>5__1 = num3;
                    str = "-";
                }
                string str2 = self.Substring(num2, <end>5__1 - num2) + str;
                yield return str2;
                num2 = <end>5__1;
                if (char.IsWhiteSpace(c))
                {
                    num2++;
                }
                <width>5__4 = GetNextWidth(enumerator, <width>5__4, ref <hw>5__5);
            }
            while (num2 < self.Length);
            <hw>5__5 = null;
            enumerator = null;
        }

        private static int GetLineEnd(int start, int length, string description)
        {
            int num = Math.Min(start + length, description.Length);
            int num2 = -1;
            for (int i = start; i < num; i++)
            {
                if (description[i] == '\n')
                {
                    return (i + 1);
                }
                if (IsEolChar(description[i]))
                {
                    num2 = i + 1;
                }
            }
            return (((num2 == -1) || (num == description.Length)) ? num : num2);
        }

        private static int GetNextWidth(IEnumerator<int> ewidths, int curWidth, ref bool? eValid)
        {
            if ((eValid == 0) || ((eValid != 0) && eValid.Value))
            {
                bool? nullable;
                eValid = nullable = new bool?(ewidths.MoveNext());
                curWidth = nullable.Value ? ewidths.Current : curWidth;
                if (curWidth < ".-".Length)
                {
                    throw new ArgumentOutOfRangeException("widths", $"Element must be >= {".-".Length}, was {curWidth}.");
                }
            }
            return curWidth;
        }

        private static bool IsEolChar(char c) => 
            !char.IsLetterOrDigit(c);

        public static IEnumerable<string> WrappedLines(string self, IEnumerable<int> widths)
        {
            if (widths == null)
            {
                throw new ArgumentNullException("widths");
            }
            return CreateWrappedLinesIterator(self, widths);
        }

        public static IEnumerable<string> WrappedLines(string self, params int[] widths)
        {
            IEnumerable<int> enumerable = widths;
            return WrappedLines(self, enumerable);
        }

    }
}

