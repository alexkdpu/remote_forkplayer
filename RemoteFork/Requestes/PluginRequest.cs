using System.Collections.Generic;
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
            } else
            {
                result.AppendLine("<?xml version = \"1.0\" encoding = \"UTF-8\" ?>");

                result.AppendLine("<root>");

                text = text.Replace("/treeview?plugin", "");

                var array = text.Split('\\');
                if (array.Length > 0) {
                    var pluginName = array[0].Split(';')[0];

                    var plugin = PluginManager.Instance.GetPlugin(pluginName);
                    if (plugin != null) {
                        Logger.Debug("PluginRequest->Execute: {0}", plugin.Name);

                        var pluginResponse = plugin.Instance.GetList(new PluginContext(ConvertParams(text)));

                        if (!string.IsNullOrWhiteSpace(pluginResponse.NextPageUrl))
                        {
                            result.AppendLine(
                                string.Format(
                                    @"
                                        <next_page_url>
                                            <![CDATA[{0}treeview?plugin{1};{2}\\.xml]]>
                                        </next_page_url>", 
                                    hostText, 
                                    pluginName, 
                                    pluginResponse.NextPageUrl
                                )
                            );
                        }

                        result.AppendLine("<items>");

                        foreach (var item in pluginResponse.Items)
                        {
                            switch (item.Type) {
                                case ItemType.DIRECTORY:
                                {
                                    string url = string.Format(
                                        "{0}treeview?plugin{1}\\.xml",
                                        hostText,
                                        HttpUtility.UrlEncode(string.Format("{0};{1}", pluginName, item.Link))
                                    );

                                    result.AppendLine(
                                        string.Format(
                                            @"
                                                <channel>
                                                    <title>
                                                        <![CDATA[{0}]]>
                                                    </title>
                                                    <description>
                                                        <![CDATA[{1}]
                                                    </description>
                                                    <logo_30x30>
                                                        {2}
                                                    </logo_30x30>
                                                    <playlist_url>
                                                        <![CDATA[{3}]]>
                                                    </playlist_url>
                                                </channel>",
                                            item.Name,
                                            item.Description,
                                            item.ImageLink,
                                            url
                                        )
                                    );
                                } break;
                                case ItemType.FILE:
                                {
                                    result.AppendLine(
                                        string.Format(
                                            @"
                                                <channel>
                                                    <title>
                                                        <![CDATA[{0}]]>
                                                    </title>
                                                    <description>
                                                        <![CDATA[{1}]]>
                                                    </description>
                                                    <logo_30x30>
                                                        {2}
                                                    </logo_30x30>
                                                    <stream_url>
                                                        <![CDATA[{3}]]>
                                                    </stream_url>
                                                </channel>",
                                            item.Name, 
                                            item.Description, 
                                            item.ImageLink, 
                                            item.Link
                                        )
                                    );
                                } break;
                                case ItemType.SEARCH:
                                {
                                    string url = string.Format(
                                        "{0}treeview?plugin{1}\\.xml",
                                        hostText,
                                        HttpUtility.UrlEncode(string.Format("{0};search_on;{1}", pluginName, item.Link))
                                    );

                                    result.AppendLine(
                                        string.Format(
                                            @"
                                            <channel>
                                                <title>
                                                    <![CDATA[{0}]]>
                                                </title>
                                                <description>
                                                    <![CDATA[{1}]]>
                                                </description>
                                                <logo_30x30>
                                                    {2}
                                                </logo_30x30>
                                                <playlist_url>
                                                    <![CDATA[{3}]]>
                                                </playlist_url>
                                                <search_on>
                                                    search_on
                                                </search_on>
                                            </channel>",
                                            item.Name, 
                                            item.Description, 
                                            item.ImageLink,
                                            url
                                        )
                                    );
                                } break;
                            }
                        }

                        result.AppendLine("</items>");
                    }
                }
                else
                {
                    result.AppendLine("<items>");
                    result.AppendLine("</items>");
                }

                result.AppendLine("</root>");
            }
            return result.ToString();
        }

        private string ConvertParams(string path)
        {
            var splittedPath = path.Split('\\');

            if (splittedPath.Length > 1)
            {
                List<string> result = new List<string>();

                foreach (var s in splittedPath)
                {
                    int andIndex = s.IndexOf("&");

                    if (andIndex >= 0)
                    {
                        result.Add(s.Substring(andIndex).Replace("&", ";"));
                    }
                    else
                    {
                        result.Add(s);
                    }
                }

                return string.Join(";", result);
            }
            else if (splittedPath.Length > 0)
            {
                return splittedPath[0];
            }
            else
            {
                return path;
            }
        }
    }
}
