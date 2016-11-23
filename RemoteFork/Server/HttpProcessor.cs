using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace RemoteFork.Server {
    internal class HttpProcessor {
        public string HttpUrl { get; private set; }
        public string HttpProtocolVersion { get; private set; }
        public string Host { get; private set; }
        public string Range { get; private set; }
        public string ContentLength { get; private set; }

        private Stream inputStream;
        public StreamWriter outputStream;

        private readonly TcpClient client;
        private readonly HttpServer server;

        private const int MAX_POST_SIZE = 10485760;
        private const int BUF_SIZE = 4096;

        public HttpProcessor(TcpClient client, HttpServer server) {
            this.client = client;
            this.server = server;
        }

        public void Process(object state) {
            inputStream = new BufferedStream(client.GetStream());
            outputStream = new StreamWriter(new BufferedStream(client.GetStream()));
            try {
                string httpMethod = ParseRequest();
                ParseHeaders();
                if (httpMethod.Equals("GET")) {
                    HandleGetRequest();
                } else {
                    if (httpMethod.Equals("POST")) {
                        HandlePostRequest();
                    }
                }
                outputStream.Flush();
            } catch (Exception ex) {
                Logger.Error("HttpProcessor->Process: {0}", ex);
                WriteFailure();
            } finally {
                outputStream?.Close();
                inputStream?.Close();

                inputStream = null;
                outputStream = null;

                client?.Close();

                Logger.Debug("HttpProcessor: Socked close");
            }
        }

        #region Parsing request

        private string ParseRequest() {
            string httpMethod;

            string text = StreamReadLine(inputStream);
            string[] array = text.Split(' ');
            if (array.Length == 3) {
                httpMethod = array[0].ToUpper();
                HttpUrl = array[1];
                HttpProtocolVersion = array[2];
                Console.WriteLine("starting: " + text);
            } else {
                throw new Exception("invalid http request line");
            }

            return httpMethod;
        }

        private void ParseHeaders() {
            Range = string.Empty;
            ContentLength = string.Empty;

            string text;
            while ((text = StreamReadLine(inputStream)) != null) {
                if (!string.IsNullOrEmpty(text)) {
                    if (text.Contains(":")) {
                        int separator = text.IndexOf(':');
                        string key = text.Substring(0, separator);
                        string value = text.Remove(0, separator + 1).Trim();

                        switch (key) {
                            case "Host":
                                Host = value;
                                break;
                            case "Range":
                                Range = value.Replace("bytes=", "");
                                break;
                            case "Content-Length":
                                ContentLength = value;
                                break;
                        }
                    } else {
                        throw new Exception("invalid http header line: " + text);
                    }
                } else {
                    break;
                }
            }
        }

        private static string StreamReadLine(Stream inputStream) {
            string text = string.Empty;
            while (true) {
                int data = inputStream.ReadByte();
                if (data != 10) {
                    if (data != 13) {
                        if (data != -1) {
                            text += Convert.ToChar(data).ToString();
                        } else {
                            Thread.Sleep(1);
                        }
                    }
                } else {
                    break;
                }
            }
            return text;
        }

        #endregion Parsing request

        #region Request processing

        private void HandleGetRequest() {
            server.HandleGetRequest(this);
        }

        private void HandlePostRequest() {
            Console.WriteLine("get post data start");
            MemoryStream memoryStream = new MemoryStream();
            if (!string.IsNullOrEmpty(ContentLength)) {
                int lenght = Convert.ToInt32(ContentLength);
                if (lenght <= MAX_POST_SIZE) {
                    byte[] buffer = new byte[BUF_SIZE];
                    int i = lenght;
                    while (i > 0) {
                        Console.WriteLine("starting Read, to_read={0}", i);
                        int count = inputStream.Read(buffer, 0, Math.Min(BUF_SIZE, i));
                        Console.WriteLine("read finished, numread={0}", count);
                        if (count == 0) {
                            if (i != 0) {
                                throw new Exception("client disconnected during post");
                            }
                            break;
                        }
                        i -= count;
                        memoryStream.Write(buffer, 0, count);
                    }
                    memoryStream.Seek(0L, SeekOrigin.Begin);
                } else {
                    throw new Exception(string.Format("POST Content-Length({0}) too big for this simple server", lenght));
                }
            }
            Console.WriteLine("get post data end");
            server.HandlePostRequest(this, new StreamReader(memoryStream));
        }

        #endregion Request processing

        #region Writing result

        public void SetAutoFlush(bool value) {
            outputStream.AutoFlush = value;
        }

        public void WriteLine(string content) {
            if (outputStream.BaseStream.CanWrite) {
                outputStream.WriteLine(content);
            }
        }

        public void WriteLines(params string[] content) {
            if (outputStream.BaseStream.CanWrite) {
                foreach (string s in content) {
                    outputStream.WriteLine(s);
                }
            }
        }

        public void WriteSuccess(string contentType = "text/html") {
            if (outputStream.BaseStream.CanWrite) {
                outputStream.WriteLine("HTTP/1.0 200 OK");
                outputStream.WriteLine("Access-Control-Allow-Origin: *");
                outputStream.WriteLine("Content-Type: text/html; charset=UTF-8");
                outputStream.WriteLine("Connection: close");
                outputStream.WriteLine("");
            }
        }

        public void WriteFailure() {
            try {
                if (outputStream.BaseStream.CanWrite) {
                    outputStream.WriteLine("HTTP/1.0 404 File not found");
                    outputStream.WriteLine("Connection: close");
                    outputStream.WriteLine("");
                }
            } catch (Exception ex) {
                Console.WriteLine(ex);
            }
        }

        public void WriteBaseStream(byte[] buffer, int offset, int count) {
            if (outputStream.BaseStream.CanWrite) {
                outputStream.BaseStream.Write(buffer, offset, count);
                outputStream.Flush();
            }
        }

        #endregion Writing result
    }
}
