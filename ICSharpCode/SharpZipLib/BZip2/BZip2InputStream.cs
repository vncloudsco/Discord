namespace ICSharpCode.SharpZipLib.BZip2
{
    using ICSharpCode.SharpZipLib.Checksums;
    using System;
    using System.IO;

    internal class BZip2InputStream : Stream
    {
        private const int START_BLOCK_STATE = 1;
        private const int RAND_PART_A_STATE = 2;
        private const int RAND_PART_B_STATE = 3;
        private const int RAND_PART_C_STATE = 4;
        private const int NO_RAND_PART_A_STATE = 5;
        private const int NO_RAND_PART_B_STATE = 6;
        private const int NO_RAND_PART_C_STATE = 7;
        private int last;
        private int origPtr;
        private int blockSize100k;
        private bool blockRandomised;
        private int bsBuff;
        private int bsLive;
        private IChecksum mCrc = new StrangeCRC();
        private bool[] inUse = new bool[0x100];
        private int nInUse;
        private byte[] seqToUnseq = new byte[0x100];
        private byte[] unseqToSeq = new byte[0x100];
        private byte[] selector = new byte[0x4652];
        private byte[] selectorMtf = new byte[0x4652];
        private int[] tt;
        private byte[] ll8;
        private int[] unzftab = new int[0x100];
        private int[][] limit = new int[6][];
        private int[][] baseArray = new int[6][];
        private int[][] perm = new int[6][];
        private int[] minLens = new int[6];
        private Stream baseStream;
        private bool streamEnd;
        private int currentChar = -1;
        private int currentState = 1;
        private int storedBlockCRC;
        private int storedCombinedCRC;
        private int computedBlockCRC;
        private uint computedCombinedCRC;
        private int count;
        private int chPrev;
        private int ch2;
        private int tPos;
        private int rNToGo;
        private int rTPos;
        private int i2;
        private int j2;
        private byte z;
        private bool isStreamOwner = true;

        public BZip2InputStream(Stream stream)
        {
            for (int i = 0; i < 6; i++)
            {
                this.limit[i] = new int[0x102];
                this.baseArray[i] = new int[0x102];
                this.perm[i] = new int[0x102];
            }
            this.BsSetStream(stream);
            this.Initialize();
            this.InitBlock();
            this.SetupBlock();
        }

        private static void BadBlockHeader()
        {
            throw new BZip2Exception("BZip2 input stream bad block header");
        }

        private static void BlockOverrun()
        {
            throw new BZip2Exception("BZip2 input stream block overrun");
        }

        private int BsGetInt32() => 
            ((((((this.BsR(8) << 8) | this.BsR(8)) << 8) | this.BsR(8)) << 8) | this.BsR(8));

        private int BsGetIntVS(int numBits) => 
            this.BsR(numBits);

        private char BsGetUChar() => 
            ((char) this.BsR(8));

        private int BsR(int n)
        {
            while (this.bsLive < n)
            {
                this.FillBuffer();
            }
            this.bsLive -= n;
            return ((this.bsBuff >> ((this.bsLive - n) & 0x1f)) & ((1 << (n & 0x1f)) - 1));
        }

        private void BsSetStream(Stream stream)
        {
            this.baseStream = stream;
            this.bsLive = 0;
            this.bsBuff = 0;
        }

        public override void Close()
        {
            if (this.IsStreamOwner && (this.baseStream != null))
            {
                this.baseStream.Close();
            }
        }

        private void Complete()
        {
            this.storedCombinedCRC = this.BsGetInt32();
            if (this.storedCombinedCRC != this.computedCombinedCRC)
            {
                CrcError();
            }
            this.streamEnd = true;
        }

        private static void CompressedStreamEOF()
        {
            throw new EndOfStreamException("BZip2 input stream end of compressed stream");
        }

        private static void CrcError()
        {
            throw new BZip2Exception("BZip2 input stream crc error");
        }

        private void EndBlock()
        {
            this.computedBlockCRC = (int) this.mCrc.Value;
            if (this.storedBlockCRC != this.computedBlockCRC)
            {
                CrcError();
            }
            this.computedCombinedCRC = ((this.computedCombinedCRC << 1) & uint.MaxValue) | (this.computedCombinedCRC >> 0x1f);
            this.computedCombinedCRC ^= (uint) this.computedBlockCRC;
        }

        private void FillBuffer()
        {
            int num = 0;
            try
            {
                num = this.baseStream.ReadByte();
            }
            catch (Exception)
            {
                CompressedStreamEOF();
            }
            if (num == -1)
            {
                CompressedStreamEOF();
            }
            this.bsBuff = (this.bsBuff << 8) | (num & 0xff);
            this.bsLive += 8;
        }

        public override void Flush()
        {
            if (this.baseStream != null)
            {
                this.baseStream.Flush();
            }
        }

        private unsafe void GetAndMoveToFrontDecode()
        {
            int num;
            int num6;
            int num7;
            int num8;
            int num9;
            byte[] buffer = new byte[0x100];
            int num2 = 0x186a0 * this.blockSize100k;
            this.origPtr = this.BsGetIntVS(0x18);
            this.RecvDecodingTables();
            int num3 = this.nInUse + 1;
            int index = -1;
            int num5 = 0;
            int num10 = 0;
            while (true)
            {
                if (num10 > 0xff)
                {
                    int num11 = 0;
                    while (true)
                    {
                        if (num11 > 0xff)
                        {
                            this.last = -1;
                            if (num5 == 0)
                            {
                                index++;
                                num5 = 50;
                            }
                            num5--;
                            num6 = this.selector[index];
                            num7 = this.minLens[num6];
                            num8 = this.BsR(num7);
                            while (true)
                            {
                                if (num8 <= this.limit[num6][num7])
                                {
                                    if (((num8 - this.baseArray[num6][num7]) < 0) || ((num8 - this.baseArray[num6][num7]) >= 0x102))
                                    {
                                        throw new BZip2Exception("Bzip data error");
                                    }
                                    num = this.perm[num6][num8 - this.baseArray[num6][num7]];
                                    break;
                                }
                                if (num7 > 20)
                                {
                                    throw new BZip2Exception("Bzip data error");
                                }
                                num7++;
                                while (true)
                                {
                                    if (this.bsLive >= 1)
                                    {
                                        num9 = (this.bsBuff >> ((this.bsLive - 1) & 0x1f)) & 1;
                                        this.bsLive--;
                                        num8 = (num8 << 1) | num9;
                                        break;
                                    }
                                    this.FillBuffer();
                                }
                            }
                            break;
                        }
                        buffer[num11] = (byte) num11;
                        num11++;
                    }
                    break;
                }
                this.unzftab[num10] = 0;
                num10++;
            }
            while (true)
            {
                int num12;
                int num13;
                while (true)
                {
                    if (num == num3)
                    {
                        return;
                    }
                    if ((num == 0) || (num == 1))
                    {
                        num12 = -1;
                        num13 = 1;
                        break;
                    }
                    this.last++;
                    if (this.last >= num2)
                    {
                        BlockOverrun();
                    }
                    byte num15 = buffer[num - 1];
                    int* numPtr2 = &(this.unzftab[this.seqToUnseq[num15]]);
                    numPtr2[0]++;
                    this.ll8[this.last] = this.seqToUnseq[num15];
                    int num16 = num - 1;
                    while (true)
                    {
                        if (num16 <= 0)
                        {
                            buffer[0] = num15;
                            if (num5 == 0)
                            {
                                index++;
                                num5 = 50;
                            }
                            num5--;
                            num6 = this.selector[index];
                            num7 = this.minLens[num6];
                            num8 = this.BsR(num7);
                            while (true)
                            {
                                if (num8 <= this.limit[num6][num7])
                                {
                                    num = this.perm[num6][num8 - this.baseArray[num6][num7]];
                                    break;
                                }
                                num7++;
                                while (true)
                                {
                                    if (this.bsLive >= 1)
                                    {
                                        num9 = (this.bsBuff >> ((this.bsLive - 1) & 0x1f)) & 1;
                                        this.bsLive--;
                                        num8 = (num8 << 1) | num9;
                                        break;
                                    }
                                    this.FillBuffer();
                                }
                            }
                            break;
                        }
                        buffer[num16] = buffer[num16 - 1];
                        num16--;
                    }
                }
                while (true)
                {
                    if (num == 0)
                    {
                        num12 += 1 * num13;
                    }
                    else if (num == 1)
                    {
                        num12 += 2 * num13;
                    }
                    num13 = num13 << 1;
                    if (num5 == 0)
                    {
                        index++;
                        num5 = 50;
                    }
                    num5--;
                    num6 = this.selector[index];
                    num7 = this.minLens[num6];
                    num8 = this.BsR(num7);
                    while (true)
                    {
                        if (num8 > this.limit[num6][num7])
                        {
                            num7++;
                            while (true)
                            {
                                if (this.bsLive >= 1)
                                {
                                    num9 = (this.bsBuff >> ((this.bsLive - 1) & 0x1f)) & 1;
                                    this.bsLive--;
                                    num8 = (num8 << 1) | num9;
                                    break;
                                }
                                this.FillBuffer();
                            }
                            continue;
                        }
                        num = this.perm[num6][num8 - this.baseArray[num6][num7]];
                        if ((num == 0) || (num == 1))
                        {
                            continue;
                        }
                        else
                        {
                            num12++;
                            byte num14 = this.seqToUnseq[buffer[0]];
                            int* numPtr1 = &(this.unzftab[num14]);
                            numPtr1[0] += num12;
                            while (true)
                            {
                                if (num12 <= 0)
                                {
                                    if (this.last >= num2)
                                    {
                                        BlockOverrun();
                                    }
                                    break;
                                }
                                this.last++;
                                this.ll8[this.last] = num14;
                                num12--;
                            }
                        }
                        break;
                    }
                    break;
                }
            }
        }

        private static unsafe void HbCreateDecodeTables(int[] limit, int[] baseArray, int[] perm, char[] length, int minLen, int maxLen, int alphaSize)
        {
            int index = 0;
            int num3 = minLen;
            while (num3 <= maxLen)
            {
                int num4 = 0;
                while (true)
                {
                    if (num4 >= alphaSize)
                    {
                        num3++;
                        break;
                    }
                    if (length[num4] == num3)
                    {
                        perm[index] = num4;
                        index++;
                    }
                    num4++;
                }
            }
            for (int i = 0; i < 0x17; i++)
            {
                baseArray[i] = 0;
            }
            for (int j = 0; j < alphaSize; j++)
            {
                int* numPtr1 = &(baseArray[length[j] + '\x0001']);
                numPtr1[0]++;
            }
            for (int k = 1; k < 0x17; k++)
            {
                int* numPtr2 = &(baseArray[k]);
                numPtr2[0] += baseArray[k - 1];
            }
            for (int m = 0; m < 0x17; m++)
            {
                limit[m] = 0;
            }
            int num2 = 0;
            for (int n = minLen; n <= maxLen; n++)
            {
                num2 += baseArray[n + 1] - baseArray[n];
                limit[n] = num2 - 1;
                num2 = num2 << 1;
            }
            for (int num10 = minLen + 1; num10 <= maxLen; num10++)
            {
                baseArray[num10] = ((limit[num10 - 1] + 1) << 1) - baseArray[num10];
            }
        }

        private void InitBlock()
        {
            char ch = this.BsGetUChar();
            char ch2 = this.BsGetUChar();
            char ch3 = this.BsGetUChar();
            char ch4 = this.BsGetUChar();
            char ch5 = this.BsGetUChar();
            char ch6 = this.BsGetUChar();
            if ((ch == '\x0017') && ((ch2 == 'r') && ((ch3 == 'E') && ((ch4 == '8') && ((ch5 == 'P') && (ch6 == '\x0090'))))))
            {
                this.Complete();
            }
            else if ((ch != '1') || ((ch2 != 'A') || ((ch3 != 'Y') || ((ch4 != '&') || ((ch5 != 'S') || (ch6 != 'Y'))))))
            {
                BadBlockHeader();
                this.streamEnd = true;
            }
            else
            {
                this.storedBlockCRC = this.BsGetInt32();
                this.blockRandomised = this.BsR(1) == 1;
                this.GetAndMoveToFrontDecode();
                this.mCrc.Reset();
                this.currentState = 1;
            }
        }

        private void Initialize()
        {
            char ch = this.BsGetUChar();
            char ch2 = this.BsGetUChar();
            char ch3 = this.BsGetUChar();
            if ((this.BsGetUChar() != 'B') || ((ch != 'Z') || ((ch2 != 'h') || ((ch3 < '1') || (ch3 > '9')))))
            {
                this.streamEnd = true;
            }
            else
            {
                this.SetDecompressStructureSizes(ch3 - '0');
                this.computedCombinedCRC = 0;
            }
        }

        private void MakeMaps()
        {
            this.nInUse = 0;
            for (int i = 0; i < 0x100; i++)
            {
                if (this.inUse[i])
                {
                    this.seqToUnseq[this.nInUse] = (byte) i;
                    this.unseqToSeq[i] = (byte) this.nInUse;
                    this.nInUse++;
                }
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            for (int i = 0; i < count; i++)
            {
                int num2 = this.ReadByte();
                if (num2 == -1)
                {
                    return i;
                }
                buffer[offset + i] = (byte) num2;
            }
            return count;
        }

        public override int ReadByte()
        {
            if (this.streamEnd)
            {
                return -1;
            }
            int currentChar = this.currentChar;
            switch (this.currentState)
            {
                case 3:
                    this.SetupRandPartB();
                    break;

                case 4:
                    this.SetupRandPartC();
                    break;

                case 6:
                    this.SetupNoRandPartB();
                    break;

                case 7:
                    this.SetupNoRandPartC();
                    break;

                default:
                    break;
            }
            return currentChar;
        }

        private void RecvDecodingTables()
        {
            char[][] chArray = new char[6][];
            for (int i = 0; i < 6; i++)
            {
                chArray[i] = new char[0x102];
            }
            bool[] flagArray = new bool[0x10];
            for (int j = 0; j < 0x10; j++)
            {
                flagArray[j] = this.BsR(1) == 1;
            }
            for (int k = 0; k < 0x10; k++)
            {
                if (flagArray[k])
                {
                    for (int n = 0; n < 0x10; n++)
                    {
                        this.inUse[(k * 0x10) + n] = this.BsR(1) == 1;
                    }
                }
                else
                {
                    for (int n = 0; n < 0x10; n++)
                    {
                        this.inUse[(k * 0x10) + n] = false;
                    }
                }
            }
            this.MakeMaps();
            int alphaSize = this.nInUse + 2;
            int num2 = this.BsR(3);
            int num3 = this.BsR(15);
            int index = 0;
            while (index < num3)
            {
                int num10 = 0;
                while (true)
                {
                    if (this.BsR(1) != 1)
                    {
                        this.selectorMtf[index] = (byte) num10;
                        index++;
                        break;
                    }
                    num10++;
                }
            }
            byte[] buffer = new byte[6];
            for (int m = 0; m < num2; m++)
            {
                buffer[m] = (byte) m;
            }
            int num12 = 0;
            while (num12 < num3)
            {
                int num13 = this.selectorMtf[num12];
                byte num14 = buffer[num13];
                while (true)
                {
                    if (num13 <= 0)
                    {
                        buffer[0] = num14;
                        this.selector[num12] = num14;
                        num12++;
                        break;
                    }
                    buffer[num13] = buffer[num13 - 1];
                    num13--;
                }
            }
            int num15 = 0;
            while (num15 < num2)
            {
                int num16 = this.BsR(5);
                int num17 = 0;
                while (true)
                {
                    if (num17 >= alphaSize)
                    {
                        num15++;
                        break;
                    }
                    while (true)
                    {
                        if (this.BsR(1) != 1)
                        {
                            chArray[num15][num17] = (char) num16;
                            num17++;
                            break;
                        }
                        num16 = (this.BsR(1) != 0) ? (num16 - 1) : (num16 + 1);
                    }
                }
            }
            int num18 = 0;
            while (num18 < num2)
            {
                int minLen = 0x20;
                int maxLen = 0;
                int num21 = 0;
                while (true)
                {
                    if (num21 >= alphaSize)
                    {
                        HbCreateDecodeTables(this.limit[num18], this.baseArray[num18], this.perm[num18], chArray[num18], minLen, maxLen, alphaSize);
                        this.minLens[num18] = minLen;
                        num18++;
                        break;
                    }
                    maxLen = Math.Max(maxLen, chArray[num18][num21]);
                    minLen = Math.Min(minLen, chArray[num18][num21]);
                    num21++;
                }
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException("BZip2InputStream Seek not supported");
        }

        private void SetDecompressStructureSizes(int newSize100k)
        {
            if ((0 > newSize100k) || ((newSize100k > 9) || ((0 > this.blockSize100k) || (this.blockSize100k > 9))))
            {
                throw new BZip2Exception("Invalid block size");
            }
            this.blockSize100k = newSize100k;
            if (newSize100k != 0)
            {
                int num = 0x186a0 * newSize100k;
                this.ll8 = new byte[num];
                this.tt = new int[num];
            }
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException("BZip2InputStream SetLength not supported");
        }

        private unsafe void SetupBlock()
        {
            int[] destinationArray = new int[] { 0 };
            Array.Copy(this.unzftab, 0, destinationArray, 1, 0x100);
            for (int i = 1; i <= 0x100; i++)
            {
                int* numPtr1 = &(destinationArray[i]);
                numPtr1[0] += destinationArray[i - 1];
            }
            for (int j = 0; j <= this.last; j++)
            {
                byte index = this.ll8[j];
                this.tt[destinationArray[index]] = j;
                int* numPtr2 = &(destinationArray[index]);
                numPtr2[0]++;
            }
            destinationArray = null;
            this.tPos = this.tt[this.origPtr];
            this.count = 0;
            this.i2 = 0;
            this.ch2 = 0x100;
            if (!this.blockRandomised)
            {
                this.SetupNoRandPartA();
            }
            else
            {
                this.rNToGo = 0;
                this.rTPos = 0;
                this.SetupRandPartA();
            }
        }

        private void SetupNoRandPartA()
        {
            if (this.i2 > this.last)
            {
                this.EndBlock();
                this.InitBlock();
                this.SetupBlock();
            }
            else
            {
                this.chPrev = this.ch2;
                this.ch2 = this.ll8[this.tPos];
                this.tPos = this.tt[this.tPos];
                this.i2++;
                this.currentChar = this.ch2;
                this.currentState = 6;
                this.mCrc.Update(this.ch2);
            }
        }

        private void SetupNoRandPartB()
        {
            if (this.ch2 != this.chPrev)
            {
                this.currentState = 5;
                this.count = 1;
                this.SetupNoRandPartA();
            }
            else
            {
                this.count++;
                if (this.count < 4)
                {
                    this.currentState = 5;
                    this.SetupNoRandPartA();
                }
                else
                {
                    this.z = this.ll8[this.tPos];
                    this.tPos = this.tt[this.tPos];
                    this.currentState = 7;
                    this.j2 = 0;
                    this.SetupNoRandPartC();
                }
            }
        }

        private void SetupNoRandPartC()
        {
            if (this.j2 < this.z)
            {
                this.currentChar = this.ch2;
                this.mCrc.Update(this.ch2);
                this.j2++;
            }
            else
            {
                this.currentState = 5;
                this.i2++;
                this.count = 0;
                this.SetupNoRandPartA();
            }
        }

        private void SetupRandPartA()
        {
            if (this.i2 > this.last)
            {
                this.EndBlock();
                this.InitBlock();
                this.SetupBlock();
            }
            else
            {
                this.chPrev = this.ch2;
                this.ch2 = this.ll8[this.tPos];
                this.tPos = this.tt[this.tPos];
                if (this.rNToGo == 0)
                {
                    this.rNToGo = BZip2Constants.RandomNumbers[this.rTPos];
                    this.rTPos++;
                    if (this.rTPos == 0x200)
                    {
                        this.rTPos = 0;
                    }
                }
                this.rNToGo--;
                this.ch2 ^= (this.rNToGo == 1) ? 1 : 0;
                this.i2++;
                this.currentChar = this.ch2;
                this.currentState = 3;
                this.mCrc.Update(this.ch2);
            }
        }

        private void SetupRandPartB()
        {
            if (this.ch2 != this.chPrev)
            {
                this.currentState = 2;
                this.count = 1;
                this.SetupRandPartA();
            }
            else
            {
                this.count++;
                if (this.count < 4)
                {
                    this.currentState = 2;
                    this.SetupRandPartA();
                }
                else
                {
                    this.z = this.ll8[this.tPos];
                    this.tPos = this.tt[this.tPos];
                    if (this.rNToGo == 0)
                    {
                        this.rNToGo = BZip2Constants.RandomNumbers[this.rTPos];
                        this.rTPos++;
                        if (this.rTPos == 0x200)
                        {
                            this.rTPos = 0;
                        }
                    }
                    this.rNToGo--;
                    this.z = (byte) (this.z ^ ((this.rNToGo == 1) ? ((byte) 1) : ((byte) 0)));
                    this.j2 = 0;
                    this.currentState = 4;
                    this.SetupRandPartC();
                }
            }
        }

        private void SetupRandPartC()
        {
            if (this.j2 < this.z)
            {
                this.currentChar = this.ch2;
                this.mCrc.Update(this.ch2);
                this.j2++;
            }
            else
            {
                this.currentState = 2;
                this.i2++;
                this.count = 0;
                this.SetupRandPartA();
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("BZip2InputStream Write not supported");
        }

        public override void WriteByte(byte value)
        {
            throw new NotSupportedException("BZip2InputStream WriteByte not supported");
        }

        public bool IsStreamOwner
        {
            get => 
                this.isStreamOwner;
            set => 
                (this.isStreamOwner = value);
        }

        public override bool CanRead =>
            this.baseStream.CanRead;

        public override bool CanSeek =>
            this.baseStream.CanSeek;

        public override bool CanWrite =>
            false;

        public override long Length =>
            this.baseStream.Length;

        public override long Position
        {
            get => 
                this.baseStream.Position;
            set
            {
                throw new NotSupportedException("BZip2InputStream position cannot be set");
            }
        }
    }
}

