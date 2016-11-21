using System;
using System.IO;
using System.Linq;
using System.Net;
using RemoteFork.Forms;
using RemoteFork.Properties;
using RemoteFork.Requestes;

namespace RemoteFork.Server {
    internal class MyHttpServer : HttpServer {
        public MyHttpServer(IPAddress ip, int port) : base(ip, port) {
        }

        public override void HandleGetRequest(HttpProcessor processor) {
            string httpUrl = System.Web.HttpUtility.UrlDecode(processor.HttpUrl);
            //Logger.Info("HandleGetRequest->Url: {0}", httpUrl);
            string result = string.Empty;

            if (!string.IsNullOrEmpty(httpUrl)) {
                if (File.Exists(httpUrl.Substring(1).Split('?').First()) && Settings.Default.Dlna) {
                    if (DlnaConfigurate.CheckAccess(httpUrl.Substring(1).Split('?').First())) {
                        var request = new DlnaFileRequest(httpUrl.Substring(1).Split('?').First(), processor);
                        request.Execute();
                    } else {
                        Logger.Error("HandleGetRequest: {0}", httpUrl);
                        processor.WriteFailure();
                    }
                } else {
                    BaseRequest request = null;

                    if (httpUrl.StartsWith("/treeview?plugin")) {
                        request = new PluginRequest(httpUrl, processor);
                    } else if (httpUrl.StartsWith("/treeview") && Settings.Default.Dlna) {
                        string text = string.Empty;
                        if (httpUrl.Length > 10) {
                            text = httpUrl.Substring(10);
                            if (httpUrl.IndexOf("&", StringComparison.Ordinal) > 0) {
                                text = text.Substring(0, text.IndexOf("&", StringComparison.Ordinal));
                            }
                        }
                        request = new DlnaBrowserRequest(text, processor);
                    } else {
                        if (httpUrl.StartsWith("/parserlink")) {
                            request = new ParseLinkRequest(httpUrl);
                        } else {
                            if (httpUrl.StartsWith("/test")) {
                                request = new TestRequest(httpUrl);
                            }
                        }
                    }
                    if (request != null) {
                        result = request.Execute();
                    }

                    Logger.Debug("HandleGetRequest->Result:\r\n {0}", result);
                    processor.WriteSuccess();
                    processor.WriteLine(result);
                }
            } else {
                Logger.Error("HandleGetRequest->Error: {0}", httpUrl);
                processor.WriteFailure();
            }
        }

        public override void HandlePostRequest(HttpProcessor processor, StreamReader inputData) {
            //Logger.Info("HandlePostRequest->Url: {0}", processor.HttpUrl);
            string arg = inputData.ReadToEnd();
            processor.WriteSuccess();
            processor.WriteLines("<html><body><h1>test server</h1>",
                "<a href=/test>return</a><p>",
                string.Format("postbody: <pre>{0}</pre>", arg));
        }
    }
}
