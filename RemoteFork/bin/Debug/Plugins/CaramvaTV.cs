using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using PluginApi.Plugins;

namespace RemoteFork.Plugins {
    [PluginAttribute(Id = "carambatv", Version = "0.1", Author = "fd_crash", Name = "CarambaTV",
        Description =
            "информационно-развлекательный сайт и интернет-телеканал, консолидирующий популярные шоу русскоязычного Интернета.",
        ImageLink = "https://pbs.twimg.com/profile_images/2155589034/caramba_twitter_logo_400x400.png")]
    public class CarambaTv : IPlugin {
        private static readonly Dictionary<string, Item> showsItems = new Dictionary<string, Item>();

        private const char SEPARATOR = ';';
        private const string PLUGIN_PATH = "pluginPath";
        private const int PAGE_SIZE = 50;
        private const string SITE_URL = "http://carambatv.ru/{0}/";
        private const string SITE_URL_VIDEO = "http://video1.carambatv.ru/v/{0}/{1}";
        private const string SITE_API_URL = "https://carambatv.ru/api/posts/unwatched/init";
        private const string IMAGE_URL = "http://jurnalu.ru/img/issues/{0}_cover.jpg";
        private const string NEXT_PAGE_IMAGE_URL = "http://www.rasputins.ca/images/right.png";

        // Item представляет собой класс, содержащащий следующие поля:
        //// Name - навзание
        //// Link - ссылка (если директория, то ссылка передается на обработку плагину, если файл, то ссылка открывается в проигрывателе)
        //// ImageLink - иконка
        //// Description - описание, поддерживает HTML формат
        //// Type - тип сущности: ItemType.FILE или ItemType.DIRECTORY (по умолчанию)

        // Главный метод для обработки запросов
        public Playlist GetList(IPluginContext context) {
            var path = context.GetRequestParams().Get(PLUGIN_PATH);

            path = path == null ? "plugin" : "plugin;" + path;

            // SEPARATOR служит для разделения команд при парсинге (char)
            var arg = path.Split(SEPARATOR);

            var items = new List<Item>();

            switch (arg.Length) {
                case 0:
                    break;
                case 1:
                    items.AddRange(GetRootListRequest());
                    break;
                default:
                    string type = arg[1];
                    switch (arg[1]) {
                        case "main": {
                            items.AddRange(arg.Length > 3
                                ? GetFilteringListRequest(context, type, arg[2], arg[3])
                                : arg.Length > 2
                                    ? GetFilteringListRequest(context, type, filter: arg[2])
                                    : GetFilteringListRequest(context, type));
                        }
                            break;
                        case "top":
                            items.AddRange(GetTopItemListRequest(context, type));
                            break;
                        case "channel":
                            items.AddRange(arg.Length > 3
                                ? GetItemListRequest(context, type, arg[2], arg[3])
                                : GetItemListRequest(context, type, arg[2]));
                            break;
                        case "channels":
                            items.AddRange(arg.Length > 2
                                ? GetContainerListRequest(context, type, arg[2])
                                : GetContainerListRequest(context, type));
                            break;
                        case "item":
                            items.AddRange(GetItemRequest(context, arg[2]));
                            break;
                    }
                    break;
            }

            var playlist = new Playlist();

            foreach (var item in items) {
                if (ItemType.DIRECTORY == item.Type) {
                    var pluginParams = new NameValueCollection();

                    pluginParams[PLUGIN_PATH] = item.Link;

                    item.Link = context.CreatePluginUrl(pluginParams);
                }
            }

            playlist.Items = items.ToArray();

            return playlist;
        }

        private Item[] GetRootListRequest() {
            var items = new List<Item>();

            var item = new Item() {
                Name = "По добавлению",
                Link = "main"
            };
            items.Add(item);

            item = new Item() {
                Name = "По популярности",
                Link = "top"
            };
            items.Add(item);

            item = new Item() {
                Name = "Список шоу",
                Link = "channels"
            };
            items.Add(item);

            return items.ToArray();
        }

