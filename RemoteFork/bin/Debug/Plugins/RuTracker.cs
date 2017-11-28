using System;
using System.Collections.Specialized;
using PluginApi.Plugins;
using System.Text.RegularExpressions;

namespace RemoteFork.Plugins
{

    [PluginAttribute(Id = "rutracker", Version = "0.1.a", Author = "ORAMAN", Name = "RuTracker", Description = "", ImageLink = "http://s1.iconbird.com/ico/1012/AmpolaIcons/w256h2561350597291videofile.png")]
    public class RuTracker : IPlugin
    {

        private string IPAdress;
        private string PortRemoteFork = "8027";
        private string PortAce = "6878";
        private bool AceProxEnabl;
        private string PLUGIN_PATH = "pluginPath";
        private string ProxyServr = "proxy.antizapret.prostovpn.org";
        private int ProxyPort = 3128;
        private bool ProxyEnablerRuTr = true;
        private string TrackerServer = "https://rutracker.org";
        private string Cookies = "bb_ssl=1; opt_js={%22only_new%22:0%2C%22h_flag%22:0%2C%22h_av%22:0%2C%22h_rnk_i%22:0%2C%22h_post_i%22:0%2C%22i_aft_l%22:0%2C%22h_smile%22:0%2C%22h_sig%22:0%2C%22sp_op%22:0%2C%22tr_tm%22:0%2C%22h_cat%22:%22%22%2C%22h_tsp%22:0%2C%22hl_brak%22:1%2C%22div_tag%22:1%2C%22h_ta%22:%2217%22}; bb_session=0-42155450-LsvX15qU34lcufO3dwS0; tr_simple=1; __lnkrntdmcvrd=-1";



        public Playlist GetList(IPluginContext context)
        {

            IPAdress = context.GetRequestParams()["host"].Split(':')[0];



            var path = context.GetRequestParams().Get(PLUGIN_PATH);
            path = ((((path == null)) ? "plugin" : "plugin;" + path));

            if (context.GetRequestParams()["search"] != null)
            {
                switch (path)
                {
                    //case "plugin;Search_NNM":
                     case "plugin;Search_rutracker":
                        return SearchListRuTr(context, context.GetRequestParams()["search"]);
                }
            }


            switch (path)
            {
                case "plugin":
                    return GetTopListRuTr(context);
            }

            string[] PathSpliter = path.Split(';');

            switch (PathSpliter[PathSpliter.Length - 1])
            {
                //Case "PAGERUTR"
                //    Return GetPAGERUTR(context, PathSpliter(PathSpliter.Length - 2))
                case "PAGEFILMRUTR":
                    return GetTorrentPageRuTr(context, PathSpliter[PathSpliter.Length - 2]);
            }

            return GetTopListRuTr(context);
        }

        public PluginApi.Plugins.Playlist GetTorrentPageRuTr(IPluginContext context, string URL)
        {

            System.Net.WebRequest RequestGet = System.Net.WebRequest.Create(URL);
            if (ProxyEnablerRuTr == true)
            {
                RequestGet.Proxy = new System.Net.WebProxy(ProxyServr, ProxyPort);
            }
            RequestGet.Method = "GET";
            RequestGet.Headers.Add("Cookie", Cookies);

            System.Net.WebResponse Response = RequestGet.GetResponse();
            System.IO.Stream dataStream = Response.GetResponseStream();
            System.IO.StreamReader reader = new System.IO.StreamReader(dataStream, System.Text.Encoding.GetEncoding(1251));
            string responseFromServer = reader.ReadToEnd();
            reader.Close();
            dataStream.Close();
            Response.Close();



            System.Text.RegularExpressions.Regex Regex = new System.Text.RegularExpressions.Regex("(?<=<p><a href=\").*?(?=\")");
            string TorrentPath = TrackerServer + "/forum/" + Regex.Matches(responseFromServer)[0].Value;


            System.Net.WebRequest RequestTorrent = System.Net.WebRequest.Create(TorrentPath);
            if (ProxyEnablerRuTr == true)
            {
                RequestTorrent.Proxy = new System.Net.WebProxy(ProxyServr, ProxyPort);
            }
            RequestTorrent.Method = "GET";
            RequestTorrent.Headers.Add("Cookie", Cookies);

            Response = RequestTorrent.GetResponse();
            dataStream = Response.GetResponseStream();
            reader = new System.IO.StreamReader(dataStream, System.Text.Encoding.GetEncoding(1251));
            string FileTorrent = reader.ReadToEnd();
            System.IO.File.WriteAllText(System.IO.Path.GetTempPath() + "TorrentTemp.torrent", FileTorrent, System.Text.Encoding.GetEncoding(1251));
            reader.Close();
            dataStream.Close();
            Response.Close();


            TorrentPlayList[] PlayListtoTorrent = GetFileListJSON(System.IO.Path.GetTempPath() + "TorrentTemp.torrent", IPAdress);

            System.Collections.Generic.List<Item> items = new System.Collections.Generic.List<Item>();

            string Description = FormatDescriptionFileRuTr(responseFromServer);
            foreach (TorrentPlayList PlayListItem in PlayListtoTorrent)
            {

                Item Item = new Item();
                Item.Name = PlayListItem.Name;
                Item.ImageLink = "http://s1.iconbird.com/ico/1012/AmpolaIcons/w256h2561350597291videofile.png";
                Item.Link = PlayListItem.Link;
                Item.Type = ItemType.FILE;
                Item.Description = Description;
                items.Add(Item);
            }

            return PlayListPlugPar(items, context);
        }

