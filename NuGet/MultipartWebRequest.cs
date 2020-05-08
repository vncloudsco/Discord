namespace NuGet
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class MultipartWebRequest
    {
        private const string FormDataTemplate = "--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}\r\n";
        private const string FileTemplate = "--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n";
        private readonly Dictionary<string, string> _formData;
        private readonly List<PostFileData> _files;

        public MultipartWebRequest() : this(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase))
        {
        }

        public MultipartWebRequest(Dictionary<string, string> formData)
        {
            this._formData = formData;
            this._files = new List<PostFileData>();
        }

        public void AddFile(Func<Stream> fileFactory, string fieldName, long length, string contentType = "application/octet-stream")
        {
            PostFileData item = new PostFileData();
            item.FileFactory = fileFactory;
            item.FieldName = fieldName;
            item.ContentType = contentType;
            item.ContentLength = length;
            this._files.Add(item);
        }

        public void AddFormData(string key, string value)
        {
            this._formData.Add(key, value);
        }

        private long CalculateContentLength(string boundary)
        {
            long num = 0L;
            foreach (KeyValuePair<string, string> pair in this._formData)
            {
                object[] objArray1 = new object[] { boundary, pair.Key, pair.Value };
                string str2 = string.Format(CultureInfo.InvariantCulture, "--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}\r\n", objArray1);
                byte[] buffer3 = Encoding.UTF8.GetBytes(str2);
                num += buffer3.Length;
            }
            byte[] bytes = Encoding.UTF8.GetBytes(Environment.NewLine);
            foreach (PostFileData data in this._files)
            {
                object[] objArray2 = new object[] { boundary, data.FieldName, data.FieldName, data.ContentType };
                string str3 = string.Format(CultureInfo.InvariantCulture, "--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n", objArray2);
                byte[] buffer4 = Encoding.UTF8.GetBytes(str3);
                num = ((num + buffer4.Length) + data.ContentLength) + bytes.Length;
            }
            object[] args = new object[] { boundary };
            string s = string.Format(CultureInfo.InvariantCulture, "--{0}--", args);
            return (num + Encoding.UTF8.GetBytes(s).Length);
        }

        public void CreateMultipartRequest(WebRequest request)
        {
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x", CultureInfo.InvariantCulture);
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            request.ContentLength = this.CalculateContentLength(boundary);
            using (Stream stream = request.GetRequestStream())
            {
                foreach (KeyValuePair<string, string> pair in this._formData)
                {
                    object[] objArray1 = new object[] { boundary, pair.Key, pair.Value };
                    string str3 = string.Format(CultureInfo.InvariantCulture, "--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}\r\n", objArray1);
                    byte[] buffer3 = Encoding.UTF8.GetBytes(str3);
                    stream.Write(buffer3, 0, buffer3.Length);
                }
                byte[] bytes = Encoding.UTF8.GetBytes(Environment.NewLine);
                foreach (PostFileData data in this._files)
                {
                    object[] objArray2 = new object[] { boundary, data.FieldName, data.FieldName, data.ContentType };
                    string str4 = string.Format(CultureInfo.InvariantCulture, "--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n", objArray2);
                    byte[] buffer4 = Encoding.UTF8.GetBytes(str4);
                    stream.Write(buffer4, 0, buffer4.Length);
                    Stream local1 = data.FileFactory();
                    local1.CopyTo(stream, 0x1000);
                    local1.Close();
                    stream.Write(bytes, 0, bytes.Length);
                }
                object[] args = new object[] { boundary };
                string s = string.Format(CultureInfo.InvariantCulture, "--{0}--", args);
                byte[] buffer = Encoding.UTF8.GetBytes(s);
                stream.Write(buffer, 0, buffer.Length);
            }
        }

        private sealed class PostFileData
        {
            public Func<Stream> FileFactory { get; set; }

            public string ContentType { get; set; }

            public string FieldName { get; set; }

            public long ContentLength { get; set; }
        }
    }
}

