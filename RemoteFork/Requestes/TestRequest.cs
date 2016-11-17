using RemoteFork.Forms;

namespace RemoteFork.Requestes {
    internal class TestRequest : BaseRequest {
        public TestRequest(string text) : base(text) {
        }

        public override string Execute() {
            const string result = "<html><h1>ForkPlayer DLNA Work!</h1><br><b>Server by Visual Studio 2015</b></html>";

            if (text.IndexOf('|') > 0) {
                string device = text.Replace("/test?", "");
                if (!Main.Devices.Contains(device)) {
                    Main.Devices.Add(device);
                }
            }

            return result;
        }
    }
}