        public string FormatDescriptionFileRuTr(string HTML)
        {

            HTML = HTML.Replace("\n", "");

            string Title = null;
            try
            {
                System.Text.RegularExpressions.Regex Regex = new System.Text.RegularExpressions.Regex("(?<=<title>).*?(</title>)");
                Title = Regex.Matches(HTML)[0].Value;
            }
            catch (Exception ex)
            {
                Title = ex.Message;
            }


            string SidsPirs = null;
            try
            {
                System.Text.RegularExpressions.Regex Regex = new System.Text.RegularExpressions.Regex("(<td class=\"borderless bCenter pad_8\">).*?(?=</div>)");
                SidsPirs = "<p>" + Regex.Matches(HTML)[0].Value;
            }
            catch (Exception ex)
            {
                SidsPirs = ex.Message;
            }


            string ImagePath = null;
            try
            {
                System.Text.RegularExpressions.Regex Regex = new System.Text.RegularExpressions.Regex("(?<=<var class=\"postImg postImgAligned img-right\" title=\").*?(?=\">)");
                ImagePath = Regex.Matches(HTML)[0].Value;
            }
            catch (Exception ex)
            {
            }


            string InfoFile = null;
            try
            {
                System.Text.RegularExpressions.Regex Regex = new System.Text.RegularExpressions.Regex("(<span class=\"post-b\">).*(?=<div class=\"sp-wrap\">)");
                InfoFile = Regex.Matches(HTML)[0].Value;
            }
            catch (Exception ex)
            {
                InfoFile = ex.Message;
            }

            return "<html><captionn align=\"left\" valign=\"top\"><strong>" + Title + "</strong></caption><style>.leftimg {float:left;margin: 7px 7px 7px 0;</style>" + SidsPirs + "<table><tbody><tr><th align=\"left\" width=\"220\" valign=\"top\"><img src=\"" + ImagePath + "\"width=\"220\" height=\"290\" class=\"leftimg\" <font face=\"Arial Narrow\" size=\"3\">" + InfoFile + "</tr></tbody></table><div></font></div></html>";

        }

        public PluginApi.Plugins.Playlist GetTopListRuTr(IPluginContext context)
        {

            System.Collections.Generic.List<Item> items = new System.Collections.Generic.List<Item>();
            Item Item = new Item();

            Item.Name = "Поиск";
            Item.Link = "Search_rutracker";
            Item.Type = ItemType.DIRECTORY;
            Item.SearchOn = "search_on";
            Item.ImageLink = "http://s1.iconbird.com/ico/0612/MustHave/w256h2561339195991Search256x256.png";
            Item.Description = "<html><font face=\"Arial\" size=\"5\"><b>" + Item.Name + "</font></b><p><img src=\"https://rutrk.org/logo/logo.gif\" />";
            items.Add(Item);

            return PlayListPlugPar(items, context);
        }