        private Item[] GetFilteringListRequest(IPluginContext context, string type, string filter = "all", string page = "1") {
            var items = new List<Item>();

            int intPage;
            int.TryParse(page, out intPage);

            //if ((string.IsNullOrEmpty(filter) || filter == "all") && (string.IsNullOrEmpty(page) || page == "1")) {
            //    var item = new Item() {
            //        Name = "Игры",
            //        Link = string.Format("{1}{0}{2}", SEPARATOR, type, "1785")
            //    };
            //    items.Add(item);
            //    item = new Item() {
            //        Name = "Кино",
            //        Link = string.Format("{1}{0}{2}", SEPARATOR, type, "1783")
            //    };
            //    items.Add(item);
            //    item = new Item() {
            //        Name = "Музыка",
            //        Link = string.Format("{1}{0}{2}", SEPARATOR, type, "1787")
            //    };
            //    items.Add(item);
            //    item = new Item() {
            //        Name = "Мультфильмы",
            //        Link = string.Format("{1}{0}{2}", SEPARATOR, type, "1805")
            //    };
            //    items.Add(item);
            //    item = new Item() {
            //        Name = "Развлечения",
            //        Link = string.Format("{1}{0}{2}", SEPARATOR, type, "1781")
            //    };
            //    items.Add(item);
            //    item = new Item() {
            //        Name = "Сериалы",
            //        Link = string.Format("{1}{0}{2}", SEPARATOR, type, "1792")
            //    };
            //    items.Add(item);
            //    item = new Item() {
            //        Name = "Спорт",
            //        Link = string.Format("{1}{0}{2}", SEPARATOR, type, "1788")
            //    };
            //    items.Add(item);
            //    item = new Item() {
            //        Name = "Технологии",
            //        Link = string.Format("{1}{0}{2}", SEPARATOR, type, "1782")
            //    };
            //    items.Add(item);
            //    item = new Item() {
            //        Name = "Юмор",
            //        Link = string.Format("{1}{0}{2}", SEPARATOR, type, "1780")
            //    };
            //    items.Add(item);

            //    filter = "all";
            //}

            var header = new Dictionary<string, string>() {
                {"Accept-Encoding", "gzip, deflate, lzma"},
                {"Content-Type", "text/html; charset=UTF-8"}
            };
            var data = new Dictionary<string, string>() {
                {"limit", PAGE_SIZE.ToString()},
                {"from", ((intPage - 1)*PAGE_SIZE).ToString()},
                {"action", "crmb_load_videos"},
            };
            string datastring = "";
            foreach (var k in data)
            {
                if (datastring != "") datastring += "&";
                datastring += System.Net.WebUtility.UrlEncode(k.Key) + "=" + System.Net.WebUtility.UrlEncode(k.Value);
            }
            string response = context.GetHttpClient().PostRequest(SITE_API_URL, datastring, header);
            
            var totalMatch = Regex.Match(response, "(\"total\"\\s*:\\s*\"(\\d+)\")");
            int intTotal = 0;
            try
            {
                intTotal = int.Parse(totalMatch.Groups[2].Value);
            }
            catch (Exception e) { }
            var listMatch = Regex.Match(response, "(\"html\"\\s*:\\s*\"([\\s\\S]*))", RegexOptions.Multiline);

            if (listMatch.Success) {
                var matches = Regex.Matches(Regex.Unescape(listMatch.Value),
                    "(class=\"item\")([\\s\\S]*?)(data-post_id=\"(\\d+)\")([\\s\\S]*?)(href=\"(\\S*?)\")([\\s\\S]*?)(src=\"(\\S*?)\")([\\s\\S]*?)(<span>([\\s\\S]*?)<\\/span>)([\\s\\S]*?)(<span>([\\s\\S]*?)<\\/span>)");

                for (int i = 0; i < matches.Count; i++) {
                    Item item = GetItemInfoRequest(matches[i].Groups[4].Value);

                    item.Name = Uri.UnescapeDataString(matches[i].Groups[16].Value);
                    item.Link = string.Format("{1}{0}{2}", SEPARATOR, "item", matches[i].Groups[7].Value);
                    item.ImageLink = matches[i].Groups[10].Value;

                    items.Add(item);
                }

                if (intTotal >= intPage * PAGE_SIZE) {
                    var item = new Item() {
                        Name = string.Format("СТРАНИЦА {0}", intPage + 1),
                        Link = string.Format("{1}{0}{2}{0}{3}", SEPARATOR, type, filter, intPage + 1),
                        ImageLink = NEXT_PAGE_IMAGE_URL
                    };

                    items.Add(item);
                }
            }

            return items.ToArray();
        }

