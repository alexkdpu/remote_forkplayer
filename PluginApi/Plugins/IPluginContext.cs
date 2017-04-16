using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace PluginApi.Plugins
{
    public interface IPluginContext
    {
        NameValueCollection GetRequestParams();

        string CreatePluginUrl(NameValueCollection parameters);

        IHTTPClient GetHttpClient();
        void ConsoleLog(string s);
        ILogger GetLogger();
    }
}