        public PluginApi.Plugins.Playlist SearchListRuTr(IPluginContext context, string search)
        {
            //Dim Kino As String = "100,101,1105,1165,1213,1235,124,1245,1246,1247,1248,1250,1386,1387,1388,1389,1390,1391,1478,1543,1576,1577,1642,1666,1670,185,187,1900,1991,208,209,2090,2091,2092,2093,2097,2109,212,2198,2199,22,2200,2201,2220,2221,2258,2339,2343,2365,2459,2491,2540,281,282,312,313,33,352,376,4,404,484,505,511,514,521,539,549,572,599,656,7,709,822,893,905,921,922,923,924,925,926,927,928,93,930,934,941"
            //Dim Dokumentals As String = ",103,1114,113,114,115,1186,1278,1280,1281,1327,1332,137,1453,1467,1468,1469,1475,1481,1482,1484,1485,1495,1569,1959,2076,2107,2110,2112,2123,2159,2160,2163,2164,2166,2168,2169,2176,2177,2178,2323,2380,24,249,251,2537,2538,294,314,373,393,46,500,532,552,56,670,671,672,752,821,827,851,876,882,939,97,979,98"
            //Dim Sport As String = ",1188,126,1287,1319,1323,1326,1343,1470,1491,1527,1551,1608,1610,1613,1614,1615,1616,1617,1620,1621,1623,1630,1667,1668,1675,1697,1952,1986,1987,1997,1998,2001,2002,2003,2004,2005,2006,2007,2008,2009,2010,2014,2069,2073,2075,2079,2111,2124,2171,2425,2514,255,256,257,259,260,262,263,283,343,486,528,550,626,660,751,845,854,875,978"
            //Dim RequestPost As System.Net.WebRequest = System.Net.WebRequest.Create(TrackerServer & "/forum/tracker.php?f=" & Kino & Dokumentals & Sport & "&nm=" & search)
            System.Net.WebRequest RequestPost = System.Net.WebRequest.Create(TrackerServer + "/forum/tracker.php?nm=" + search);

            if (ProxyEnablerRuTr == true)
            {
                RequestPost.Proxy = new System.Net.WebProxy(ProxyServr, ProxyPort);
            }
            RequestPost.Method = "POST";
            RequestPost.ContentType = "text/html; charset=windows-1251";
            RequestPost.Headers.Add("Cookie", Cookies);
            RequestPost.ContentType = "application/x-www-form-urlencoded";
            System.IO.Stream myStream = RequestPost.GetRequestStream();
            string DataStr = "prev_new=0&prev_oop=0&o=10&s=2&pn=&nm=" + search;
            byte[] DataByte = System.Text.Encoding.GetEncoding(1251).GetBytes(DataStr);
            myStream.Write(DataByte, 0, DataByte.Length);
            myStream.Close();

            System.Net.WebResponse Response = RequestPost.GetResponse();
            System.IO.Stream dataStream = Response.GetResponseStream();
            System.IO.StreamReader reader = new System.IO.StreamReader(dataStream, System.Text.Encoding.GetEncoding(1251));
            string ResponseFromServer = reader.ReadToEnd();


            System.Collections.Generic.List<Item> items = new System.Collections.Generic.List<Item>();
            System.Text.RegularExpressions.Regex Regex = new System.Text.RegularExpressions.Regex("(<tr class=\"tCenter hl-tr\">).*?(</tr>)");
            System.Text.RegularExpressions.MatchCollection Result = Regex.Matches(ResponseFromServer.Replace("\n", " "));

            if (Result.Count > 0)
            {

                foreach (System.Text.RegularExpressions.Match Match in Result)
                {
                    Item Item = new Item();
                    Regex = new System.Text.RegularExpressions.Regex("(?<=<a data-topic_id=\").*?(?=\")");
                    string LinkID = Regex.Matches(Match.Value)[0].Value;
                    Item.Link = TrackerServer + "/forum/viewtopic.php?t=" + LinkID + ";PAGEFILMRUTR";
                    Regex = new System.Text.RegularExpressions.Regex("(?<=" + LinkID + "\">).*?(?=</a>)");
                    Item.Name = Regex.Matches(Match.Value)[0].Value;
                    Item.ImageLink = "http://s1.iconbird.com/ico/1012/AmpolaIcons/w256h2561350597291utorrent2.png";
                    Item.Description = GetDescriptionSearhRuTr(Match.Value, Item.Name, LinkID);
                    items.Add(Item);
                }
            }
            else
            {
                Item Item = new Item();
                Item.Name = "Ничего не найдено";
                Item.Link = "";

                items.Add(Item);
            }

            return PlayListPlugPar(items, context);
        }

