namespace Mono.Cecil.Metadata
{
    using System;

    internal sealed class UserStringHeapBuffer : StringHeapBuffer
    {
        protected override void WriteString(string @string)
        {
            base.WriteCompressedUInt32((uint) ((@string.Length * 2) + 1));
            byte num = 0;
            for (int i = 0; i < @string.Length; i++)
            {
                char ch = @string[i];
                base.WriteUInt16(ch);
                if (((num != 1) && ((ch < ' ') || (ch > '~'))) && ((((ch > '~') || ((ch >= '\x0001') && (ch <= '\b'))) || (((ch >= '\x000e') && (ch <= '\x001f')) || (ch == '\''))) || (ch == '-')))
                {
                    num = 1;
                }
            }
            base.WriteByte(num);
        }
    }
}