        private Item[] GetTopItemListRequest(IPluginContext context, string type) {
            var items = new List<Item>();

            var tempItemsMatches = new List<Match>();

            var header = new Dictionary<string, string>() {
                {"Accept-Encoding", "gzip, deflate, lzma"},
                {"Content-Type", "text/html; charset=UTF-8"}
            };

            string response = context.GetHttpClient().GetRequest(string.Format(SITE_URL, type), header);

            var tempMatches = Regex.Matches(response,
                "(<div class=\"items-gallery items-matrix)([\\S\\s]*?)(<\\/div><!-- \\.items-gallery -->)", RegexOptions.Multiline);

            foreach (Match tempMatch in tempMatches) {
                tempItemsMatches.AddRange(Regex.Matches(tempMatch.Value,
                    "(<div id='post_(\\d+)' class=\"item\")([\\s\\S]*?)(data-post_id=\"(\\d+)\")([\\s\\S]*?)(<a class=\"item-image\" href=\"(\\S*?)\">)([\\s\\S]*?)(src=\"(\\S*?)\")([\\s\\S]*?)(<span>([\\s\\S]*?)<\\/span>)([\\s\\S]*?)(<span>([\\s\\S]*?)<\\/span>)",
                    RegexOptions.Multiline)
                    .Cast<Match>()
                    .ToList());
            }

            if (tempItemsMatches.Any()) {
                foreach (Match itemMatch in tempItemsMatches) {
                    var item = GetItemInfoRequest(itemMatch.Groups[8].Value);

                    item.Name = itemMatch.Groups[17].Value + " " + itemMatch.Groups[14].Value;
                    item.Link = string.Format("{1}{0}{2}", SEPARATOR, "item", itemMatch.Groups[8].Value);
                    item.ImageLink = itemMatch.Groups[11].Value;
                    item.Description = string.Format("<img src=\"{0}\" alt=\"\" align=\"left\"/><h3>{1}</h3>",
                        itemMatch.Groups[11].Value,
                        itemMatch.Groups[17].Value);

                    items.Add(item);
                }
            }

            return items.ToArray();
        }

        private Item[] GetContainerListRequest(IPluginContext context, string type, string page = "1") {
            var items = new List<Item>();

            int intPage;
            int.TryParse(page, out intPage);

            var containerListMatches = new List<Match>();
            
                var header = new Dictionary<string, string>() {
                    {"Accept-Encoding", "gzip, deflate, lzma"},
                    {"Content-Type", "text/html; charset=UTF-8"}
                };

                string response = context.GetHttpClient().GetRequest(string.Format(SITE_URL, type), header);

                var tempMatch = Regex.Match(response,
                    "(<div class=\"items items-shows\">\\s*)([\\s\\S]*?)(\\s*<\\/div><!-- .items -->)");

            if (tempMatch.Success) {
                containerListMatches.AddRange(Regex.Matches(tempMatch.Value,
                    "(<div class='item' data-item='(\\d+)'>\\s*)([\\s\\S]*?)(<a class='item-a' href=')([\\S]*?)('>)([\\s\\S]*?)(<span class='item-image'><img src=')([\\S]*?)('>)([\\s\\S]*?)(<span class='item-name'>)([\\s\\S]*?)(<\\/span>)([\\s\\S]*?)(<span class='item-title'>)([\\s\\S]*?)(<\\/span>)([\\s\\S]*?)(data-show_id='(\\d+)')",
                    RegexOptions.Multiline)
                    .Cast<Match>());
            }

            if (containerListMatches.Any()) {
                for (int i = (intPage-1)*PAGE_SIZE; i < Math.Min(intPage * PAGE_SIZE, containerListMatches.Count); i++) {
                    var item = GetItemInfoRequest(containerListMatches[i].Groups[21].Value);

                    if (string.IsNullOrEmpty(item.Description)) {
                        item.Description = containerListMatches[i].Groups[13].Value;
                    }
                    if (string.IsNullOrEmpty(item.ImageLink)) {
                        item.ImageLink = containerListMatches[i].Groups[9].Value;
                    }
                    item.Name = containerListMatches[i].Groups[17].Value;
                    item.Link = string.Format("{1}{0}{2}", SEPARATOR, "channel", containerListMatches[i].Groups[21].Value);

                    items.Add(item);
                }

                if (containerListMatches.Count >= intPage * PAGE_SIZE) {
                    var item = new Item() {
                        Name = string.Format("СТРАНИЦА {0}", intPage + 1),
                        Link = string.Format("{1}{0}{2}", SEPARATOR, type, intPage + 1),
                        ImageLink = NEXT_PAGE_IMAGE_URL
                    };

                    items.Add(item);
                }
            }

            return items.ToArray();
        }

