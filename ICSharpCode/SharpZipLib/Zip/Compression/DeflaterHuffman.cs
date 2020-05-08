namespace ICSharpCode.SharpZipLib.Zip.Compression
{
    using ICSharpCode.SharpZipLib;
    using System;

    internal class DeflaterHuffman
    {
        private const int BUFSIZE = 0x4000;
        private const int LITERAL_NUM = 0x11e;
        private const int DIST_NUM = 30;
        private const int BITLEN_NUM = 0x13;
        private const int REP_3_6 = 0x10;
        private const int REP_3_10 = 0x11;
        private const int REP_11_138 = 0x12;
        private const int EOF_SYMBOL = 0x100;
        private static readonly int[] BL_ORDER = new int[] { 
            0x10, 0x11, 0x12, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2,
            14, 1, 15
        };
        private static readonly byte[] bit4Reverse = new byte[] { 0, 8, 4, 12, 2, 10, 6, 14, 1, 9, 5, 13, 3, 11, 7, 15 };
        private static short[] staticLCodes = new short[0x11e];
        private static byte[] staticLLength = new byte[0x11e];
        private static short[] staticDCodes;
        private static byte[] staticDLength;
        public DeflaterPending pending;
        private Tree literalTree;
        private Tree distTree;
        private Tree blTree;
        private short[] d_buf;
        private byte[] l_buf;
        private int last_lit;
        private int extra_bits;

        static DeflaterHuffman()
        {
            int index = 0;
            while (index < 0x90)
            {
                staticLCodes[index] = BitReverse((0x30 + index) << 8);
                staticLLength[index++] = 8;
            }
            while (index < 0x100)
            {
                staticLCodes[index] = BitReverse((0x100 + index) << 7);
                staticLLength[index++] = 9;
            }
            while (index < 280)
            {
                staticLCodes[index] = BitReverse((-256 + index) << 9);
                staticLLength[index++] = 7;
            }
            while (index < 0x11e)
            {
                staticLCodes[index] = BitReverse((-88 + index) << 8);
                staticLLength[index++] = 8;
            }
            staticDCodes = new short[30];
            staticDLength = new byte[30];
            for (index = 0; index < 30; index++)
            {
                staticDCodes[index] = BitReverse(index << 11);
                staticDLength[index] = 5;
            }
        }

        public DeflaterHuffman(DeflaterPending pending)
        {
            this.pending = pending;
            this.literalTree = new Tree(this, 0x11e, 0x101, 15);
            this.distTree = new Tree(this, 30, 1, 15);
            this.blTree = new Tree(this, 0x13, 4, 7);
            this.d_buf = new short[0x4000];
            this.l_buf = new byte[0x4000];
        }

        public static short BitReverse(int toReverse) => 
            ((short) ((((bit4Reverse[toReverse & 15] << 12) | (bit4Reverse[(toReverse >> 4) & 15] << 8)) | (bit4Reverse[(toReverse >> 8) & 15] << 4)) | bit4Reverse[toReverse >> 12]));

        public void CompressBlock()
        {
            for (int i = 0; i < this.last_lit; i++)
            {
                int code = this.l_buf[i] & 0xff;
                int distance = this.d_buf[i];
                if (distance-- == 0)
                {
                    this.literalTree.WriteSymbol(code);
                }
                else
                {
                    int num4 = Lcode(code);
                    this.literalTree.WriteSymbol(num4);
                    int count = (num4 - 0x105) / 4;
                    if ((count > 0) && (count <= 5))
                    {
                        this.pending.WriteBits(code & ((1 << (count & 0x1f)) - 1), count);
                    }
                    int num6 = Dcode(distance);
                    this.distTree.WriteSymbol(num6);
                    count = (num6 / 2) - 1;
                    if (count > 0)
                    {
                        this.pending.WriteBits(distance & ((1 << (count & 0x1f)) - 1), count);
                    }
                }
            }
            this.literalTree.WriteSymbol(0x100);
        }

        private static int Dcode(int distance)
        {
            int num = 0;
            while (distance >= 4)
            {
                num += 2;
                distance = distance >> 1;
            }
            return (num + distance);
        }

        public unsafe void FlushBlock(byte[] stored, int storedOffset, int storedLength, bool lastBlock)
        {
            short* numPtr1 = &(this.literalTree.freqs[0x100]);
            numPtr1[0] = (short) (numPtr1[0] + 1);
            this.literalTree.BuildTree();
            this.distTree.BuildTree();
            this.literalTree.CalcBLFreq(this.blTree);
            this.distTree.CalcBLFreq(this.blTree);
            this.blTree.BuildTree();
            int blTreeCodes = 4;
            for (int i = 0x12; i > blTreeCodes; i--)
            {
                if (this.blTree.length[BL_ORDER[i]] > 0)
                {
                    blTreeCodes = i + 1;
                }
            }
            int num2 = ((((14 + (blTreeCodes * 3)) + this.blTree.GetEncodedLength()) + this.literalTree.GetEncodedLength()) + this.distTree.GetEncodedLength()) + this.extra_bits;
            int num3 = this.extra_bits;
            for (int j = 0; j < 0x11e; j++)
            {
                num3 += this.literalTree.freqs[j] * staticLLength[j];
            }
            for (int k = 0; k < 30; k++)
            {
                num3 += this.distTree.freqs[k] * staticDLength[k];
            }
            if (num2 >= num3)
            {
                num2 = num3;
            }
            if ((storedOffset >= 0) && ((storedLength + 4) < (num2 >> 3)))
            {
                this.FlushStoredBlock(stored, storedOffset, storedLength, lastBlock);
            }
            else if (num2 != num3)
            {
                this.pending.WriteBits((2 << 1) + (lastBlock ? 1 : 0), 3);
                this.SendAllTrees(blTreeCodes);
                this.CompressBlock();
                this.Reset();
            }
            else
            {
                this.pending.WriteBits((1 << 1) + (lastBlock ? 1 : 0), 3);
                this.literalTree.SetStaticCodes(staticLCodes, staticLLength);
                this.distTree.SetStaticCodes(staticDCodes, staticDLength);
                this.CompressBlock();
                this.Reset();
            }
        }

        public void FlushStoredBlock(byte[] stored, int storedOffset, int storedLength, bool lastBlock)
        {
            this.pending.WriteBits((0 << 1) + (lastBlock ? 1 : 0), 3);
            this.pending.AlignToByte();
            this.pending.WriteShort(storedLength);
            this.pending.WriteShort(~storedLength);
            this.pending.WriteBlock(stored, storedOffset, storedLength);
            this.Reset();
        }

        public bool IsFull() => 
            (this.last_lit >= 0x4000);

        private static int Lcode(int length)
        {
            if (length == 0xff)
            {
                return 0x11d;
            }
            int num = 0x101;
            while (length >= 8)
            {
                num += 4;
                length = length >> 1;
            }
            return (num + length);
        }

        public void Reset()
        {
            this.last_lit = 0;
            this.extra_bits = 0;
            this.literalTree.Reset();
            this.distTree.Reset();
            this.blTree.Reset();
        }

        public void SendAllTrees(int blTreeCodes)
        {
            this.blTree.BuildCodes();
            this.literalTree.BuildCodes();
            this.distTree.BuildCodes();
            this.pending.WriteBits(this.literalTree.numCodes - 0x101, 5);
            this.pending.WriteBits(this.distTree.numCodes - 1, 5);
            this.pending.WriteBits(blTreeCodes - 4, 4);
            for (int i = 0; i < blTreeCodes; i++)
            {
                this.pending.WriteBits(this.blTree.length[BL_ORDER[i]], 3);
            }
            this.literalTree.WriteTree(this.blTree);
            this.distTree.WriteTree(this.blTree);
        }

        public unsafe bool TallyDist(int distance, int length)
        {
            this.d_buf[this.last_lit] = (short) distance;
            int index = this.last_lit;
            this.last_lit = index + 1;
            this.l_buf[index] = (byte) (length - 3);
            int num = Lcode(length - 3);
            short* numPtr1 = &(this.literalTree.freqs[num]);
            numPtr1[0] = (short) (numPtr1[0] + 1);
            if ((num >= 0x109) && (num < 0x11d))
            {
                this.extra_bits += (num - 0x105) / 4;
            }
            int num2 = Dcode(distance - 1);
            short* numPtr2 = &(this.distTree.freqs[num2]);
            numPtr2[0] = (short) (numPtr2[0] + 1);
            if (num2 >= 4)
            {
                this.extra_bits += (num2 / 2) - 1;
            }
            return this.IsFull();
        }

        public unsafe bool TallyLit(int literal)
        {
            this.d_buf[this.last_lit] = 0;
            int index = this.last_lit;
            this.last_lit = index + 1;
            this.l_buf[index] = (byte) literal;
            short* numPtr1 = &(this.literalTree.freqs[literal]);
            numPtr1[0] = (short) (numPtr1[0] + 1);
            return this.IsFull();
        }

        private class Tree
        {
            public short[] freqs;
            public byte[] length;
            public int minNumCodes;
            public int numCodes;
            private short[] codes;
            private int[] bl_counts;
            private int maxLength;
            private DeflaterHuffman dh;

            public Tree(DeflaterHuffman dh, int elems, int minCodes, int maxLength)
            {
                this.dh = dh;
                this.minNumCodes = minCodes;
                this.maxLength = maxLength;
                this.freqs = new short[elems];
                this.bl_counts = new int[maxLength];
            }

            public unsafe void BuildCodes()
            {
                int length = this.freqs.Length;
                int[] numArray = new int[this.maxLength];
                int num = 0;
                this.codes = new short[this.freqs.Length];
                for (int i = 0; i < this.maxLength; i++)
                {
                    numArray[i] = num;
                    num += this.bl_counts[i] << ((15 - i) & 0x1f);
                }
                for (int j = 0; j < this.numCodes; j++)
                {
                    int num4 = this.length[j];
                    if (num4 > 0)
                    {
                        this.codes[j] = DeflaterHuffman.BitReverse(numArray[num4 - 1]);
                        int* numPtr1 = &(numArray[num4 - 1]);
                        numPtr1[0] += 1 << ((0x10 - num4) & 0x1f);
                    }
                }
            }

            private unsafe void BuildLength(int[] childs)
            {
                this.length = new byte[this.freqs.Length];
                int num = childs.Length / 2;
                int num2 = (num + 1) / 2;
                int num3 = 0;
                for (int i = 0; i < this.maxLength; i++)
                {
                    this.bl_counts[i] = 0;
                }
                int[] numArray = new int[num];
                numArray[num - 1] = 0;
                for (int j = num - 1; j >= 0; j--)
                {
                    if (childs[(2 * j) + 1] == -1)
                    {
                        int num10 = numArray[j];
                        int* numPtr1 = &(this.bl_counts[num10 - 1]);
                        numPtr1[0]++;
                        this.length[childs[2 * j]] = (byte) numArray[j];
                    }
                    else
                    {
                        int maxLength = numArray[j] + 1;
                        if (maxLength > this.maxLength)
                        {
                            maxLength = this.maxLength;
                            num3++;
                        }
                        numArray[childs[2 * j]] = numArray[childs[(2 * j) + 1]] = maxLength;
                    }
                }
                if (num3 != 0)
                {
                    int index = this.maxLength - 1;
                    while (true)
                    {
                        if (this.bl_counts[--index] == 0)
                        {
                            continue;
                        }
                        while (true)
                        {
                            int* numPtr2 = &(this.bl_counts[index]);
                            numPtr2[0]--;
                            int* numPtr3 = &(this.bl_counts[++index]);
                            numPtr3[0]++;
                            num3 -= 1 << (((this.maxLength - 1) - index) & 0x1f);
                            if ((num3 <= 0) || (index >= (this.maxLength - 1)))
                            {
                                if (num3 > 0)
                                {
                                    break;
                                }
                                int* numPtr4 = &(this.bl_counts[this.maxLength - 1]);
                                numPtr4[0] += num3;
                                int* numPtr5 = &(this.bl_counts[this.maxLength - 2]);
                                numPtr5[0] -= num3;
                                int num5 = 2 * num2;
                                int maxLength = this.maxLength;
                                while (maxLength != 0)
                                {
                                    int num12 = this.bl_counts[maxLength - 1];
                                    while (true)
                                    {
                                        if (num12 <= 0)
                                        {
                                            maxLength--;
                                            break;
                                        }
                                        int num13 = 2 * childs[num5++];
                                        if (childs[num13 + 1] == -1)
                                        {
                                            this.length[childs[num13]] = (byte) maxLength;
                                            num12--;
                                        }
                                    }
                                }
                                return;
                            }
                        }
                    }
                }
            }

            public void BuildTree()
            {
                int length = this.freqs.Length;
                int[] numArray = new int[length];
                int num2 = 0;
                int num3 = 0;
                for (int i = 0; i < length; i++)
                {
                    int num6 = this.freqs[i];
                    if (num6 != 0)
                    {
                        int index = num2++;
                        while (true)
                        {
                            int num8;
                            if ((index <= 0) || (this.freqs[numArray[num8 = (index - 1) / 2]] <= num6))
                            {
                                numArray[index] = i;
                                num3 = i;
                                break;
                            }
                            numArray[index] = numArray[num8];
                            index = num8;
                        }
                    }
                }
                while (num2 < 2)
                {
                    numArray[num2++] = (num3 < 2) ? ++num3 : 0;
                }
                this.numCodes = Math.Max(num3 + 1, this.minNumCodes);
                int[] childs = new int[(4 * num2) - 2];
                int[] numArray3 = new int[(2 * num2) - 1];
                int num4 = num2;
                for (int j = 0; j < num2; j++)
                {
                    int index = numArray[j];
                    childs[2 * j] = index;
                    childs[(2 * j) + 1] = -1;
                    numArray3[j] = this.freqs[index] << 8;
                    numArray[j] = j;
                }
                while (true)
                {
                    int index = numArray[0];
                    int num13 = numArray[--num2];
                    int num14 = 0;
                    int num15 = 1;
                    while (true)
                    {
                        if (num15 >= num2)
                        {
                            int num16 = numArray3[num13];
                            while (true)
                            {
                                if (((num15 = num14) <= 0) || (numArray3[numArray[num14 = (num15 - 1) / 2]] <= num16))
                                {
                                    numArray[num15] = num13;
                                    int num17 = numArray[0];
                                    num13 = num4++;
                                    childs[2 * num13] = index;
                                    childs[(2 * num13) + 1] = num17;
                                    int num18 = Math.Min((int) (numArray3[index] & 0xff), (int) (numArray3[num17] & 0xff));
                                    numArray3[num13] = num16 = ((numArray3[index] + numArray3[num17]) - num18) + 1;
                                    num14 = 0;
                                    num15 = 1;
                                    while (true)
                                    {
                                        if (num15 >= num2)
                                        {
                                            while (true)
                                            {
                                                if (((num15 = num14) <= 0) || (numArray3[numArray[num14 = (num15 - 1) / 2]] <= num16))
                                                {
                                                    numArray[num15] = num13;
                                                    if (num2 > 1)
                                                    {
                                                        break;
                                                    }
                                                    if (numArray[0] != ((childs.Length / 2) - 1))
                                                    {
                                                        throw new SharpZipBaseException("Heap invariant violated");
                                                    }
                                                    this.BuildLength(childs);
                                                    return;
                                                }
                                                numArray[num15] = numArray[num14];
                                            }
                                            break;
                                        }
                                        if (((num15 + 1) < num2) && (numArray3[numArray[num15]] > numArray3[numArray[num15 + 1]]))
                                        {
                                            num15++;
                                        }
                                        numArray[num14] = numArray[num15];
                                        num15 = (num15 * 2) + 1;
                                    }
                                    break;
                                }
                                numArray[num15] = numArray[num14];
                            }
                            break;
                        }
                        if (((num15 + 1) < num2) && (numArray3[numArray[num15]] > numArray3[numArray[num15 + 1]]))
                        {
                            num15++;
                        }
                        numArray[num14] = numArray[num15];
                        num14 = num15;
                        num15 = (num15 * 2) + 1;
                    }
                }
            }

            public unsafe void CalcBLFreq(DeflaterHuffman.Tree blTree)
            {
                int index = -1;
                int num5 = 0;
                while (num5 < this.numCodes)
                {
                    int num;
                    int num2;
                    int num3 = 1;
                    int num6 = this.length[num5];
                    if (num6 == 0)
                    {
                        num = 0x8a;
                        num2 = 3;
                    }
                    else
                    {
                        num = 6;
                        num2 = 3;
                        if (index != num6)
                        {
                            short* numPtr1 = &(blTree.freqs[num6]);
                            numPtr1[0] = (short) (numPtr1[0] + 1);
                            num3 = 0;
                        }
                    }
                    index = num6;
                    num5++;
                    while (true)
                    {
                        if ((num5 < this.numCodes) && (index == this.length[num5]))
                        {
                            num5++;
                            if (++num3 < num)
                            {
                                continue;
                            }
                        }
                        if (num3 < num2)
                        {
                            short* numPtr2 = &(blTree.freqs[index]);
                            numPtr2[0] = (short) (numPtr2[0] + ((short) num3));
                        }
                        else if (index != 0)
                        {
                            short* numPtr3 = &(blTree.freqs[0x10]);
                            numPtr3[0] = (short) (numPtr3[0] + 1);
                        }
                        else if (num3 <= 10)
                        {
                            short* numPtr4 = &(blTree.freqs[0x11]);
                            numPtr4[0] = (short) (numPtr4[0] + 1);
                        }
                        else
                        {
                            short* numPtr5 = &(blTree.freqs[0x12]);
                            numPtr5[0] = (short) (numPtr5[0] + 1);
                        }
                        break;
                    }
                }
            }

            public void CheckEmpty()
            {
                bool flag = true;
                for (int i = 0; i < this.freqs.Length; i++)
                {
                    if (this.freqs[i] != 0)
                    {
                        flag = false;
                    }
                }
                if (!flag)
                {
                    throw new SharpZipBaseException("!Empty");
                }
            }

            public int GetEncodedLength()
            {
                int num = 0;
                for (int i = 0; i < this.freqs.Length; i++)
                {
                    num += this.freqs[i] * this.length[i];
                }
                return num;
            }

            public void Reset()
            {
                for (int i = 0; i < this.freqs.Length; i++)
                {
                    this.freqs[i] = 0;
                }
                this.codes = null;
                this.length = null;
            }

            public void SetStaticCodes(short[] staticCodes, byte[] staticLengths)
            {
                this.codes = staticCodes;
                this.length = staticLengths;
            }

            public void WriteSymbol(int code)
            {
                this.dh.pending.WriteBits(this.codes[code] & 0xffff, this.length[code]);
            }

            public void WriteTree(DeflaterHuffman.Tree blTree)
            {
                int code = -1;
                int index = 0;
                while (index < this.numCodes)
                {
                    int num;
                    int num2;
                    int num3 = 1;
                    int num6 = this.length[index];
                    if (num6 == 0)
                    {
                        num = 0x8a;
                        num2 = 3;
                    }
                    else
                    {
                        num = 6;
                        num2 = 3;
                        if (code != num6)
                        {
                            blTree.WriteSymbol(num6);
                            num3 = 0;
                        }
                    }
                    code = num6;
                    index++;
                    while (true)
                    {
                        if ((index < this.numCodes) && (code == this.length[index]))
                        {
                            index++;
                            if (++num3 < num)
                            {
                                continue;
                            }
                        }
                        if (num3 < num2)
                        {
                            while (num3-- > 0)
                            {
                                blTree.WriteSymbol(code);
                            }
                        }
                        else if (code != 0)
                        {
                            blTree.WriteSymbol(0x10);
                            this.dh.pending.WriteBits(num3 - 3, 2);
                        }
                        else if (num3 <= 10)
                        {
                            blTree.WriteSymbol(0x11);
                            this.dh.pending.WriteBits(num3 - 3, 3);
                        }
                        else
                        {
                            blTree.WriteSymbol(0x12);
                            this.dh.pending.WriteBits(num3 - 11, 7);
                        }
                        break;
                    }
                }
            }
        }
    }
}

