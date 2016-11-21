using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PluginApi.Plugins;
using RemoteFork.Network;

namespace RemoteFork.Plugins
{
    class PluginContext : IPluginContext
    {
        private readonly IHTTPClient httpClient = new HttpClient();

        private readonly ILogger logger = new Logger();

        private readonly string path;

        public PluginContext(string path = "")
        {
            this.path = path;
        }

        public string GetPath()
        {
            return path;
        }

        public IHTTPClient GetHttpClient()
        {
            return httpClient;
        }

        public ILogger GetLogger()
        {
            return logger;
        }

        internal class HttpClient : IHTTPClient
        {
            public string GetRequest(string link, Dictionary<string, string> header = null)
            {
                return HttpUtility.GetRequest(link, header);
            }

            public string PostRequest(string link, Dictionary<string, string> data, Dictionary<string, string> header = null)
            {
                return HttpUtility.PostRequest(link, data, header);
            }
        }

        internal class Logger : ILogger
        {
            public void Info(string message)
            {
                RemoteFork.Logger.Info(message);
            }

            public void Info(string format, params object[] args)
            {
                RemoteFork.Logger.Info(format, args);
            }

            public void Error(string message)
            {
                RemoteFork.Logger.Error(message);
            }

            public void Error(string format, params object[] args)
            {
                RemoteFork.Logger.Error(format, args);
            }

            public void Debug(string message)
            {
                RemoteFork.Logger.Debug(message);
            }

            public void Debug(string format, params object[] args)
            {
                RemoteFork.Logger.Debug(format, args);
            }
        }

    }
}
