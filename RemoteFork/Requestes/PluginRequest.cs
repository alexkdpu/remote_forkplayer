using System.Text;
using System.Web;
using RemoteFork.Plugins;
using RemoteFork.Server;

namespace RemoteFork.Requestes {
    internal class PluginRequest : ProcessorRequest {
        public PluginRequest(string text, HttpProcessor processor) : base(text, processor) {
        }

        public override string Execute() {
            string hostText = string.Format("http://{0}/", processor.Host);

            var result = new StringBuilder();

            if (text == "/treeview?plugin") {
                var plugins = PluginManager.Instance.GetPlugins();

                foreach (var plugin in plugins) {
                    string urlText = HttpUtility.UrlEncode(string.Format("{0}\\", plugin.Key));

                    result.AppendLine(string.Format("#EXTINF:-1,{0}\n{1}treeview?plugin{2}.xml", plugin.Value.Name,
                        hostText, urlText));

                    Logger.Debug("PluginRequest->List {0}", plugin.Value.Name);
                }
            } else {
                result.AppendLine("<?xml version = \"1.0\" encoding = \"UTF-8\" ?>");
                result.AppendLine("<items>");

                text = text.Replace("/treeview?plugin", "");

                var array = text.Split('\\');
                if (array.Length > 0) {
                    var arg = array[0].Split(';');
                    var plugin = PluginManager.Instance.GetPlugin(arg[0]);
                    if (plugin != null) {
                        Logger.Debug("PluginRequest->Execute: {0}", plugin.Name);

                        var items = plugin.Instance.GetList(new PluginContext(array[0]));
                        foreach (var item in items) {
                            switch (item.Type) {
                                case ItemType.DIRECTORY:
                                    string urlText =
                                        HttpUtility.UrlEncode(string.Format("{0}{1}{2}", arg[0], ';', item.Link));

                                    result.AppendLine(
                                        string.Format(
                                            "<channel><title><![CDATA[{0}]]></title><description>{1}</description><logo_30x30>{2}</logo_30x30><playlist_url><![CDATA[{3}treeview?plugin{4}\\.xml]]></playlist_url></channel>",
                                            item.Name, item.Description, item.ImageLink, hostText, urlText));
                                    break;
                                case ItemType.FILE:
                                    result.AppendLine(
                                        string.Format(
                                            "<channel><title><![CDATA[{0}]]></title><description>{1}</description><logo_30x30>{2}</logo_30x30><stream_url><![CDATA[{3}]]></stream_url></channel>",
                                            item.Name, item.Description, item.ImageLink, item.Link));
                                    break;
                            }
                        }
                    }
                }
                result.AppendLine("</items>");
            }
            return result.ToString();
        }
    }
}
