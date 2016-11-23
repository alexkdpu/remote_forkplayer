using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using PluginApi.Plugins;
using RemoteFork.Plugins;
using RemoteFork.Server;

namespace RemoteFork.Requestes {
    internal class PluginRequest : ProcessorRequest {
        public PluginRequest(string text, HttpProcessor processor) : base(text, processor) {
        }

        public override string Execute() {
            string hostText = string.Format("http://{0}/", processor.Host);

            var response = Response.EmptyResponse;

            if (Regex.IsMatch(text, @"\/treeview\?plugin[\d\w]+")) {
                text = text.Replace("/treeview?plugin", "");

                var array = text.Split('\\');

                if (array.Length > 0) {
                    var pluginName = array[0].Split(';')[0];

                    var plugin = PluginManager.Instance.GetPlugin(pluginName);
                    if (plugin != null) {
                        Logger.Debug("PluginRequest->Execute: {0}", plugin.Name);

                        var pluginResponse = plugin.Instance.GetList(new PluginContext(GetPluginPath(text)));

                        response = new Response();

                        var pluginUrl = $"{hostText}treeview?plugin{HttpUtility.UrlEncode(pluginName)}";

                        if (!string.IsNullOrWhiteSpace(pluginResponse.NextPageUrl)) {
                            response.NextPageUrl = pluginUrl + HttpUtility.UrlEncode($";{pluginResponse.NextPageUrl}\\.xml");
                        }

                        List<Item> items = new List<Item>();

                        foreach (var pluginItem in pluginResponse.Items) {
                            var item = new Item(pluginItem);

                            if (ItemType.FILE != item.Type) {
                                item.Link = pluginUrl + HttpUtility.UrlEncode($";{item.Link}\\.xml");
                            }

                            items.Add(item);
                        }

                        response.Items = items.ToArray();
                    }
                }
            }

            return ResponseSerializer.ToXml(response);
        }

        private string GetPluginPath(string path) {
            var splittedPath = path.Split('\\');

            if (splittedPath.Length > 1) {
                List<string> result = new List<string>();
                
                foreach (var s in splittedPath) {
                    int andIndex = s.IndexOf("&");

                    if (andIndex >= 0) {
                        result.Add(s.Substring(andIndex).Replace("&", ";"));
                    } else {
                        result.Add(s);
                    }
                }

                return string.Join(";", result);
            } else if (splittedPath.Length > 0) {
                return splittedPath[0];
            } else {
                return path;
            }
        }
    }
}
