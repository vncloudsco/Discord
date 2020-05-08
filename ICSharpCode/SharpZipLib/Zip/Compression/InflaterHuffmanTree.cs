namespace ICSharpCode.SharpZipLib.Zip.Compression
{
    using ICSharpCode.SharpZipLib;
    using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
    using System;

    internal class InflaterHuffmanTree
    {
        private const int MAX_BITLEN = 15;
        private short[] tree;
        public static InflaterHuffmanTree defLitLenTree;
        public static InflaterHuffmanTree defDistTree;

        static InflaterHuffmanTree()
        {
            try
            {
                byte[] codeLengths = new byte[0x120];
                int num = 0;
                while (true)
                {
                    if (num >= 0x90)
                    {
                        while (true)
                        {
                            if (num >= 0x100)
                            {
                                while (true)
                                {
                                    if (num >= 280)
                                    {
                                        while (true)
                                        {
                                            if (num >= 0x120)
                                            {
                                                defLitLenTree = new InflaterHuffmanTree(codeLengths);
                                                codeLengths = new byte[0x20];
                                                num = 0;
                                                while (true)
                                                {
                                                    if (num >= 0x20)
                                                    {
                                                        defDistTree = new InflaterHuffmanTree(codeLengths);
                                                        break;
                                                    }
                                                    codeLengths[num++] = 5;
                                                }
                                                break;
                                            }
                                            codeLengths[num++] = 8;
                                        }
                                        break;
                                    }
                                    codeLengths[num++] = 7;
                                }
                                break;
                            }
                            codeLengths[num++] = 9;
                        }
                        break;
                    }
                    codeLengths[num++] = 8;
                }
            }
            catch (Exception)
            {
                throw new SharpZipBaseException("InflaterHuffmanTree: static tree length illegal");
            }
        }

        public InflaterHuffmanTree(byte[] codeLengths)
        {
            this.BuildTree(codeLengths);
        }

        private unsafe void BuildTree(byte[] codeLengths)
        {
            int[] numArray = new int[0x10];
            int[] numArray2 = new int[0x10];
            for (int i = 0; i < codeLengths.Length; i++)
            {
                int num5 = codeLengths[i];
                if (num5 > 0)
                {
                    int* numPtr1 = &(numArray[num5]);
                    numPtr1[0]++;
                }
            }
            int toReverse = 0;
            int num2 = 0x200;
            for (int j = 1; j <= 15; j++)
            {
                numArray2[j] = toReverse;
                toReverse += numArray[j] << ((0x10 - j) & 0x1f);
                if (j >= 10)
                {
                    num2 += ((toReverse & 0x1ff80) - (numArray2[j] & 0x1ff80)) >> ((0x10 - j) & 0x1f);
                }
            }
            this.tree = new short[num2];
            int num3 = 0x200;
            int index = 15;
            while (index >= 10)
            {
                int num10 = toReverse & 0x1ff80;
                toReverse -= numArray[index] << ((0x10 - index) & 0x1f);
                int num11 = toReverse & 0x1ff80;
                while (true)
                {
                    if (num11 >= num10)
                    {
                        index--;
                        break;
                    }
                    this.tree[DeflaterHuffman.BitReverse(num11)] = (short) ((-num3 << 4) | index);
                    num3 += 1 << ((index - 9) & 0x1f);
                    num11 += 0x80;
                }
            }
            for (int k = 0; k < codeLengths.Length; k++)
            {
                int num13 = codeLengths[k];
                if (num13 != 0)
                {
                    toReverse = numArray2[num13];
                    int num14 = DeflaterHuffman.BitReverse(toReverse);
                    if (num13 <= 9)
                    {
                        do
                        {
                            this.tree[num14] = (short) ((k << 4) | num13);
                            num14 += 1 << (num13 & 0x1f);
                        }
                        while (num14 < 0x200);
                    }
                    else
                    {
                        int num15 = this.tree[num14 & 0x1ff];
                        int num16 = 1 << ((num15 & 15) & 0x1f);
                        num15 = -(num15 >> 4);
                        do
                        {
                            this.tree[num15 | (num14 >> 9)] = (short) ((k << 4) | num13);
                            num14 += 1 << (num13 & 0x1f);
                        }
                        while (num14 < num16);
                    }
                    numArray2[num13] = toReverse + (1 << ((0x10 - num13) & 0x1f));
                }
            }
        }

        public int GetSymbol(StreamManipulator input)
        {
            int num2;
            int index = input.PeekBits(9);
            if (index < 0)
            {
                int num6 = input.AvailableBits;
                index = input.PeekBits(num6);
                num2 = this.tree[index];
                if ((num2 < 0) || ((num2 & 15) > num6))
                {
                    return -1;
                }
                input.DropBits(num2 & 15);
                return (num2 >> 4);
            }
            num2 = this.tree[index];
            if (num2 >= 0)
            {
                input.DropBits(num2 & 15);
                return (num2 >> 4);
            }
            int num3 = -(num2 >> 4);
            int bitCount = num2 & 15;
            index = input.PeekBits(bitCount);
            if (index >= 0)
            {
                num2 = this.tree[num3 | (index >> 9)];
                input.DropBits(num2 & 15);
                return (num2 >> 4);
            }
            int availableBits = input.AvailableBits;
            num2 = this.tree[num3 | (input.PeekBits(availableBits) >> 9)];
            if ((num2 & 15) > availableBits)
            {
                return -1;
            }
            input.DropBits(num2 & 15);
            return (num2 >> 4);
        }
    }
}

