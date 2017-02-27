using System.Reflection;
using System.Web;
using RemoteFork.Forms;
using Unosquare.Net;

namespace RemoteFork.Requestes {
    internal class TestRequestHandler : BaseRequestHandler {
        internal static readonly string UrlPath = "/test";

        public override void Handle(HttpListenerRequest request, HttpListenerResponse response) {
            var rawUrl = HttpUtility.UrlDecode(request.RawUrl);

            if (rawUrl != null && rawUrl.IndexOf('|') > 0) {
                Properties.Settings.Default.proxy = false;
                var device = rawUrl.Replace("/test?", "");

                if (!Main.Devices.Contains(device)) {
                    Main.Devices.Add(device);
                }
            }

            WriteResponse(response, $"<html><h1>ForkPlayer DLNA Work!</h1><br><b>RemoteFork Server. v. {Assembly.GetExecutingAssembly().GetName().Version}</b></html>");
        }
    }
}