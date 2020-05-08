namespace Squirrel.Bsdiff
{
    using ICSharpCode.SharpZipLib.BZip2;
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal class BinaryPatchUtility
    {
        private const long c_fileSignature = 0x3034464649445342L;
        private const int c_headerSize = 0x20;

        public static unsafe void Apply(Stream input, Func<Stream> openPatchStream, Stream output)
        {
            long num;
            long num2;
            long num3;
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            if (openPatchStream == null)
            {
                throw new ArgumentNullException("openPatchStream");
            }
            if (output == null)
            {
                throw new ArgumentNullException("output");
            }
            using (Stream stream = openPatchStream())
            {
                if (!stream.CanRead)
                {
                    throw new ArgumentException("Patch stream must be readable.", "openPatchStream");
                }
                if (!stream.CanSeek)
                {
                    throw new ArgumentException("Patch stream must be seekable.", "openPatchStream");
                }
                byte[] buf = stream.ReadExactly(0x20);
                if (ReadInt64(buf, 0) != 0x3034464649445342L)
                {
                    throw new InvalidOperationException("Corrupt patch.");
                }
                byte[] local1 = buf;
                num = ReadInt64(local1, 8);
                num2 = ReadInt64(local1, 0x10);
                num3 = ReadInt64(local1, 0x18);
                if ((num < 0L) || ((num2 < 0L) || (num3 < 0L)))
                {
                    throw new InvalidOperationException("Corrupt patch.");
                }
            }
            byte[] buffer = new byte[0x100000];
            byte[] buffer2 = new byte[0x100000];
            using (Stream stream2 = openPatchStream())
            {
                using (Stream stream3 = openPatchStream())
                {
                    using (Stream stream4 = openPatchStream())
                    {
                        stream2.Seek((long) 0x20, SeekOrigin.Current);
                        stream3.Seek(0x20 + num, SeekOrigin.Current);
                        stream4.Seek((0x20 + num) + num2, SeekOrigin.Current);
                        using (BZip2InputStream stream5 = new BZip2InputStream(stream2))
                        {
                            using (BZip2InputStream stream6 = new BZip2InputStream(stream3))
                            {
                                using (BZip2InputStream stream7 = new BZip2InputStream(stream4))
                                {
                                    long[] numArray = new long[3];
                                    byte[] buffer3 = new byte[8];
                                    int num4 = 0;
                                    int num5 = 0;
                                    while (num5 < num3)
                                    {
                                        int index = 0;
                                        while (true)
                                        {
                                            if (index >= 3)
                                            {
                                                if ((num5 + numArray[0]) > num3)
                                                {
                                                    throw new InvalidOperationException("Corrupt patch.");
                                                }
                                                input.Position = num4;
                                                int num6 = (int) numArray[0];
                                                while (true)
                                                {
                                                    if (num6 <= 0)
                                                    {
                                                        if ((num5 + numArray[1]) > num3)
                                                        {
                                                            throw new InvalidOperationException("Corrupt patch.");
                                                        }
                                                        num6 = (int) numArray[1];
                                                        while (true)
                                                        {
                                                            if (num6 <= 0)
                                                            {
                                                                num4 += (int) numArray[2];
                                                                break;
                                                            }
                                                            int num11 = Math.Min(num6, 0x100000);
                                                            stream7.ReadExactly(buffer, 0, num11);
                                                            output.Write(buffer, 0, num11);
                                                            num5 += num11;
                                                            num6 -= num11;
                                                        }
                                                        break;
                                                    }
                                                    int count = Math.Min(num6, 0x100000);
                                                    stream6.ReadExactly(buffer, 0, count);
                                                    int num9 = Math.Min(count, (int) (input.Length - input.Position));
                                                    input.ReadExactly(buffer2, 0, num9);
                                                    int num10 = 0;
                                                    while (true)
                                                    {
                                                        if (num10 >= num9)
                                                        {
                                                            output.Write(buffer, 0, count);
                                                            num5 += count;
                                                            num4 += count;
                                                            num6 -= count;
                                                            break;
                                                        }
                                                        byte* numPtr1 = &(buffer[num10]);
                                                        numPtr1[0] = (byte) (numPtr1[0] + buffer2[num10]);
                                                        num10++;
                                                    }
                                                }
                                                break;
                                            }
                                            stream5.ReadExactly(buffer3, 0, 8);
                                            numArray[index] = ReadInt64(buffer3, 0);
                                            index++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static int CompareBytes(byte[] left, int leftOffset, byte[] right, int rightOffset)
        {
            for (int i = 0; (i < (left.Length - leftOffset)) && (i < (right.Length - rightOffset)); i++)
            {
                int num2 = left[i + leftOffset] - right[i + rightOffset];
                if (num2 != 0)
                {
                    return num2;
                }
            }
            return 0;
        }

        public static void Create(byte[] oldData, byte[] newData, Stream output)
        {
            Exception ex = null;
            Thread thread1 = new Thread(delegate {
                try
                {
                    CreateInternal(oldData, newData, output);
                }
                catch (Exception exception)
                {
                    ex = exception;
                }
            }, 0x2800000);
            thread1.Start();
            thread1.Join();
            if (ex != null)
            {
                throw ex;
            }
        }

        private static void CreateInternal(byte[] oldData, byte[] newData, Stream output)
        {
            long num4;
            if (oldData == null)
            {
                throw new ArgumentNullException("oldData");
            }
            if (newData == null)
            {
                throw new ArgumentNullException("newData");
            }
            if (output == null)
            {
                throw new ArgumentNullException("output");
            }
            if (!output.CanSeek)
            {
                throw new ArgumentException("Output stream must be seekable.", "output");
            }
            if (!output.CanWrite)
            {
                throw new ArgumentException("Output stream must be writable.", "output");
            }
            byte[] buf = new byte[0x20];
            WriteInt64(0x3034464649445342L, buf, 0);
            WriteInt64(0L, buf, 8);
            WriteInt64(0L, buf, 0x10);
            WriteInt64((long) newData.Length, buf, 0x18);
            long position = output.Position;
            output.Write(buf, 0, buf.Length);
            int[] i = SuffixSort(oldData);
            byte[] buffer = new byte[newData.Length];
            byte[] buffer3 = new byte[newData.Length];
            int count = 0;
            int num3 = 0;
            using (WrappingStream stream = new WrappingStream(output, Ownership.None))
            {
                using (BZip2OutputStream stream2 = new BZip2OutputStream(stream))
                {
                    int num12;
                    int newOffset = 0;
                    int pos = 0;
                    int num8 = 0;
                    int num9 = 0;
                    int num10 = 0;
                    int num11 = 0;
                    goto TR_0059;
                TR_0049:
                    if ((num8 != num12) || (newOffset == newData.Length))
                    {
                        int num14 = 0;
                        int num15 = 0;
                        int num16 = 0;
                        int num18 = 0;
                        while (true)
                        {
                            if (((num9 + num18) >= newOffset) || ((num10 + num18) >= oldData.Length))
                            {
                                int num17 = 0;
                                if (newOffset < newData.Length)
                                {
                                    num14 = 0;
                                    int num19 = 0;
                                    for (int j = 1; (newOffset >= (num9 + j)) && (pos >= j); j++)
                                    {
                                        if (oldData[pos - j] == newData[newOffset - j])
                                        {
                                            num14++;
                                        }
                                        if (((num14 * 2) - j) > ((num19 * 2) - num17))
                                        {
                                            num19 = num14;
                                            num17 = j;
                                        }
                                    }
                                }
                                if ((num9 + num16) > (newOffset - num17))
                                {
                                    int num21 = (num9 + num16) - (newOffset - num17);
                                    num14 = 0;
                                    int num22 = 0;
                                    int num23 = 0;
                                    int num24 = 0;
                                    while (true)
                                    {
                                        if (num24 >= num21)
                                        {
                                            num16 += num23 - num21;
                                            num17 -= num23;
                                            break;
                                        }
                                        if (newData[((num9 + num16) - num21) + num24] == oldData[((num10 + num16) - num21) + num24])
                                        {
                                            num14++;
                                        }
                                        if (newData[(newOffset - num17) + num24] == oldData[(pos - num17) + num24])
                                        {
                                            num14--;
                                        }
                                        if (num14 > num22)
                                        {
                                            num22 = num14;
                                            num23 = num24 + 1;
                                        }
                                        num24++;
                                    }
                                }
                                int num25 = 0;
                                while (true)
                                {
                                    if (num25 >= num16)
                                    {
                                        int num26 = 0;
                                        while (true)
                                        {
                                            if (num26 >= ((newOffset - num17) - (num9 + num16)))
                                            {
                                                count += num16;
                                                num3 += (newOffset - num17) - (num9 + num16);
                                                byte[] buffer4 = new byte[8];
                                                WriteInt64((long) num16, buffer4, 0);
                                                stream2.Write(buffer4, 0, 8);
                                                WriteInt64((long) ((newOffset - num17) - (num9 + num16)), buffer4, 0);
                                                stream2.Write(buffer4, 0, 8);
                                                WriteInt64((long) ((pos - num17) - (num10 + num16)), buffer4, 0);
                                                stream2.Write(buffer4, 0, 8);
                                                num9 = newOffset - num17;
                                                num10 = pos - num17;
                                                num11 = pos - newOffset;
                                                break;
                                            }
                                            buffer3[num3 + num26] = newData[(num9 + num16) + num26];
                                            num26++;
                                        }
                                        break;
                                    }
                                    buffer[count + num25] = (byte) (newData[num9 + num25] - oldData[num10 + num25]);
                                    num25++;
                                }
                                break;
                            }
                            if (oldData[num10 + num18] == newData[num9 + num18])
                            {
                                num14++;
                            }
                            num18++;
                            if (((num14 * 2) - num18) > ((num15 * 2) - num16))
                            {
                                num15 = num14;
                                num16 = num18;
                            }
                        }
                    }
                TR_0059:
                    while (true)
                    {
                        if (newOffset < newData.Length)
                        {
                            num12 = 0;
                            int num1 = newOffset + num8;
                            int index = newOffset = num1;
                            while (true)
                            {
                                if (newOffset >= newData.Length)
                                {
                                    break;
                                }
                                num8 = Search(i, oldData, newData, newOffset, 0, oldData.Length, out pos);
                                while (true)
                                {
                                    if (index < (newOffset + num8))
                                    {
                                        if (((index + num11) < oldData.Length) && (oldData[index + num11] == newData[index]))
                                        {
                                            num12++;
                                        }
                                        index++;
                                        continue;
                                    }
                                    if (((num8 != num12) || (num8 == 0)) && (num8 <= (num12 + 8)))
                                    {
                                        if (((newOffset + num11) < oldData.Length) && (oldData[newOffset + num11] == newData[newOffset]))
                                        {
                                            num12--;
                                        }
                                        newOffset++;
                                        break;
                                    }
                                    break;
                                }
                            }
                        }
                        else
                        {
                            goto TR_0021;
                        }
                        break;
                    }
                    goto TR_0049;
                }
            }
        TR_0021:
            num4 = output.Position;
            WriteInt64((num4 - position) - 0x20, buf, 8);
            using (WrappingStream stream3 = new WrappingStream(output, Ownership.None))
            {
                using (BZip2OutputStream stream4 = new BZip2OutputStream(stream3))
                {
                    stream4.Write(buffer, 0, count);
                }
            }
            WriteInt64(output.Position - num4, buf, 0x10);
            using (WrappingStream stream5 = new WrappingStream(output, Ownership.None))
            {
                using (BZip2OutputStream stream6 = new BZip2OutputStream(stream5))
                {
                    stream6.Write(buffer3, 0, num3);
                }
            }
            long num5 = output.Position;
            output.Position = position;
            output.Write(buf, 0, buf.Length);
            output.Position = num5;
        }

        private static int MatchLength(byte[] oldData, int oldOffset, byte[] newData, int newOffset)
        {
            int num = 0;
            while ((num < (oldData.Length - oldOffset)) && ((num < (newData.Length - newOffset)) && (oldData[num + oldOffset] == newData[num + newOffset])))
            {
                num++;
            }
            return num;
        }

        private static long ReadInt64(byte[] buf, int offset)
        {
            long num = buf[offset + 7] & 0x7f;
            for (int i = 6; i >= 0; i--)
            {
                num = (num * 0x100L) + buf[offset + i];
            }
            if ((buf[offset + 7] & 0x80) != 0)
            {
                num = -num;
            }
            return num;
        }

        private static int Search(int[] I, byte[] oldData, byte[] newData, int newOffset, int start, int end, out int pos)
        {
            if ((end - start) >= 2)
            {
                int index = start + ((end - start) / 2);
                return ((CompareBytes(oldData, I[index], newData, newOffset) < 0) ? Search(I, oldData, newData, newOffset, index, end, out pos) : Search(I, oldData, newData, newOffset, start, index, out pos));
            }
            int num = MatchLength(oldData, I[start], newData, newOffset);
            int num2 = MatchLength(oldData, I[end], newData, newOffset);
            if (num > num2)
            {
                pos = I[start];
                return num;
            }
            pos = I[end];
            return num2;
        }

        private static void Split(int[] I, int[] v, int start, int len, int h)
        {
            if (len < 0x10)
            {
                int index = start;
                while (index < (start + len))
                {
                    int num = 1;
                    int num3 = v[I[index] + h];
                    int num4 = 1;
                    while (true)
                    {
                        if ((index + num4) >= (start + len))
                        {
                            int num5 = 0;
                            while (true)
                            {
                                if (num5 >= num)
                                {
                                    if (num == 1)
                                    {
                                        I[index] = -1;
                                    }
                                    index += num;
                                    break;
                                }
                                v[I[index + num5]] = (index + num) - 1;
                                num5++;
                            }
                            break;
                        }
                        if (v[I[index + num4] + h] < num3)
                        {
                            num3 = v[I[index + num4] + h];
                            num = 0;
                        }
                        if (v[I[index + num4] + h] == num3)
                        {
                            Swap(ref I[index + num], ref I[index + num4]);
                            num++;
                        }
                        num4++;
                    }
                }
            }
            else
            {
                int num6 = v[I[start + (len / 2)] + h];
                int index = 0;
                int num8 = 0;
                for (int i = start; i < (start + len); i++)
                {
                    if (v[I[i] + h] < num6)
                    {
                        index++;
                    }
                    if (v[I[i] + h] == num6)
                    {
                        num8++;
                    }
                }
                index += start;
                num8 += index;
                int num9 = start;
                int num10 = 0;
                int num11 = 0;
                while (num9 < index)
                {
                    if (v[I[num9] + h] < num6)
                    {
                        num9++;
                        continue;
                    }
                    if (v[I[num9] + h] == num6)
                    {
                        Swap(ref I[num9], ref I[index + num10]);
                        num10++;
                        continue;
                    }
                    Swap(ref I[num9], ref I[num8 + num11]);
                    num11++;
                }
                while ((index + num10) < num8)
                {
                    if (v[I[index + num10] + h] == num6)
                    {
                        num10++;
                        continue;
                    }
                    Swap(ref I[index + num10], ref I[num8 + num11]);
                    num11++;
                }
                if (index > start)
                {
                    Split(I, v, start, index - start, h);
                }
                for (num9 = 0; num9 < (num8 - index); num9++)
                {
                    v[I[index + num9]] = num8 - 1;
                }
                if (index == (num8 - 1))
                {
                    I[index] = -1;
                }
                if ((start + len) > num8)
                {
                    Split(I, v, num8, (start + len) - num8, h);
                }
            }
        }

        private static unsafe int[] SuffixSort(byte[] oldData)
        {
            int[] numArray = new int[0x100];
            byte[] buffer = oldData;
            int index = 0;
            while (index < buffer.Length)
            {
                byte num2 = buffer[index];
                int* numPtr1 = &(numArray[num2]);
                numPtr1[0]++;
                index++;
            }
            for (int i = 1; i < 0x100; i++)
            {
                int* numPtr2 = &(numArray[i]);
                numPtr2[0] += numArray[i - 1];
            }
            for (int j = 0xff; j > 0; j--)
            {
                numArray[j] = numArray[j - 1];
            }
            numArray[0] = 0;
            int[] numArray2 = new int[oldData.Length + 1];
            for (int k = 0; k < oldData.Length; k++)
            {
                int* numPtr3 = &(numArray[oldData[k]]);
                index = numPtr3[0] + 1;
                numPtr3[0] = index;
                numArray2[index] = k;
            }
            int[] v = new int[oldData.Length + 1];
            for (int m = 0; m < oldData.Length; m++)
            {
                v[m] = numArray[oldData[m]];
            }
            for (int n = 1; n < 0x100; n++)
            {
                if (numArray[n] == (numArray[n - 1] + 1))
                {
                    numArray2[numArray[n]] = -1;
                }
            }
            numArray2[0] = -1;
            int h = 1;
            while (numArray2[0] != -(oldData.Length + 1))
            {
                int len = 0;
                int num10 = 0;
                while (true)
                {
                    if (num10 >= (oldData.Length + 1))
                    {
                        if (len != 0)
                        {
                            numArray2[num10 - len] = -len;
                        }
                        h += h;
                        break;
                    }
                    if (numArray2[num10] < 0)
                    {
                        len -= numArray2[num10];
                        num10 -= numArray2[num10];
                        continue;
                    }
                    if (len != 0)
                    {
                        numArray2[num10 - len] = -len;
                    }
                    len = (v[numArray2[num10]] + 1) - num10;
                    Split(numArray2, v, num10, len, h);
                    num10 += len;
                    len = 0;
                }
            }
            for (int num11 = 0; num11 < (oldData.Length + 1); num11++)
            {
                numArray2[v[num11]] = num11;
            }
            return numArray2;
        }

        private static void Swap(ref int first, ref int second)
        {
            int num = first;
            first = second;
            second = num;
        }

        private static unsafe void WriteInt64(long value, byte[] buf, int offset)
        {
            long num = (value < 0L) ? -value : value;
            for (int i = 0; i < 8; i++)
            {
                buf[offset + i] = (byte) (num % 0x100L);
                num = (num - buf[offset + i]) / 0x100L;
            }
            if (value < 0L)
            {
                byte* numPtr1 = &(buf[offset + 7]);
                numPtr1[0] = (byte) (numPtr1[0] | 0x80);
            }
        }
    }
}