        public string GetDescriptionSearhRuTr(string HTML, string NameFilm, string LinkID)
        {

            string SizeFile = null;
            try
            {
                System.Text.RegularExpressions.Regex Regex = new System.Text.RegularExpressions.Regex("(?<=" + LinkID + "\">).*?(?=<)");
                SizeFile = "<p> Размер: <b>" + Regex.Matches(HTML)[1].Value + "</b>";
            }
            catch (Exception ex)
            {
            }

            string DobavlenFile = null;
            try
            {
                System.Text.RegularExpressions.Regex Regex = new System.Text.RegularExpressions.Regex("(?<=<span class=\"tor-icon tor-approved\">&radic;</span></span>&nbsp;).*?(?=</p>)");
                DobavlenFile = "<p><b>" + Regex.Matches(HTML)[0].Value + "</b>";
            }
            catch (Exception ex)
            {
            }

            string Seeders = null;
            try
            {
                System.Text.RegularExpressions.Regex Regex = new System.Text.RegularExpressions.Regex("(?<=title=\"Сидов\">).*?(?=<)");
                Seeders = "<p> Seeders: <b> " + Regex.Matches(HTML)[0].Value + "</b>";
            }
            catch (Exception ex)
            {
            }

            string Leechers = null;
            try
            {
                System.Text.RegularExpressions.Regex Regex = new System.Text.RegularExpressions.Regex("(?<=title=\"Личей\">).*?(?=<)");
                Leechers = "<p> Leechers: <b> " + Regex.Matches(HTML)[0].Value + "</b>";
            }
            catch (Exception ex)
            {
            }

            return "<html><font face=\"Arial\" size=\"5\"><b>" + NameFilm + "</font></b><p><font face=\"Arial Narrow\" size=\"4\">" + SizeFile + DobavlenFile + Seeders + Leechers + "</font></html>";
        }

        public PluginApi.Plugins.Playlist PlayListPlugPar(System.Collections.Generic.List<Item> items, IPluginContext context)
        {
            PluginApi.Plugins.Playlist PlayList = new PluginApi.Plugins.Playlist();
            PlayList.Items = items.ToArray();
            foreach (Item Item in PlayList.Items)
            {
                if (ItemType.DIRECTORY == Item.Type)
                {
                    var pluginParams = new NameValueCollection();
                    pluginParams[PLUGIN_PATH] = Item.Link;
                    Item.Link = context.CreatePluginUrl(pluginParams);
                }
            }
            return PlayList;
        }
        public string GetID(string PathTorrent, string ServerAdress)
        {
            System.Net.WebClient WC = new System.Net.WebClient();
            WC.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:47.0) Gecko/20100101 Firefox/47.0");
            WC.Encoding = System.Text.Encoding.UTF8;
            byte[] FileTorrent = WC.DownloadData(PathTorrent);

            string FileTorrentString = System.Convert.ToBase64String(FileTorrent);
            FileTorrent = System.Text.Encoding.Default.GetBytes(FileTorrentString);

            System.Net.WebRequest request = System.Net.WebRequest.Create("http://api.torrentstream.net/upload/raw");
            request.Method = "POST";
            request.ContentType = "application/octet-stream\\r\\n";
            request.ContentLength = FileTorrent.Length;
            System.IO.Stream dataStream = request.GetRequestStream();
            dataStream.Write(FileTorrent, 0, FileTorrent.Length);
            dataStream.Close();

            System.Net.WebResponse response = request.GetResponse();
            dataStream = response.GetResponseStream();
            System.IO.StreamReader reader = new System.IO.StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();