        private Item[] GetItemListRequest(IPluginContext context, string type, string url, string page = "1") {
            var items = new List<Item>();

            int intPage;
            int.TryParse(page, out intPage);

            var data = new Dictionary<string, string>() {
                    {"limit", PAGE_SIZE.ToString()},
                    {"from", ((intPage-1)*PAGE_SIZE).ToString()},
                    {"term_id", url},
                    {"action", "crmb_load_videos_by_show"},
                };
            var header = new Dictionary<string, string>() {
                {"Accept-Encoding", "gzip, deflate, lzma"},
                {"Content-Type", "text/html; charset=UTF-8"}
            };
            string datastring = "";
            foreach (var k in data)
            {
                if (datastring != "") datastring += "&";
                datastring += System.Net.WebUtility.UrlEncode(k.Key) + "=" + System.Net.WebUtility.UrlEncode(k.Value);
            }
            string response = context.GetHttpClient().PostRequest(SITE_API_URL, datastring, header);

            var totalMatch = Regex.Match(response, "(\"total\"\\s*:\\s*\"(\\d+)\")");
            int intTotal = int.Parse(totalMatch.Groups[2].Value);

            var listMatch = Regex.Match(response, "(\"html\"\\s*:\\s*\"([\\s\\S]*))", RegexOptions.Multiline);

            if (listMatch.Success) {
                var matches = Regex.Matches(Regex.Unescape(listMatch.Value),
                    "(class=\"item\")([\\s\\S]*?)(data-post_id=\"(\\d+)\")([\\s\\S]*?)(href=\"(\\S*?)\")([\\s\\S]*?)(src=\"(\\S*?)\")([\\s\\S]*?)(<span>([\\s\\S]*?)<\\/span>)([\\s\\S]*?)(<span>([\\s\\S]*?)<\\/span>)");

                for (int i = 0; i < matches.Count; i++) {
                    Item item = GetItemInfoRequest(matches[i].Groups[4].Value);

                    item.Name = Uri.UnescapeDataString(matches[i].Groups[16].Value);
                    item.Link = string.Format("{1}{0}{2}", SEPARATOR, "item", matches[i].Groups[7].Value);
                    item.ImageLink = matches[i].Groups[10].Value;

                    items.Add(item);
                }

                if (intTotal >= intPage*PAGE_SIZE) {
                    var item = new Item() {
                        Name = string.Format("СТРАНИЦА {0}", intPage + 1),
                        Link = string.Format("{1}{0}{2}{0}{3}", SEPARATOR, type, url, intPage + 1),
                        ImageLink = NEXT_PAGE_IMAGE_URL
                    };

                    items.Add(item);
                }
            }

            return items.ToArray();
        }

        private Item[] GetItemRequest(IPluginContext context, string url) {
            var items = new List<Item>();

            var header = new Dictionary<string, string>() {
                {"Accept-Encoding", "gzip, deflate, lzma"},
                {"Content-Type", "text/html; charset=UTF-8"}
            };
            string response = context.GetHttpClient().GetRequest(url, header);

            var match = Regex.Match(response, "(video_id:\\s*)(\\d+)");

            if (match.Success) {
                response = context.GetHttpClient().GetRequest(string.Format(SITE_URL_VIDEO, match.Groups[2].Value, "videoinfo.js"), header);

                var videoMatch = Regex.Match(response, "(\"qualities\":[)([\\s\\S]*?)(])",
                    RegexOptions.Multiline);

                var qulityMatch = Regex.Matches(videoMatch.Value, "({ \"\\w+\":\")(\\S+.mp4)(\", \"height\":)(\\S+)");

                var titleMatch = Regex.Match(response, "(\"title\": \")([\\s\\S]*?)(\")", RegexOptions.Multiline);
                var imageMatch = Regex.Match(response, "(\"splash\": \")([\\s\\S]*?)(\")");

                var description = string.Format("<img src=\"{0}\" alt=\"\" align=\"left\"/><h3>{1}</h3>",
                    imageMatch.Groups[2].Value,
                    Regex.Unescape(titleMatch.Groups[2].Value));
                
                Item Item = GetItemInfoRequest(url.Split('/')[1]);

                for (int i = 0; i < qulityMatch.Count; i++) {
                    var item = new Item(Item) {
                        Name = qulityMatch[i].Groups[2].Value,
                        Link = string.Format(SITE_URL_VIDEO, match.Groups[2].Value, qulityMatch[i].Groups[2].Value),
                        ImageLink = imageMatch.Groups[2].Value,
                        Description = description,
                        Type = ItemType.FILE
                    };

                    items.Add(item);
                }
            }

            return items.ToArray();
        }

        private Item GetItemInfoRequest(string id, string text = "") {
            Item item;

            if (string.IsNullOrEmpty(text)) {
                if (showsItems.ContainsKey(id)) {
                    item = showsItems[id];
                } else {
                    item = new Item();
                    showsItems.Add(id, item);
                }
            } else {
                if (showsItems.ContainsKey(id)) {
                    item = showsItems[id];
                } else {
                    item = new Item();
                    showsItems.Add(id, item);
                }
                if (string.IsNullOrEmpty(item.Description)) {
                    item.Description = string.Format(
                        "<img src=\"{0}\" alt=\"\" align=\"left\"/>{1}",
                        string.Format(IMAGE_URL, id), text);
                }
            }

            return new Item(item);
        }
    }
}