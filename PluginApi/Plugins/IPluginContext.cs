using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PluginApi.Plugins
{
    public interface IPluginContext
    {
        string GetPath();

        IHTTPClient GetHttpClient();

        ILogger GetLogger();
    }
}
