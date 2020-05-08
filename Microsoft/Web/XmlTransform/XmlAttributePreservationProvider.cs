namespace Microsoft.Web.XmlTransform
{
    using System;
    using System.IO;
    using System.Text;

    internal class XmlAttributePreservationProvider : IDisposable
    {
        private StreamReader streamReader;
        private PositionTrackingTextReader reader;

        public XmlAttributePreservationProvider(string fileName)
        {
            this.streamReader = new StreamReader(File.OpenRead(fileName));
            this.reader = new PositionTrackingTextReader(this.streamReader);
        }

        public void Close()
        {
            this.Dispose();
            GC.SuppressFinalize(this);
        }

        public void Dispose()
        {
            if (this.streamReader != null)
            {
                this.streamReader.Close();
                this.streamReader = null;
            }
            if (this.reader != null)
            {
                this.reader.Dispose();
                this.reader = null;
            }
        }

        ~XmlAttributePreservationProvider()
        {
            this.Dispose();
        }

        public XmlAttributePreservationDict GetDictAtPosition(int lineNumber, int linePosition)
        {
            if (this.reader.ReadToPosition(lineNumber, linePosition))
            {
                StringBuilder builder = new StringBuilder();
                bool flag = false;
                while (true)
                {
                    int num = this.reader.Read();
                    if (num == 0x22)
                    {
                        flag = !flag;
                    }
                    builder.Append((char) num);
                    if ((num <= 0) || ((((ushort) num) == 0x3e) && !flag))
                    {
                        if (num <= 0)
                        {
                            break;
                        }
                        XmlAttributePreservationDict dict = new XmlAttributePreservationDict();
                        dict.ReadPreservationInfo(builder.ToString());
                        return dict;
                    }
                }
            }
            return null;
        }
    }
}

