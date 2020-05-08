namespace WpfAnimatedGif.Decoding
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;

    internal class GifFile
    {
        private GifFile()
        {
        }

        private void Read(Stream stream, bool metadataOnly)
        {
            this.Header = GifHeader.ReadHeader(stream);
            if (this.Header.LogicalScreenDescriptor.HasGlobalColorTable)
            {
                this.GlobalColorTable = GifHelpers.ReadColorTable(stream, this.Header.LogicalScreenDescriptor.GlobalColorTableSize);
            }
            this.ReadFrames(stream, metadataOnly);
            GifApplicationExtension ext = this.Extensions.OfType<GifApplicationExtension>().FirstOrDefault<GifApplicationExtension>(new Func<GifApplicationExtension, bool>(GifHelpers.IsNetscapeExtension));
            if (ext != null)
            {
                this.RepeatCount = GifHelpers.GetRepeatCount(ext);
            }
            else
            {
                this.RepeatCount = 1;
            }
        }

        private void ReadFrames(Stream stream, bool metadataOnly)
        {
            List<GifFrame> list = new List<GifFrame>();
            List<GifExtension> controlExtensions = new List<GifExtension>();
            List<GifExtension> list3 = new List<GifExtension>();
            while (true)
            {
                GifBlock block = GifBlock.ReadBlock(stream, controlExtensions, metadataOnly);
                if (block.Kind == GifBlockKind.GraphicRendering)
                {
                    controlExtensions = new List<GifExtension>();
                }
                if (block is GifFrame)
                {
                    list.Add((GifFrame) block);
                }
                else if (!(block is GifExtension))
                {
                    if (block is GifTrailer)
                    {
                        this.Frames = list.AsReadOnly();
                        this.Extensions = list3.AsReadOnly();
                        return;
                    }
                }
                else
                {
                    GifExtension item = (GifExtension) block;
                    switch (item.Kind)
                    {
                        case GifBlockKind.Control:
                            controlExtensions.Add(item);
                            break;

                        case GifBlockKind.SpecialPurpose:
                            list3.Add(item);
                            break;

                        default:
                            break;
                    }
                }
            }
        }

        internal static GifFile ReadGifFile(Stream stream, bool metadataOnly)
        {
            GifFile file = new GifFile();
            file.Read(stream, metadataOnly);
            return file;
        }

        public GifHeader Header { get; private set; }

        public GifColor[] GlobalColorTable { get; set; }

        public IList<GifFrame> Frames { get; set; }

        public IList<GifExtension> Extensions { get; set; }

        public ushort RepeatCount { get; set; }
    }
}

