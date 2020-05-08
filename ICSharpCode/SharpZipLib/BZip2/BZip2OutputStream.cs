namespace ICSharpCode.SharpZipLib.BZip2
{
    using ICSharpCode.SharpZipLib.Checksums;
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    internal class BZip2OutputStream : Stream
    {
        private const int SETMASK = 0x200000;
        private const int CLEARMASK = -2097153;
        private const int GREATER_ICOST = 15;
        private const int LESSER_ICOST = 0;
        private const int SMALL_THRESH = 20;
        private const int DEPTH_THRESH = 10;
        private const int QSORT_STACK_SIZE = 0x3e8;
        private readonly int[] increments;
        private bool isStreamOwner;
        private int last;
        private int origPtr;
        private int blockSize100k;
        private bool blockRandomised;
        private int bytesOut;
        private int bsBuff;
        private int bsLive;
        private IChecksum mCrc;
        private bool[] inUse;
        private int nInUse;
        private char[] seqToUnseq;
        private char[] unseqToSeq;
        private char[] selector;
        private char[] selectorMtf;
        private byte[] block;
        private int[] quadrant;
        private int[] zptr;
        private short[] szptr;
        private int[] ftab;
        private int nMTF;
        private int[] mtfFreq;
        private int workFactor;
        private int workDone;
        private int workLimit;
        private bool firstAttempt;
        private int nBlocksRandomised;
        private int currentChar;
        private int runLength;
        private uint blockCRC;
        private uint combinedCRC;
        private int allowableBlockSize;
        private Stream baseStream;
        private bool disposed_;

        public BZip2OutputStream(Stream stream) : this(stream, 9)
        {
        }

        public BZip2OutputStream(Stream stream, int blockSize)
        {
            this.increments = new int[] { 1, 4, 13, 40, 0x79, 0x16c, 0x445, 0xcd0, 0x2671, 0x7354, 0x159fd, 0x40df8, 0xc29e9, 0x247dbc };
            this.isStreamOwner = true;
            this.mCrc = new StrangeCRC();
            this.inUse = new bool[0x100];
            this.seqToUnseq = new char[0x100];
            this.unseqToSeq = new char[0x100];
            this.selector = new char[0x4652];
            this.selectorMtf = new char[0x4652];
            this.mtfFreq = new int[0x102];
            this.currentChar = -1;
            this.BsSetStream(stream);
            this.workFactor = 50;
            if (blockSize > 9)
            {
                blockSize = 9;
            }
            if (blockSize < 1)
            {
                blockSize = 1;
            }
            this.blockSize100k = blockSize;
            this.AllocateCompressStructures();
            this.Initialize();
            this.InitBlock();
        }

        private void AllocateCompressStructures()
        {
            int num = 0x186a0 * this.blockSize100k;
            this.block = new byte[(num + 1) + 20];
            this.quadrant = new int[num + 20];
            this.zptr = new int[num];
            this.ftab = new int[0x10001];
            if ((this.block != null) && ((this.quadrant != null) && (this.zptr != null)))
            {
                int[] ftab = this.ftab;
            }
            this.szptr = new short[2 * num];
        }

        private void BsFinishedWithStream()
        {
            while (this.bsLive > 0)
            {
                int num = this.bsBuff >> 0x18;
                this.baseStream.WriteByte((byte) num);
                this.bsBuff = this.bsBuff << 8;
                this.bsLive -= 8;
                this.bytesOut++;
            }
        }

        private void BsPutint(int u)
        {
            this.BsW(8, (u >> 0x18) & 0xff);
            this.BsW(8, (u >> 0x10) & 0xff);
            this.BsW(8, (u >> 8) & 0xff);
            this.BsW(8, u & 0xff);
        }

        private void BsPutIntVS(int numBits, int c)
        {
            this.BsW(numBits, c);
        }

        private void BsPutUChar(int c)
        {
            this.BsW(8, c);
        }

        private void BsSetStream(Stream stream)
        {
            this.baseStream = stream;
            this.bsLive = 0;
            this.bsBuff = 0;
            this.bytesOut = 0;
        }

        private void BsW(int n, int v)
        {
            while (this.bsLive >= 8)
            {
                int num = this.bsBuff >> 0x18;
                this.baseStream.WriteByte((byte) num);
                this.bsBuff = this.bsBuff << 8;
                this.bsLive -= 8;
                this.bytesOut++;
            }
            this.bsBuff |= v << (((0x20 - this.bsLive) - n) & 0x1f);
            this.bsLive += n;
        }

        public override void Close()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                base.Dispose(disposing);
                if (!this.disposed_)
                {
                    this.disposed_ = true;
                    if (this.runLength > 0)
                    {
                        this.WriteRun();
                    }
                    this.currentChar = -1;
                    this.EndBlock();
                    this.EndCompression();
                    this.Flush();
                }
            }
            finally
            {
                if (disposing && this.IsStreamOwner)
                {
                    this.baseStream.Close();
                }
            }
        }

        private void DoReversibleTransformation()
        {
            this.workLimit = this.workFactor * this.last;
            this.workDone = 0;
            this.blockRandomised = false;
            this.firstAttempt = true;
            this.MainSort();
            if ((this.workDone > this.workLimit) && this.firstAttempt)
            {
                this.RandomiseBlock();
                this.workLimit = this.workDone = 0;
                this.blockRandomised = true;
                this.firstAttempt = false;
                this.MainSort();
            }
            this.origPtr = -1;
            int index = 0;
            while (true)
            {
                if (index <= this.last)
                {
                    if (this.zptr[index] != 0)
                    {
                        index++;
                        continue;
                    }
                    this.origPtr = index;
                }
                if (this.origPtr == -1)
                {
                    Panic();
                }
                return;
            }
        }

        private void EndBlock()
        {
            if (this.last >= 0)
            {
                this.blockCRC = (uint) this.mCrc.Value;
                this.combinedCRC = (this.combinedCRC << 1) | (this.combinedCRC >> 0x1f);
                this.combinedCRC ^= this.blockCRC;
                this.DoReversibleTransformation();
                this.BsPutUChar(0x31);
                this.BsPutUChar(0x41);
                this.BsPutUChar(0x59);
                this.BsPutUChar(0x26);
                this.BsPutUChar(0x53);
                this.BsPutUChar(0x59);
                this.BsPutint((int) this.blockCRC);
                if (!this.blockRandomised)
                {
                    this.BsW(1, 0);
                }
                else
                {
                    this.BsW(1, 1);
                    this.nBlocksRandomised++;
                }
                this.MoveToFrontCodeAndSend();
            }
        }

        private void EndCompression()
        {
            this.BsPutUChar(0x17);
            this.BsPutUChar(0x72);
            this.BsPutUChar(0x45);
            this.BsPutUChar(0x38);
            this.BsPutUChar(80);
            this.BsPutUChar(0x90);
            this.BsPutint((int) this.combinedCRC);
            this.BsFinishedWithStream();
        }

        ~BZip2OutputStream()
        {
            this.Dispose(false);
        }

        public override void Flush()
        {
            this.baseStream.Flush();
        }

        private bool FullGtU(int i1, int i2)
        {
            byte num2 = this.block[i1 + 1];
            byte num3 = this.block[i2 + 1];
            if (num2 != num3)
            {
                return (num2 > num3);
            }
            i1++;
            i2++;
            num2 = this.block[i1 + 1];
            num3 = this.block[i2 + 1];
            if (num2 != num3)
            {
                return (num2 > num3);
            }
            i1++;
            i2++;
            num2 = this.block[i1 + 1];
            num3 = this.block[i2 + 1];
            if (num2 != num3)
            {
                return (num2 > num3);
            }
            i1++;
            i2++;
            num2 = this.block[i1 + 1];
            num3 = this.block[i2 + 1];
            if (num2 != num3)
            {
                return (num2 > num3);
            }
            i1++;
            i2++;
            num2 = this.block[i1 + 1];
            num3 = this.block[i2 + 1];
            if (num2 != num3)
            {
                return (num2 > num3);
            }
            i1++;
            i2++;
            num2 = this.block[i1 + 1];
            num3 = this.block[i2 + 1];
            if (num2 != num3)
            {
                return (num2 > num3);
            }
            i1++;
            i2++;
            int num = this.last + 1;
            while (true)
            {
                num2 = this.block[i1 + 1];
                num3 = this.block[i2 + 1];
                if (num2 != num3)
                {
                    return (num2 > num3);
                }
                int num4 = this.quadrant[i1];
                int num5 = this.quadrant[i2];
                if (num4 != num5)
                {
                    return (num4 > num5);
                }
                i1++;
                i2++;
                num2 = this.block[i1 + 1];
                num3 = this.block[i2 + 1];
                if (num2 != num3)
                {
                    return (num2 > num3);
                }
                num4 = this.quadrant[i1];
                num5 = this.quadrant[i2];
                if (num4 != num5)
                {
                    return (num4 > num5);
                }
                i1++;
                i2++;
                num2 = this.block[i1 + 1];
                num3 = this.block[i2 + 1];
                if (num2 != num3)
                {
                    return (num2 > num3);
                }
                num4 = this.quadrant[i1];
                num5 = this.quadrant[i2];
                if (num4 != num5)
                {
                    return (num4 > num5);
                }
                i1++;
                i2++;
                num2 = this.block[i1 + 1];
                num3 = this.block[i2 + 1];
                if (num2 != num3)
                {
                    return (num2 > num3);
                }
                num4 = this.quadrant[i1];
                num5 = this.quadrant[i2];
                if (num4 != num5)
                {
                    return (num4 > num5);
                }
                i1++;
                i2++;
                if (i1 > this.last)
                {
                    i1 -= this.last;
                    i1--;
                }
                if (i2 > this.last)
                {
                    i2 -= this.last;
                    i2--;
                }
                num -= 4;
                this.workDone++;
                if (num < 0)
                {
                    return false;
                }
            }
        }

        private unsafe void GenerateMTFValues()
        {
            int num;
            int num6;
            char[] chArray = new char[0x100];
            this.MakeMaps();
            int index = this.nInUse + 1;
            for (num = 0; num <= index; num++)
            {
                this.mtfFreq[num] = 0;
            }
            int num4 = 0;
            int num3 = 0;
            for (num = 0; num < this.nInUse; num++)
            {
                chArray[num] = (char) num;
            }
            num = 0;
            while (num <= this.last)
            {
                char ch3 = this.unseqToSeq[this.block[this.zptr[num]]];
                int num2 = 0;
                char ch = chArray[num2];
                while (true)
                {
                    if (ch3 == ch)
                    {
                        chArray[0] = ch;
                        if (num2 == 0)
                        {
                            num3++;
                        }
                        else
                        {
                            if (num3 > 0)
                            {
                                num3--;
                                while (true)
                                {
                                    num6 = num3 % 2;
                                    if (num6 == 0)
                                    {
                                        this.szptr[num4] = 0;
                                        num4++;
                                        int* mtfFreq = this.mtfFreq;
                                        mtfFreq[0]++;
                                    }
                                    else if (num6 == 1)
                                    {
                                        this.szptr[num4] = 1;
                                        num4++;
                                        int* numPtr2 = &(this.mtfFreq[1]);
                                        numPtr2[0]++;
                                    }
                                    if (num3 < 2)
                                    {
                                        num3 = 0;
                                        break;
                                    }
                                    num3 = (num3 - 2) / 2;
                                }
                            }
                            this.szptr[num4] = (short) (num2 + 1);
                            num4++;
                            int* numPtr3 = &(this.mtfFreq[num2 + 1]);
                            numPtr3[0]++;
                        }
                        num++;
                        break;
                    }
                    num2++;
                    char ch2 = ch;
                    ch = chArray[num2];
                    chArray[num2] = ch2;
                }
            }
            if (num3 > 0)
            {
                num3--;
                while (true)
                {
                    num6 = num3 % 2;
                    if (num6 == 0)
                    {
                        this.szptr[num4] = 0;
                        num4++;
                        int* mtfFreq = this.mtfFreq;
                        mtfFreq[0]++;
                    }
                    else if (num6 == 1)
                    {
                        this.szptr[num4] = 1;
                        num4++;
                        int* numPtr5 = &(this.mtfFreq[1]);
                        numPtr5[0]++;
                    }
                    if (num3 < 2)
                    {
                        break;
                    }
                    num3 = (num3 - 2) / 2;
                }
            }
            this.szptr[num4] = (short) index;
            num4++;
            int* numPtr6 = &(this.mtfFreq[index]);
            numPtr6[0]++;
            this.nMTF = num4;
        }

        private static void HbAssignCodes(int[] code, char[] length, int minLen, int maxLen, int alphaSize)
        {
            int num = 0;
            int num2 = minLen;
            while (num2 <= maxLen)
            {
                int index = 0;
                while (true)
                {
                    if (index >= alphaSize)
                    {
                        num = num << 1;
                        num2++;
                        break;
                    }
                    if (length[index] == num2)
                    {
                        code[index] = num;
                        num++;
                    }
                    index++;
                }
            }
        }

        private static void HbMakeCodeLengths(char[] len, int[] freq, int alphaSize, int maxLen)
        {
            int[] numArray = new int[260];
            int[] numArray2 = new int[0x204];
            int[] numArray3 = new int[0x204];
            for (int i = 0; i < alphaSize; i++)
            {
                numArray2[i + 1] = ((freq[i] == null) ? 1 : freq[i]) << 8;
            }
            while (true)
            {
                int index = alphaSize;
                int num2 = 0;
                numArray[0] = 0;
                numArray2[0] = 0;
                numArray3[0] = -2;
                int num8 = 1;
                while (true)
                {
                    if (num8 > alphaSize)
                    {
                        if (num2 >= 260)
                        {
                            Panic();
                        }
                        while (true)
                        {
                            if (num2 <= 1)
                            {
                                if (index >= 0x204)
                                {
                                    Panic();
                                }
                                bool flag = false;
                                int num15 = 1;
                                while (true)
                                {
                                    int num5;
                                    if (num15 > alphaSize)
                                    {
                                        if (!flag)
                                        {
                                            return;
                                        }
                                        for (int j = 1; j < alphaSize; j++)
                                        {
                                            num5 = 1 + ((numArray2[j] >> 8) / 2);
                                            numArray2[j] = num5 << 8;
                                        }
                                        break;
                                    }
                                    num5 = 0;
                                    int num6 = num15;
                                    while (true)
                                    {
                                        if (numArray3[num6] < 0)
                                        {
                                            len[num15 - 1] = (char) num5;
                                            if (num5 > maxLen)
                                            {
                                                flag = true;
                                            }
                                            num15++;
                                            break;
                                        }
                                        num6 = numArray3[num6];
                                        num5++;
                                    }
                                }
                                break;
                            }
                            int num3 = numArray[1];
                            numArray[1] = numArray[num2];
                            num2--;
                            int num11 = 1;
                            int num12 = 0;
                            int num13 = numArray[num11];
                            while (true)
                            {
                                num12 = num11 << 1;
                                if (num12 > num2)
                                {
                                    break;
                                }
                                if ((num12 < num2) && (numArray2[numArray[num12 + 1]] < numArray2[numArray[num12]]))
                                {
                                    num12++;
                                }
                                if (numArray2[num13] < numArray2[numArray[num12]])
                                {
                                    break;
                                }
                                numArray[num11] = numArray[num12];
                                num11 = num12;
                            }
                            numArray[num11] = num13;
                            int num4 = numArray[1];
                            numArray[1] = numArray[num2];
                            num2--;
                            num11 = 1;
                            num12 = 0;
                            num13 = numArray[num11];
                            while (true)
                            {
                                num12 = num11 << 1;
                                if (num12 > num2)
                                {
                                    break;
                                }
                                if ((num12 < num2) && (numArray2[numArray[num12 + 1]] < numArray2[numArray[num12]]))
                                {
                                    num12++;
                                }
                                if (numArray2[num13] < numArray2[numArray[num12]])
                                {
                                    break;
                                }
                                numArray[num11] = numArray[num12];
                                num11 = num12;
                            }
                            numArray[num11] = num13;
                            index++;
                            numArray3[num3] = numArray3[num4] = index;
                            numArray2[index] = ((numArray2[num3] & 0xffffff00UL) + (numArray2[num4] & 0xffffff00UL)) | (1 + (((numArray2[num3] & 0xff) > (numArray2[num4] & 0xff)) ? (numArray2[num3] & 0xff) : (numArray2[num4] & 0xff)));
                            numArray3[index] = -1;
                            num2++;
                            numArray[num2] = index;
                            num11 = num2;
                            num13 = numArray[num11];
                            while (true)
                            {
                                if (numArray2[num13] >= numArray2[numArray[num11 >> 1]])
                                {
                                    numArray[num11] = num13;
                                    break;
                                }
                                numArray[num11] = numArray[num11 >> 1];
                                num11 = num11 >> 1;
                            }
                        }
                        break;
                    }
                    numArray3[num8] = -1;
                    num2++;
                    numArray[num2] = num8;
                    int num9 = num2;
                    int num10 = numArray[num9];
                    while (true)
                    {
                        if (numArray2[num10] >= numArray2[numArray[num9 >> 1]])
                        {
                            numArray[num9] = num10;
                            num8++;
                            break;
                        }
                        numArray[num9] = numArray[num9 >> 1];
                        num9 = num9 >> 1;
                    }
                }
            }
        }

        private void InitBlock()
        {
            this.mCrc.Reset();
            this.last = -1;
            for (int i = 0; i < 0x100; i++)
            {
                this.inUse[i] = false;
            }
            this.allowableBlockSize = (0x186a0 * this.blockSize100k) - 20;
        }

        private void Initialize()
        {
            this.bytesOut = 0;
            this.nBlocksRandomised = 0;
            this.BsPutUChar(0x42);
            this.BsPutUChar(90);
            this.BsPutUChar(0x68);
            this.BsPutUChar(0x30 + this.blockSize100k);
            this.combinedCRC = 0;
        }

        private unsafe void MainSort()
        {
            int num;
            int[] numArray = new int[0x100];
            int[] numArray2 = new int[0x100];
            bool[] flagArray = new bool[0x100];
            for (num = 0; num < 20; num++)
            {
                this.block[(this.last + num) + 2] = this.block[(num % (this.last + 1)) + 1];
            }
            num = 0;
            while (num <= (this.last + 20))
            {
                this.quadrant[num] = 0;
                num++;
            }
            this.block[0] = this.block[this.last + 1];
            if (this.last < 0xfa0)
            {
                num = 0;
                while (num <= this.last)
                {
                    this.zptr[num] = num;
                    num++;
                }
                this.firstAttempt = false;
                this.workDone = this.workLimit = 0;
                this.SimpleSort(0, this.last, 0);
            }
            else
            {
                int num2;
                int num6;
                int num7 = 0;
                num = 0;
                while (num <= 0xff)
                {
                    flagArray[num] = false;
                    num++;
                }
                num = 0;
                while (num <= 0x10000)
                {
                    this.ftab[num] = 0;
                    num++;
                }
                int index = this.block[0];
                num = 0;
                while (num <= this.last)
                {
                    num6 = this.block[num + 1];
                    int* numPtr1 = &(this.ftab[(index << 8) + num6]);
                    numPtr1[0]++;
                    index = num6;
                    num++;
                }
                num = 1;
                while (num <= 0x10000)
                {
                    int* numPtr2 = &(this.ftab[num]);
                    numPtr2[0] += this.ftab[num - 1];
                    num++;
                }
                index = this.block[1];
                num = 0;
                while (num < this.last)
                {
                    num6 = this.block[num + 2];
                    num2 = (index << 8) + num6;
                    index = num6;
                    int* numPtr3 = &(this.ftab[num2]);
                    numPtr3[0]--;
                    this.zptr[this.ftab[num2]] = num;
                    num++;
                }
                num2 = (this.block[this.last + 1] << 8) + this.block[1];
                int* numPtr4 = &(this.ftab[num2]);
                numPtr4[0]--;
                this.zptr[this.ftab[num2]] = this.last;
                num = 0;
                while (num <= 0xff)
                {
                    numArray[num] = num;
                    num++;
                }
                int num10 = 1;
                while (true)
                {
                    num10 = (3 * num10) + 1;
                    if (num10 > 0x100)
                    {
                        while (true)
                        {
                            num10 /= 3;
                            num = num10;
                            while (true)
                            {
                                if (num > 0xff)
                                {
                                    if (num10 != 1)
                                    {
                                        break;
                                    }
                                    num = 0;
                                    while (num <= 0xff)
                                    {
                                        int num3 = numArray[num];
                                        num2 = 0;
                                        while (true)
                                        {
                                            if (num2 > 0xff)
                                            {
                                                flagArray[num3] = true;
                                                if (num < 0xff)
                                                {
                                                    int num13 = this.ftab[num3 << 8] & -2097153;
                                                    int num14 = (this.ftab[(num3 + 1) << 8] & -2097153) - num13;
                                                    int num15 = 0;
                                                    while (true)
                                                    {
                                                        if ((num14 >> (num15 & 0x1f)) <= 0xfffe)
                                                        {
                                                            num2 = 0;
                                                            while (true)
                                                            {
                                                                if (num2 >= num14)
                                                                {
                                                                    if (((num14 - 1) >> (num15 & 0x1f)) > 0xffff)
                                                                    {
                                                                        Panic();
                                                                    }
                                                                    break;
                                                                }
                                                                int num16 = this.zptr[num13 + num2];
                                                                int num17 = num2 >> (num15 & 0x1f);
                                                                this.quadrant[num16] = num17;
                                                                if (num16 < 20)
                                                                {
                                                                    this.quadrant[(num16 + this.last) + 1] = num17;
                                                                }
                                                                num2++;
                                                            }
                                                            break;
                                                        }
                                                        num15++;
                                                    }
                                                }
                                                num2 = 0;
                                                while (true)
                                                {
                                                    if (num2 > 0xff)
                                                    {
                                                        num2 = this.ftab[num3 << 8] & -2097153;
                                                        while (true)
                                                        {
                                                            if (num2 >= (this.ftab[(num3 + 1) << 8] & -2097153))
                                                            {
                                                                num2 = 0;
                                                                while (true)
                                                                {
                                                                    if (num2 > 0xff)
                                                                    {
                                                                        num++;
                                                                        break;
                                                                    }
                                                                    int* numPtr7 = &(this.ftab[(num2 << 8) + num3]);
                                                                    numPtr7[0] |= 0x200000;
                                                                    num2++;
                                                                }
                                                                break;
                                                            }
                                                            index = this.block[this.zptr[num2]];
                                                            if (!flagArray[index])
                                                            {
                                                                this.zptr[numArray2[index]] = (this.zptr[num2] == 0) ? this.last : (this.zptr[num2] - 1);
                                                                int* numPtr6 = &(numArray2[index]);
                                                                numPtr6[0]++;
                                                            }
                                                            num2++;
                                                        }
                                                        break;
                                                    }
                                                    numArray2[num2] = this.ftab[(num2 << 8) + num3] & -2097153;
                                                    num2++;
                                                }
                                                break;
                                            }
                                            int num4 = (num3 << 8) + num2;
                                            if ((this.ftab[num4] & 0x200000) != 0x200000)
                                            {
                                                int loSt = this.ftab[num4] & -2097153;
                                                int hiSt = (this.ftab[num4 + 1] & -2097153) - 1;
                                                if (hiSt > loSt)
                                                {
                                                    this.QSort3(loSt, hiSt, 2);
                                                    num7 += (hiSt - loSt) + 1;
                                                    if ((this.workDone > this.workLimit) && this.firstAttempt)
                                                    {
                                                        return;
                                                    }
                                                }
                                                int* numPtr5 = &(this.ftab[num4]);
                                                numPtr5[0] |= 0x200000;
                                            }
                                            num2++;
                                        }
                                    }
                                    return;
                                }
                                int num9 = numArray[num];
                                num2 = num;
                                while (true)
                                {
                                    if ((this.ftab[(numArray[num2 - num10] + 1) << 8] - this.ftab[numArray[num2 - num10] << 8]) > (this.ftab[(num9 + 1) << 8] - this.ftab[num9 << 8]))
                                    {
                                        numArray[num2] = numArray[num2 - num10];
                                        num2 -= num10;
                                        if (num2 > (num10 - 1))
                                        {
                                            continue;
                                        }
                                    }
                                    numArray[num2] = num9;
                                    num++;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void MakeMaps()
        {
            this.nInUse = 0;
            for (int i = 0; i < 0x100; i++)
            {
                if (this.inUse[i])
                {
                    this.seqToUnseq[this.nInUse] = (char) i;
                    this.unseqToSeq[i] = (char) this.nInUse;
                    this.nInUse++;
                }
            }
        }

        private static byte Med3(byte a, byte b, byte c)
        {
            if (a > b)
            {
                byte num1 = a;
                a = b;
                b = num1;
            }
            if (b > c)
            {
                byte num2 = b;
                b = c;
                c = num2;
            }
            if (a > b)
            {
                b = a;
            }
            return b;
        }

        private void MoveToFrontCodeAndSend()
        {
            this.BsPutIntVS(0x18, this.origPtr);
            this.GenerateMTFValues();
            this.SendMTFValues();
        }

        private static void Panic()
        {
            throw new BZip2Exception("BZip2 output stream panic");
        }

        private void QSort3(int loSt, int hiSt, int dSt)
        {
            StackElement[] elementArray = new StackElement[0x3e8];
            int index = 0;
            elementArray[index].ll = loSt;
            elementArray[index].hh = hiSt;
            elementArray[index].dd = dSt;
            index++;
            while (true)
            {
                while (index > 0)
                {
                    if (index >= 0x3e8)
                    {
                        Panic();
                    }
                    index--;
                    int ll = elementArray[index].ll;
                    int hh = elementArray[index].hh;
                    int dd = elementArray[index].dd;
                    if (((hh - ll) < 20) || (dd > 10))
                    {
                        this.SimpleSort(ll, hh, dd);
                        if ((this.workDone > this.workLimit) && this.firstAttempt)
                        {
                            return;
                        }
                    }
                    else
                    {
                        int num3;
                        int num4;
                        int num5 = Med3(this.block[(this.zptr[ll] + dd) + 1], this.block[(this.zptr[hh] + dd) + 1], this.block[(this.zptr[(ll + hh) >> 1] + dd) + 1]);
                        int num = num3 = ll;
                        int num2 = num4 = hh;
                        while (true)
                        {
                            int num6;
                            if (num <= num2)
                            {
                                num6 = this.block[(this.zptr[num] + dd) + 1] - num5;
                                if (num6 == 0)
                                {
                                    this.zptr[num] = this.zptr[num3];
                                    this.zptr[num3] = this.zptr[num];
                                    num3++;
                                    num++;
                                    continue;
                                }
                                if (num6 <= 0)
                                {
                                    num++;
                                    continue;
                                }
                            }
                            while (true)
                            {
                                if (num <= num2)
                                {
                                    num6 = this.block[(this.zptr[num2] + dd) + 1] - num5;
                                    if (num6 == 0)
                                    {
                                        this.zptr[num2] = this.zptr[num4];
                                        this.zptr[num4] = this.zptr[num2];
                                        num4--;
                                        num2--;
                                        continue;
                                    }
                                    if (num6 >= 0)
                                    {
                                        num2--;
                                        continue;
                                    }
                                }
                                if (num > num2)
                                {
                                    if (num4 < num3)
                                    {
                                        elementArray[index].ll = ll;
                                        elementArray[index].hh = hh;
                                        elementArray[index].dd = dd + 1;
                                        index++;
                                    }
                                    else
                                    {
                                        num6 = ((num3 - ll) < (num - num3)) ? (num3 - ll) : (num - num3);
                                        this.Vswap(ll, num - num6, num6);
                                        int n = ((hh - num4) < (num4 - num2)) ? (hh - num4) : (num4 - num2);
                                        this.Vswap(num, (hh - n) + 1, n);
                                        num6 = ((ll + num) - num3) - 1;
                                        n = (hh - (num4 - num2)) + 1;
                                        elementArray[index].ll = ll;
                                        elementArray[index].hh = num6;
                                        elementArray[index].dd = dd;
                                        index++;
                                        elementArray[index].ll = num6 + 1;
                                        elementArray[index].hh = n - 1;
                                        elementArray[index].dd = dd + 1;
                                        index++;
                                        elementArray[index].ll = n;
                                        elementArray[index].hh = hh;
                                        elementArray[index].dd = dd;
                                        index++;
                                    }
                                    continue;
                                }
                                else
                                {
                                    this.zptr[num] = this.zptr[num2];
                                    this.zptr[num2] = this.zptr[num];
                                    num++;
                                    num2--;
                                }
                                break;
                            }
                        }
                    }
                }
                return;
            }
        }

        private unsafe void RandomiseBlock()
        {
            int num;
            int num2 = 0;
            int index = 0;
            for (num = 0; num < 0x100; num++)
            {
                this.inUse[num] = false;
            }
            for (num = 0; num <= this.last; num++)
            {
                if (num2 == 0)
                {
                    num2 = BZip2Constants.RandomNumbers[index];
                    if ((index + 1) == 0x200)
                    {
                        index = 0;
                    }
                }
                num2--;
                byte* numPtr1 = &(this.block[num + 1]);
                numPtr1[0] = (byte) (numPtr1[0] ^ ((num2 == 1) ? ((byte) 1) : ((byte) 0)));
                byte* numPtr2 = &(this.block[num + 1]);
                numPtr2[0] = (byte) (numPtr2[0] & 0xff);
                this.inUse[this.block[num + 1]] = true;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("BZip2OutputStream Read not supported");
        }

        public override int ReadByte()
        {
            throw new NotSupportedException("BZip2OutputStream ReadByte not supported");
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException("BZip2OutputStream Seek not supported");
        }

        private unsafe void SendMTFValues()
        {
            int num2;
            char[][] chArray = new char[6][];
            for (int i = 0; i < 6; i++)
            {
                chArray[i] = new char[0x102];
            }
            int index = 0;
            int alphaSize = this.nInUse + 2;
            int num16 = 0;
            while (num16 < 6)
            {
                int num17 = 0;
                while (true)
                {
                    if (num17 >= alphaSize)
                    {
                        num16++;
                        break;
                    }
                    chArray[num16][num17] = '\x000f';
                    num17++;
                }
            }
            if (this.nMTF <= 0)
            {
                Panic();
            }
            int v = (this.nMTF >= 200) ? ((this.nMTF >= 600) ? ((this.nMTF >= 0x4b0) ? ((this.nMTF >= 0x960) ? 6 : 5) : 4) : 3) : 2;
            int num13 = v;
            int nMTF = this.nMTF;
            int num = 0;
            while (num13 > 0)
            {
                int num18 = nMTF / num13;
                int num19 = 0;
                num2 = num - 1;
                while (true)
                {
                    if ((num19 >= num18) || (num2 >= (alphaSize - 1)))
                    {
                        if ((num2 > num) && ((num13 != v) && ((num13 != 1) && (((v - num13) % 2) == 1))))
                        {
                            num19 -= this.mtfFreq[num2];
                            num2--;
                        }
                        int num20 = 0;
                        while (true)
                        {
                            if (num20 >= alphaSize)
                            {
                                num13--;
                                num = num2 + 1;
                                nMTF -= num19;
                                break;
                            }
                            chArray[num13 - 1][num20] = ((num20 < num) || (num20 > num2)) ? '\x000f' : '\0';
                            num20++;
                        }
                        break;
                    }
                    num2++;
                    num19 += this.mtfFreq[num2];
                }
            }
            int[][] numArray = new int[6][];
            for (int j = 0; j < 6; j++)
            {
                numArray[j] = new int[0x102];
            }
            int[] numArray2 = new int[6];
            short[] numArray3 = new short[6];
            int num6 = 0;
            while (num6 < 4)
            {
                int num22 = 0;
                while (true)
                {
                    if (num22 >= v)
                    {
                        int num23 = 0;
                        while (true)
                        {
                            if (num23 >= v)
                            {
                                index = 0;
                                int num3 = 0;
                                num = 0;
                                while (true)
                                {
                                    if (num >= this.nMTF)
                                    {
                                        int num39 = 0;
                                        while (true)
                                        {
                                            if (num39 >= v)
                                            {
                                                num6++;
                                                break;
                                            }
                                            HbMakeCodeLengths(chArray[num39], numArray[num39], alphaSize, 20);
                                            num39++;
                                        }
                                        break;
                                    }
                                    if (((num + 50) - 1) >= this.nMTF)
                                    {
                                        num2 = this.nMTF - 1;
                                    }
                                    int num25 = 0;
                                    while (true)
                                    {
                                        if (num25 >= v)
                                        {
                                            if (v != 6)
                                            {
                                                int num34 = num;
                                                while (num34 <= num2)
                                                {
                                                    short num35 = this.szptr[num34];
                                                    int num36 = 0;
                                                    while (true)
                                                    {
                                                        if (num36 >= v)
                                                        {
                                                            num34++;
                                                            break;
                                                        }
                                                        short* numPtr1 = &(numArray3[num36]);
                                                        numPtr1[0] = (short) (numPtr1[0] + ((short) chArray[num36][num35]));
                                                        num36++;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                short num27;
                                                short num28;
                                                short num29;
                                                short num30;
                                                short num31;
                                                short num26 = num27 = num28 = num29 = num30 = num31 = 0;
                                                int num32 = num;
                                                while (true)
                                                {
                                                    if (num32 > num2)
                                                    {
                                                        numArray3[0] = num26;
                                                        numArray3[1] = num27;
                                                        numArray3[2] = num28;
                                                        numArray3[3] = num29;
                                                        numArray3[4] = num30;
                                                        numArray3[5] = num31;
                                                        break;
                                                    }
                                                    short num33 = this.szptr[num32];
                                                    num26 = (short) (num26 + ((short) chArray[0][num33]));
                                                    num27 = (short) (num27 + ((short) chArray[1][num33]));
                                                    num28 = (short) (num28 + ((short) chArray[2][num33]));
                                                    num29 = (short) (num29 + ((short) chArray[3][num33]));
                                                    num30 = (short) (num30 + ((short) chArray[4][num33]));
                                                    num31 = (short) (num31 + ((short) chArray[5][num33]));
                                                    num32++;
                                                }
                                            }
                                            int num5 = 0x3b9ac9ff;
                                            int num4 = -1;
                                            int num37 = 0;
                                            while (true)
                                            {
                                                if (num37 >= v)
                                                {
                                                    num3 += num5;
                                                    int* numPtr2 = &(numArray2[num4]);
                                                    numPtr2[0]++;
                                                    this.selector[index] = (char) num4;
                                                    index++;
                                                    int num38 = num;
                                                    while (true)
                                                    {
                                                        if (num38 > num2)
                                                        {
                                                            num = num2 + 1;
                                                            break;
                                                        }
                                                        int* numPtr3 = &(numArray[num4][this.szptr[num38]]);
                                                        numPtr3[0]++;
                                                        num38++;
                                                    }
                                                    break;
                                                }
                                                if (numArray3[num37] < num5)
                                                {
                                                    num5 = numArray3[num37];
                                                    num4 = num37;
                                                }
                                                num37++;
                                            }
                                            break;
                                        }
                                        numArray3[num25] = 0;
                                        num25++;
                                    }
                                }
                                break;
                            }
                            int num24 = 0;
                            while (true)
                            {
                                if (num24 >= alphaSize)
                                {
                                    num23++;
                                    break;
                                }
                                numArray[num23][num24] = 0;
                                num24++;
                            }
                        }
                        break;
                    }
                    numArray2[num22] = 0;
                    num22++;
                }
            }
            numArray = null;
            numArray2 = null;
            numArray3 = null;
            if (v >= 8)
            {
                Panic();
            }
            if ((index >= 0x8000) || (index > 0x4652))
            {
                Panic();
            }
            char[] chArray2 = new char[6];
            for (int k = 0; k < v; k++)
            {
                chArray2[k] = (char) k;
            }
            int num41 = 0;
            while (num41 < index)
            {
                char ch = this.selector[num41];
                int num42 = 0;
                char ch3 = chArray2[num42];
                while (true)
                {
                    if (ch == ch3)
                    {
                        chArray2[0] = ch3;
                        this.selectorMtf[num41] = (char) num42;
                        num41++;
                        break;
                    }
                    num42++;
                    char ch2 = ch3;
                    ch3 = chArray2[num42];
                    chArray2[num42] = ch2;
                }
            }
            int[][] numArray4 = new int[6][];
            for (int m = 0; m < 6; m++)
            {
                numArray4[m] = new int[0x102];
            }
            int num44 = 0;
            while (num44 < v)
            {
                int minLen = 0x20;
                int maxLen = 0;
                int num45 = 0;
                while (true)
                {
                    if (num45 >= alphaSize)
                    {
                        if (maxLen > 20)
                        {
                            Panic();
                        }
                        if (minLen < 1)
                        {
                            Panic();
                        }
                        HbAssignCodes(numArray4[num44], chArray[num44], minLen, maxLen, alphaSize);
                        num44++;
                        break;
                    }
                    if (chArray[num44][num45] > maxLen)
                    {
                        maxLen = chArray[num44][num45];
                    }
                    if (chArray[num44][num45] < minLen)
                    {
                        minLen = chArray[num44][num45];
                    }
                    num45++;
                }
            }
            bool[] flagArray = new bool[0x10];
            int num46 = 0;
            while (num46 < 0x10)
            {
                flagArray[num46] = false;
                int num47 = 0;
                while (true)
                {
                    if (num47 >= 0x10)
                    {
                        num46++;
                        break;
                    }
                    if (this.inUse[(num46 * 0x10) + num47])
                    {
                        flagArray[num46] = true;
                    }
                    num47++;
                }
            }
            for (int n = 0; n < 0x10; n++)
            {
                if (flagArray[n])
                {
                    this.BsW(1, 1);
                }
                else
                {
                    this.BsW(1, 0);
                }
            }
            for (int num49 = 0; num49 < 0x10; num49++)
            {
                if (flagArray[num49])
                {
                    for (int num50 = 0; num50 < 0x10; num50++)
                    {
                        if (this.inUse[(num49 * 0x10) + num50])
                        {
                            this.BsW(1, 1);
                        }
                        else
                        {
                            this.BsW(1, 0);
                        }
                    }
                }
            }
            this.BsW(3, v);
            this.BsW(15, index);
            int num51 = 0;
            while (num51 < index)
            {
                int num52 = 0;
                while (true)
                {
                    if (num52 >= this.selectorMtf[num51])
                    {
                        this.BsW(1, 0);
                        num51++;
                        break;
                    }
                    this.BsW(1, 1);
                    num52++;
                }
            }
            int num53 = 0;
            while (num53 < v)
            {
                int num54 = chArray[num53][0];
                this.BsW(5, num54);
                int num55 = 0;
                while (true)
                {
                    if (num55 >= alphaSize)
                    {
                        num53++;
                        break;
                    }
                    while (true)
                    {
                        if (num54 >= chArray[num53][num55])
                        {
                            while (true)
                            {
                                if (num54 <= chArray[num53][num55])
                                {
                                    this.BsW(1, 0);
                                    num55++;
                                    break;
                                }
                                this.BsW(2, 3);
                                num54--;
                            }
                            break;
                        }
                        this.BsW(2, 2);
                        num54++;
                    }
                }
            }
            int num11 = 0;
            num = 0;
            while (num < this.nMTF)
            {
                num2 = (num + 50) - 1;
                if (num2 >= this.nMTF)
                {
                    num2 = this.nMTF - 1;
                }
                int num56 = num;
                while (true)
                {
                    if (num56 > num2)
                    {
                        num = num2 + 1;
                        num11++;
                        break;
                    }
                    this.BsW(chArray[this.selector[num11]][this.szptr[num56]], numArray4[this.selector[num11]][this.szptr[num56]]);
                    num56++;
                }
            }
            if (num11 != index)
            {
                Panic();
            }
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException("BZip2OutputStream SetLength not supported");
        }

        private void SimpleSort(int lo, int hi, int d)
        {
            int num;
            int num3;
            int num4 = (hi - lo) + 1;
            if (num4 < 2)
            {
                return;
            }
            int index = 0;
            while (true)
            {
                if (this.increments[index] >= num4)
                {
                    index--;
                    break;
                }
                index++;
            }
            goto TR_001D;
        TR_000A:
            index--;
        TR_001D:
            while (true)
            {
                if (index < 0)
                {
                    return;
                }
                num3 = this.increments[index];
                num = lo + num3;
                break;
            }
            while (true)
            {
                while (true)
                {
                    if (num > hi)
                    {
                        goto TR_000A;
                    }
                    else
                    {
                        int num6 = this.zptr[num];
                        int num2 = num;
                        while (true)
                        {
                            if (this.FullGtU(this.zptr[num2 - num3] + d, num6 + d))
                            {
                                this.zptr[num2] = this.zptr[num2 - num3];
                                num2 -= num3;
                                if (num2 > ((lo + num3) - 1))
                                {
                                    continue;
                                }
                            }
                            this.zptr[num2] = num6;
                            num++;
                            if (num <= hi)
                            {
                                num6 = this.zptr[num];
                                num2 = num;
                                while (true)
                                {
                                    if (this.FullGtU(this.zptr[num2 - num3] + d, num6 + d))
                                    {
                                        this.zptr[num2] = this.zptr[num2 - num3];
                                        num2 -= num3;
                                        if (num2 > ((lo + num3) - 1))
                                        {
                                            continue;
                                        }
                                    }
                                    this.zptr[num2] = num6;
                                    num++;
                                    if (num > hi)
                                    {
                                        break;
                                    }
                                    num6 = this.zptr[num];
                                    num2 = num;
                                    while (true)
                                    {
                                        if (this.FullGtU(this.zptr[num2 - num3] + d, num6 + d))
                                        {
                                            this.zptr[num2] = this.zptr[num2 - num3];
                                            num2 -= num3;
                                            if (num2 > ((lo + num3) - 1))
                                            {
                                                continue;
                                            }
                                        }
                                        this.zptr[num2] = num6;
                                        num++;
                                        if ((this.workDone <= this.workLimit) || !this.firstAttempt)
                                        {
                                            break;
                                        }
                                        return;
                                    }
                                }
                            }
                            goto TR_000A;
                        }
                    }
                }
            }
        }

        private void Vswap(int p1, int p2, int n)
        {
            int num = 0;
            while (n > 0)
            {
                num = this.zptr[p1];
                this.zptr[p1] = this.zptr[p2];
                this.zptr[p2] = num;
                p1++;
                p2++;
                n--;
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if ((buffer.Length - offset) < count)
            {
                throw new ArgumentException("Offset/count out of range");
            }
            for (int i = 0; i < count; i++)
            {
                this.WriteByte(buffer[offset + i]);
            }
        }

        public override void WriteByte(byte value)
        {
            int num = (0x100 + value) % 0x100;
            if (this.currentChar == -1)
            {
                this.currentChar = num;
                this.runLength++;
            }
            else if (this.currentChar != num)
            {
                this.WriteRun();
                this.runLength = 1;
                this.currentChar = num;
            }
            else
            {
                this.runLength++;
                if (this.runLength > 0xfe)
                {
                    this.WriteRun();
                    this.currentChar = -1;
                    this.runLength = 0;
                }
            }
        }

        private void WriteRun()
        {
            if (this.last >= this.allowableBlockSize)
            {
                this.EndBlock();
                this.InitBlock();
                this.WriteRun();
            }
            else
            {
                this.inUse[this.currentChar] = true;
                for (int i = 0; i < this.runLength; i++)
                {
                    this.mCrc.Update(this.currentChar);
                }
                switch (this.runLength)
                {
                    case 1:
                        this.last++;
                        this.block[this.last + 1] = (byte) this.currentChar;
                        return;

                    case 2:
                        this.last++;
                        this.block[this.last + 1] = (byte) this.currentChar;
                        this.last++;
                        this.block[this.last + 1] = (byte) this.currentChar;
                        return;

                    case 3:
                        this.last++;
                        this.block[this.last + 1] = (byte) this.currentChar;
                        this.last++;
                        this.block[this.last + 1] = (byte) this.currentChar;
                        this.last++;
                        this.block[this.last + 1] = (byte) this.currentChar;
                        return;
                }
                this.inUse[this.runLength - 4] = true;
                this.last++;
                this.block[this.last + 1] = (byte) this.currentChar;
                this.last++;
                this.block[this.last + 1] = (byte) this.currentChar;
                this.last++;
                this.block[this.last + 1] = (byte) this.currentChar;
                this.last++;
                this.block[this.last + 1] = (byte) this.currentChar;
                this.last++;
                this.block[this.last + 1] = (byte) (this.runLength - 4);
            }
        }

        public bool IsStreamOwner
        {
            get => 
                this.isStreamOwner;
            set => 
                (this.isStreamOwner = value);
        }

        public override bool CanRead =>
            false;

        public override bool CanSeek =>
            false;

        public override bool CanWrite =>
            this.baseStream.CanWrite;

        public override long Length =>
            this.baseStream.Length;

        public override long Position
        {
            get => 
                this.baseStream.Position;
            set
            {
                throw new NotSupportedException("BZip2OutputStream position cannot be set");
            }
        }

        public int BytesWritten =>
            this.bytesOut;

        [StructLayout(LayoutKind.Sequential)]
        private struct StackElement
        {
            public int ll;
            public int hh;
            public int dd;
        }
    }
}