            string[] responseSplit = responseFromServer.Split('\"');
            return responseSplit[3];
        }

        public TorrentPlayList[] GetFileListJSON(string PathTorrent, string ServerAdress)
        {
            System.Net.WebClient WC = new System.Net.WebClient();
            WC.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:47.0) Gecko/20100101 Firefox/47.0");
            WC.Encoding = System.Text.Encoding.UTF8;

            string[] CodeZnaki = { "\\U0430", "\\U0431", "\\U0432", "\\U0433", "\\U0434", "\\U0435", "\\U0451", "\\U0436", "\\U0437", "\\U0438", "\\U0439", "\\U043A", "\\U043B", "\\U043C", "\\U043D", "\\U043E", "\\U043F", "\\U0440", "\\U0441", "\\U0442", "\\U0443", "\\U0444", "\\U0445", "\\U0446", "\\U0447", "\\U0448", "\\U0449", "\\U044A", "\\U044B", "\\U044C", "\\U044D", "\\U044E", "\\U044F", "\\U0410", "\\U0411", "\\U0412", "\\U0413", "\\U0414", "\\U0415", "\\U0401", "\\U0416", "\\U0417", "\\U0418", "\\U0419", "\\U041A", "\\U041B", "\\U041C", "\\U041D", "\\U041E", "\\U041F", "\\U0420", "\\U0421", "\\U0422", "\\U0423", "\\U0424", "\\U0425", "\\U0426", "\\U0427", "\\U0428", "\\U0429", "\\U042A", "\\U042B", "\\U042C", "\\U042D", "\\U042E", "\\U042F", "\\U00AB", "\\U00BB", "U2116" };
            string[] DecodeZnaki = { "а", "б", "в", "г", "д", "е", "ё", "ж", "з", "и", "й", "к", "л", "м", "н", "о", "п", "р", "с", "т", "у", "ф", "х", "ц", "ч", "ш", "щ", "ъ", "ы", "ь", "э", "ю", "я", "А", "Б", "В", "Г", "Д", "Е", "Ё", "Ж", "З", "И", "Й", "К", "Л", "М", "Н", "О", "П", "Р", "С", "Т", "У", "Ф", "Х", "Ц", "Ч", "Ш", "Щ", "Ъ", "Ы", "Ь", "Э", "Ю", "Я", "«", "»", "№" };

            string ContentID = GetID(PathTorrent, ServerAdress);
            string ItogStr = WC.DownloadString("http://" + ServerAdress + ":" + PortAce + "/server/api?method=get_media_files&content_id=" + ContentID);
            for (int I = 0; I <= 68; ++I)
            {
                ItogStr = ItogStr.Replace(CodeZnaki[I].ToLower(), DecodeZnaki[I]);
            }
            WC.Dispose();

            string PlayListJson = ItogStr;
            PlayListJson = PlayListJson.Replace(",", null);
            PlayListJson = PlayListJson.Replace(":", null);
            PlayListJson = PlayListJson.Replace("}", null);
            PlayListJson = PlayListJson.Replace("{", null);
            PlayListJson = PlayListJson.Replace("result", null);
            PlayListJson = PlayListJson.Replace("error", null);
            PlayListJson = PlayListJson.Replace("null", null);
            PlayListJson = PlayListJson.Replace("\"\"", "\"");
            PlayListJson = PlayListJson.Replace("\" \"", "\"");

            string[] ListSplit = PlayListJson.Split('\"');
            TorrentPlayList[] PlayListTorrent = new TorrentPlayList[(ListSplit.Length / 2) - 2 + 1];
            int N = 0;
            for (int I = 1; I <= ListSplit.Length - 2; ++I)
            {
                PlayListTorrent[N].IDX = ListSplit[I];
                PlayListTorrent[N].Name = ListSplit[I + 1];
                PlayListTorrent[N].Link = "http://" + ServerAdress + ":" + PortAce + "/ace/getstream?id=" + ContentID + "&_idx=" + PlayListTorrent[N].IDX;

                ++I;
                ++N;
            }
            return PlayListTorrent;
        }
        public struct TorrentPlayList
        {
            public string IDX;
            public string Name;
            public string Link;
            public string Description;
        }

    }
}