namespace ICSharpCode.SharpZipLib.Zip.Compression
{
    using ICSharpCode.SharpZipLib;
    using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
    using System;

    internal class InflaterDynHeader
    {
        private const int LNUM = 0;
        private const int DNUM = 1;
        private const int BLNUM = 2;
        private const int BLLENS = 3;
        private const int LENS = 4;
        private const int REPS = 5;
        private static readonly int[] repMin = new int[] { 3, 3, 11 };
        private static readonly int[] repBits = new int[] { 2, 3, 7 };
        private static readonly int[] BL_ORDER = new int[] { 
            0x10, 0x11, 0x12, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2,
            14, 1, 15
        };
        private byte[] blLens;
        private byte[] litdistLens;
        private InflaterHuffmanTree blTree;
        private int mode;
        private int lnum;
        private int dnum;
        private int blnum;
        private int num;
        private int repSymbol;
        private byte lastLen;
        private int ptr;

        public InflaterHuffmanTree BuildDistTree()
        {
            byte[] destinationArray = new byte[this.dnum];
            Array.Copy(this.litdistLens, this.lnum, destinationArray, 0, this.dnum);
            return new InflaterHuffmanTree(destinationArray);
        }

        public InflaterHuffmanTree BuildLitLenTree()
        {
            byte[] destinationArray = new byte[this.lnum];
            Array.Copy(this.litdistLens, 0, destinationArray, 0, this.lnum);
            return new InflaterHuffmanTree(destinationArray);
        }

        public bool Decode(StreamManipulator input)
        {
            int mode;
            int num5;
        TR_002D:
            while (true)
            {
                mode = this.mode;
                switch (mode)
                {
                    case 0:
                        this.lnum = input.PeekBits(5);
                        if (this.lnum < 0)
                        {
                            return false;
                        }
                        this.lnum += 0x101;
                        input.DropBits(5);
                        this.mode = 1;
                        goto TR_0026;

                    case 1:
                        goto TR_0026;

                    case 2:
                        goto TR_0023;

                    case 3:
                        goto TR_0021;

                    case 4:
                        goto TR_001A;

                    case 5:
                        break;

                    default:
                    {
                        continue;
                    }
                }
                goto TR_000E;
            }
            goto TR_0026;
        TR_000E:
            num5 = repBits[this.repSymbol];
            int num6 = input.PeekBits(num5);
            if (num6 < 0)
            {
                return false;
            }
            input.DropBits(num5);
            num6 += repMin[this.repSymbol];
            if ((this.ptr + num6) > this.num)
            {
                throw new SharpZipBaseException();
            }
            while (true)
            {
                if (num6-- <= 0)
                {
                    if (this.ptr == this.num)
                    {
                        return true;
                    }
                    this.mode = 4;
                    break;
                }
                mode = this.ptr;
                this.ptr = mode + 1;
                this.litdistLens[mode] = this.lastLen;
            }
            goto TR_002D;
        TR_001A:
            while (true)
            {
                int num3;
                if (((num3 = this.blTree.GetSymbol(input)) & -16) != 0)
                {
                    if (num3 < 0)
                    {
                        return false;
                    }
                    if (num3 >= 0x11)
                    {
                        this.lastLen = 0;
                    }
                    else if (this.ptr == 0)
                    {
                        throw new SharpZipBaseException();
                    }
                    this.repSymbol = num3 - 0x10;
                    this.mode = 5;
                    break;
                }
                mode = this.ptr;
                this.ptr = mode + 1;
                this.litdistLens[mode] = this.lastLen = (byte) num3;
                if (this.ptr == this.num)
                {
                    return true;
                }
            }
            goto TR_000E;
        TR_0021:
            while (true)
            {
                if (this.ptr >= this.blnum)
                {
                    this.blTree = new InflaterHuffmanTree(this.blLens);
                    this.blLens = null;
                    this.ptr = 0;
                    this.mode = 4;
                    break;
                }
                int num2 = input.PeekBits(3);
                if (num2 < 0)
                {
                    return false;
                }
                input.DropBits(3);
                this.blLens[BL_ORDER[this.ptr]] = (byte) num2;
                this.ptr++;
            }
            goto TR_001A;
        TR_0023:
            this.blnum = input.PeekBits(4);
            if (this.blnum < 0)
            {
                return false;
            }
            this.blnum += 4;
            input.DropBits(4);
            this.blLens = new byte[0x13];
            this.ptr = 0;
            this.mode = 3;
            goto TR_0021;
        TR_0026:
            this.dnum = input.PeekBits(5);
            if (this.dnum < 0)
            {
                return false;
            }
            this.dnum++;
            input.DropBits(5);
            this.num = this.lnum + this.dnum;
            this.litdistLens = new byte[this.num];
            this.mode = 2;
            goto TR_0023;
        }
    }
}

