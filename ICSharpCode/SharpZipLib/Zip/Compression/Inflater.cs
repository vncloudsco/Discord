namespace ICSharpCode.SharpZipLib.Zip.Compression
{
    using ICSharpCode.SharpZipLib;
    using ICSharpCode.SharpZipLib.Checksums;
    using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
    using System;

    internal class Inflater
    {
        private static readonly int[] CPLENS = new int[] { 
            3, 4, 5, 6, 7, 8, 9, 10, 11, 13, 15, 0x11, 0x13, 0x17, 0x1b, 0x1f,
            0x23, 0x2b, 0x33, 0x3b, 0x43, 0x53, 0x63, 0x73, 0x83, 0xa3, 0xc3, 0xe3, 0x102
        };
        private static readonly int[] CPLEXT = new int[] { 
            0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2,
            3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 0
        };
        private static readonly int[] CPDIST = new int[] { 
            1, 2, 3, 4, 5, 7, 9, 13, 0x11, 0x19, 0x21, 0x31, 0x41, 0x61, 0x81, 0xc1,
            0x101, 0x181, 0x201, 0x301, 0x401, 0x601, 0x801, 0xc01, 0x1001, 0x1801, 0x2001, 0x3001, 0x4001, 0x6001
        };
        private static readonly int[] CPDEXT = new int[] { 
            0, 0, 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6,
            7, 7, 8, 8, 9, 9, 10, 10, 11, 11, 12, 12, 13, 13
        };
        private const int DECODE_HEADER = 0;
        private const int DECODE_DICT = 1;
        private const int DECODE_BLOCKS = 2;
        private const int DECODE_STORED_LEN1 = 3;
        private const int DECODE_STORED_LEN2 = 4;
        private const int DECODE_STORED = 5;
        private const int DECODE_DYN_HEADER = 6;
        private const int DECODE_HUFFMAN = 7;
        private const int DECODE_HUFFMAN_LENBITS = 8;
        private const int DECODE_HUFFMAN_DIST = 9;
        private const int DECODE_HUFFMAN_DISTBITS = 10;
        private const int DECODE_CHKSUM = 11;
        private const int FINISHED = 12;
        private int mode;
        private int readAdler;
        private int neededBits;
        private int repLength;
        private int repDist;
        private int uncomprLen;
        private bool isLastBlock;
        private long totalOut;
        private long totalIn;
        private bool noHeader;
        private StreamManipulator input;
        private OutputWindow outputWindow;
        private InflaterDynHeader dynHeader;
        private InflaterHuffmanTree litlenTree;
        private InflaterHuffmanTree distTree;
        private Adler32 adler;

        public Inflater() : this(false)
        {
        }

        public Inflater(bool noHeader)
        {
            this.noHeader = noHeader;
            this.adler = new Adler32();
            this.input = new StreamManipulator();
            this.outputWindow = new OutputWindow();
            this.mode = noHeader ? 2 : 0;
        }

        private bool Decode()
        {
            int num3;
            int num4;
            switch (this.mode)
            {
                case 0:
                    return this.DecodeHeader();

                case 1:
                    return this.DecodeDict();

                case 2:
                {
                    if (this.isLastBlock)
                    {
                        if (this.noHeader)
                        {
                            this.mode = 12;
                            return false;
                        }
                        this.input.SkipToByteBoundary();
                        this.neededBits = 0x20;
                        this.mode = 11;
                        return true;
                    }
                    int num2 = this.input.PeekBits(3);
                    if (num2 < 0)
                    {
                        return false;
                    }
                    this.input.DropBits(3);
                    if ((num2 & 1) != 0)
                    {
                        this.isLastBlock = true;
                    }
                    switch ((num2 >> 1))
                    {
                        case 0:
                            this.input.SkipToByteBoundary();
                            this.mode = 3;
                            break;

                        case 1:
                            this.litlenTree = InflaterHuffmanTree.defLitLenTree;
                            this.distTree = InflaterHuffmanTree.defDistTree;
                            this.mode = 7;
                            break;

                        case 2:
                            this.dynHeader = new InflaterDynHeader();
                            this.mode = 6;
                            break;

                        default:
                            throw new SharpZipBaseException("Unknown block type " + num2);
                    }
                    return true;
                }
                case 3:
                    this.uncomprLen = this.input.PeekBits(0x10);
                    if (this.uncomprLen < 0)
                    {
                        return false;
                    }
                    this.input.DropBits(0x10);
                    this.mode = 4;
                    goto TR_001D;

                case 4:
                    goto TR_001D;

                case 5:
                    goto TR_0018;

                case 6:
                    if (!this.dynHeader.Decode(this.input))
                    {
                        return false;
                    }
                    this.litlenTree = this.dynHeader.BuildLitLenTree();
                    this.distTree = this.dynHeader.BuildDistTree();
                    this.mode = 7;
                    break;

                case 7:
                case 8:
                case 9:
                case 10:
                    break;

                case 11:
                    return this.DecodeChksum();

                case 12:
                    return false;

                default:
                    throw new SharpZipBaseException("Inflater.Decode unknown mode");
            }
            return this.DecodeHuffman();
        TR_0018:
            num4 = this.outputWindow.CopyStored(this.input, this.uncomprLen);
            this.uncomprLen -= num4;
            if (this.uncomprLen != 0)
            {
                return !this.input.IsNeedingInput;
            }
            this.mode = 2;
            return true;
        TR_001D:
            num3 = this.input.PeekBits(0x10);
            if (num3 < 0)
            {
                return false;
            }
            this.input.DropBits(0x10);
            if (num3 != (this.uncomprLen ^ 0xffff))
            {
                throw new SharpZipBaseException("broken uncompressed block");
            }
            this.mode = 5;
            goto TR_0018;
        }

        private bool DecodeChksum()
        {
            while (this.neededBits > 0)
            {
                int num = this.input.PeekBits(8);
                if (num < 0)
                {
                    return false;
                }
                this.input.DropBits(8);
                this.readAdler = (this.readAdler << 8) | num;
                this.neededBits -= 8;
            }
            if (((int) this.adler.Value) == this.readAdler)
            {
                this.mode = 12;
                return false;
            }
            object[] objArray1 = new object[] { "Adler chksum doesn't match: ", (int) this.adler.Value, " vs. ", this.readAdler };
            throw new SharpZipBaseException(string.Concat(objArray1));
        }

        private bool DecodeDict()
        {
            while (this.neededBits > 0)
            {
                int num = this.input.PeekBits(8);
                if (num < 0)
                {
                    return false;
                }
                this.input.DropBits(8);
                this.readAdler = (this.readAdler << 8) | num;
                this.neededBits -= 8;
            }
            return false;
        }

        private bool DecodeHeader()
        {
            int num = this.input.PeekBits(0x10);
            if (num < 0)
            {
                return false;
            }
            this.input.DropBits(0x10);
            num = ((num << 8) | (num >> 8)) & 0xffff;
            if ((num % 0x1f) != 0)
            {
                throw new SharpZipBaseException("Header checksum illegal");
            }
            if ((num & 0xf00) != 0x800)
            {
                throw new SharpZipBaseException("Compression Method unknown");
            }
            if ((num & 0x20) == 0)
            {
                this.mode = 2;
            }
            else
            {
                this.mode = 1;
                this.neededBits = 0x20;
            }
            return true;
        }

        private bool DecodeHuffman()
        {
            int num2;
            int freeSpace = this.outputWindow.GetFreeSpace();
            goto TR_0023;
        TR_000E:
            if (this.neededBits > 0)
            {
                this.mode = 10;
                int num5 = this.input.PeekBits(this.neededBits);
                if (num5 < 0)
                {
                    return false;
                }
                this.input.DropBits(this.neededBits);
                this.repDist += num5;
            }
            this.outputWindow.Repeat(this.repLength, this.repDist);
            freeSpace -= this.repLength;
            this.mode = 7;
            goto TR_0023;
        TR_0011:
            num2 = this.distTree.GetSymbol(this.input);
            if (num2 < 0)
            {
                return false;
            }
            try
            {
                this.repDist = CPDIST[num2];
                this.neededBits = CPDEXT[num2];
            }
            catch (Exception)
            {
                throw new SharpZipBaseException("Illegal rep dist code");
            }
            goto TR_000E;
        TR_0023:
            while (true)
            {
                if (freeSpace < 0x102)
                {
                    return true;
                }
                int mode = this.mode;
                switch (mode)
                {
                    case 7:
                        while (true)
                        {
                            if (((num2 = this.litlenTree.GetSymbol(this.input)) & -256) != 0)
                            {
                                if (num2 >= 0x101)
                                {
                                    try
                                    {
                                        this.repLength = CPLENS[num2 - 0x101];
                                        this.neededBits = CPLEXT[num2 - 0x101];
                                    }
                                    catch (Exception)
                                    {
                                        throw new SharpZipBaseException("Illegal rep length code");
                                    }
                                    break;
                                }
                                if (num2 < 0)
                                {
                                    return false;
                                }
                                this.distTree = null;
                                this.litlenTree = null;
                                this.mode = 2;
                                return true;
                            }
                            this.outputWindow.Write(num2);
                            if (--freeSpace < 0x102)
                            {
                                return true;
                            }
                        }
                        break;

                    case 8:
                        break;

                    case 9:
                        goto TR_0011;

                    case 10:
                        goto TR_000E;

                    default:
                        throw new SharpZipBaseException("Inflater unknown mode");
                }
                if (this.neededBits > 0)
                {
                    this.mode = 8;
                    int num4 = this.input.PeekBits(this.neededBits);
                    if (num4 < 0)
                    {
                        return false;
                    }
                    this.input.DropBits(this.neededBits);
                    this.repLength += num4;
                }
                this.mode = 9;
                break;
            }
            goto TR_0011;
        }

        public int Inflate(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            return this.Inflate(buffer, 0, buffer.Length);
        }

        public int Inflate(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", "count cannot be negative");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", "offset cannot be negative");
            }
            if ((offset + count) > buffer.Length)
            {
                throw new ArgumentException("count exceeds buffer bounds");
            }
            if (count == 0)
            {
                if (!this.IsFinished)
                {
                    this.Decode();
                }
                return 0;
            }
            int num = 0;
            while (true)
            {
                if (this.mode != 11)
                {
                    int num2 = this.outputWindow.CopyOutput(buffer, offset, count);
                    if (num2 > 0)
                    {
                        this.adler.Update(buffer, offset, num2);
                        offset += num2;
                        num += num2;
                        this.totalOut += num2;
                        count -= num2;
                        if (count == 0)
                        {
                            return num;
                        }
                    }
                }
                if (!this.Decode() && ((this.outputWindow.GetAvailable() <= 0) || (this.mode == 11)))
                {
                    return num;
                }
            }
        }

        public void Reset()
        {
            this.mode = this.noHeader ? 2 : 0;
            this.totalIn = 0L;
            this.totalOut = 0L;
            this.input.Reset();
            this.outputWindow.Reset();
            this.dynHeader = null;
            this.litlenTree = null;
            this.distTree = null;
            this.isLastBlock = false;
            this.adler.Reset();
        }

        public void SetDictionary(byte[] buffer)
        {
            this.SetDictionary(buffer, 0, buffer.Length);
        }

        public void SetDictionary(byte[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (!this.IsNeedingDictionary)
            {
                throw new InvalidOperationException("Dictionary is not needed");
            }
            this.adler.Update(buffer, index, count);
            if (((int) this.adler.Value) != this.readAdler)
            {
                throw new SharpZipBaseException("Wrong adler checksum");
            }
            this.adler.Reset();
            this.outputWindow.CopyDict(buffer, index, count);
            this.mode = 2;
        }

        public void SetInput(byte[] buffer)
        {
            this.SetInput(buffer, 0, buffer.Length);
        }

        public void SetInput(byte[] buffer, int index, int count)
        {
            this.input.SetInput(buffer, index, count);
            this.totalIn += count;
        }

        public bool IsNeedingInput =>
            this.input.IsNeedingInput;

        public bool IsNeedingDictionary =>
            ((this.mode == 1) && (this.neededBits == 0));

        public bool IsFinished =>
            ((this.mode == 12) && (this.outputWindow.GetAvailable() == 0));

        public int Adler =>
            (this.IsNeedingDictionary ? this.readAdler : ((int) this.adler.Value));

        public long TotalOut =>
            this.totalOut;

        public long TotalIn =>
            (this.totalIn - this.RemainingInput);

        public int RemainingInput =>
            this.input.AvailableBytes;
    }
}

