Option Explicit On
Imports System.Linq
Imports System
Imports System.Collections.Generic
Imports System.Collections.Specialized
Imports PluginApi.Plugins
Imports RemoteFork.Plugins
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic

Namespace RemoteFork.Plugins

    <PluginAttribute(Id:="rutrackervb", Version:="0.15b", Author:="ORAMAN", Name:="RuTrackervb", Description:="", ImageLink:="http://s1.iconbird.com/ico/1012/AmpolaIcons/w256h2561350597291videofile.png")>
    Public Class RuTracker
        Implements IPlugin

        Dim IPAdress As String
        Dim PortRemoteFork As String = "8027"
        Dim PortAce As String = "6878"
        Dim AceProxEnabl As Boolean
        Dim PLUGIN_PATH As String = "pluginPath"
        Dim PlayList As New PluginApi.Plugins.Playlist
        Dim next_page_url As String
        Dim ProxyServr As String = "proxy.antizapret.prostovpn.org"
        Dim ProxyPort As Integer = 3128
        Dim ProxyEnablerRuTr As Boolean = True
        Dim TrackerServer As String = "https://rutracker.org"

        Public Function GetList(context As IPluginContext) As Playlist Implements IPlugin.GetList
            IPAdress = context.GetRequestParams.Get("host").Split(":")(0)

            Dim path = context.GetRequestParams().Get(PLUGIN_PATH)
            path = (If((path Is Nothing), "plugin", "plugin;" & path))


            If context.GetRequestParams("search") <> Nothing Then
                Select Case path
                    Case "plugin;Search_rutracker"
                        Return SearchListRuTr(context, context.GetRequestParams("search"))

                    Case "plugin;RuTr_Login"
                        Login = context.GetRequestParams("search")
                        ' Microsoft.Win32.Registry.SetValue("HKEY_CURRENT_USER\Software\RemoteFork\Plugins\RuTracker\", " Logging", Login)
                        Return SetPassword(context)

                    Case "plugin;RuTr_Password"
                        Password = context.GetRequestParams("search")
                        'Microsoft.Win32.Registry.SetValue("HKEY_CURRENT_USER\Software\RemoteFork\Plugins\RuTracker\", " Password", Password)
                        Return AuthorizationRuTr(context)
                    Case "plugin;RuTr_Capcha_Key"
                        Capcha = context.GetRequestParams("search")
                        Return AuthorizationRuTr(context)
                End Select

            End If


            Select Case path
                Case "plugin"
                    Return GetTopListRuTr(context)
            End Select

            Dim PathSpliter() As String = path.Split(";")

            Select Case PathSpliter(PathSpliter.Length - 1)
                'Case "PAGERUTR"
                '    Return GetPAGERUTR(context, PathSpliter(PathSpliter.Length - 2))
                Case "PAGEFILMRUTR"
                    Return GetTorrentPageRuTr(context, PathSpliter(PathSpliter.Length - 2))
                Case "RuTrNonAuthorization"
                    Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree("Software\RemoteFork\Plugins\RuTracker\", False)
                    Return GetTopListRuTr(context)
            End Select

            Return GetTopListRuTr(context)
        End Function

#Region "Иконки"
        Dim ICO_Folder As String = "http://s1.iconbird.com/ico/1012/AmpolaIcons/w256h2561350597246folder.png"

        Dim ICO_Settings As String = "http://s1.iconbird.com/ico/2013/11/504/w128h1281385326483gear.png"
        Dim ICO_SettingsFolder As String = "http://s1.iconbird.com/ico/2013/11/504/w128h1281385326516tools.png"
        Dim ICO_SettingsParam As String = "http://s1.iconbird.com/ico/2013/11/504/w128h1281385326449check.png"
        Dim ICO_SettingsReset As String = "http://s1.iconbird.com/ico/2013/11/504/w128h1281385326539check.png"

        Dim ICO_VideoFile As String = "http://s1.iconbird.com/ico/1012/AmpolaIcons/w256h2561350597291videofile.png"
        Dim ICO_MusicFile As String = "http://s1.iconbird.com/ico/1012/AmpolaIcons/w256h2561350597240aimp.png"
        Dim ICO_TorrentFile As String = "http://s1.iconbird.com/ico/1012/AmpolaIcons/w256h2561350597291utorrent2.png"
        Dim ICO_ImageFile As String = "http://s1.iconbird.com/ico/1012/AmpolaIcons/w256h2561350597278imagefile.png"
        Dim ICO_M3UFile As String = "http://s1.iconbird.com/ico/0912/VannillACreamIconSet/w128h1281348320736M3U.png"
        Dim ICO_NNMClub As String = "http://s1.iconbird.com/ico/0912/MorphoButterfly/w128h1281348669898RhetenorMorpho.png"
        Dim ICO_Search As String = "http://s1.iconbird.com/ico/0612/MustHave/w256h2561339195991Search256x256.png"
        Dim ICO_Error As String = "http://s1.iconbird.com/ico/0912/ToolbarIcons/w256h2561346685474SymbolError.png"
        Dim ICO_Error2 As String = "http://errorfix48.ru/uploads/posts/2014-09/1409846068_400px-warning_icon.png"
        Dim ICO_Save As String = "http://s1.iconbird.com/ico/2013/6/355/w128h1281372334742check.png"
        Dim ICO_Delete As String = "http://s1.iconbird.com/ico/2013/6/355/w128h1281372334742delete.png"
        Dim ICO_Pusto As String = "https://avatanplus.com/files/resources/mid/5788db3ecaa49155ee986d6e.png"
        Dim ICO_Login As String = "http://s1.iconbird.com/ico/1012/AmpolaIcons/w256h2561350597246portal2.png"
        Dim ICO_Password As String = "http://s1.iconbird.com/ico/0612/GooglePlusInterfaceIcons/w128h1281338911371password.png"
        Dim ICO_LoginKey As String = "http://s1.iconbird.com/ico/0912/ToolbarIcons/w256h2561346685464Login.png"
#End Region
#Region "Авторизация"
        Dim Login, Password, Cap_Sid, Cap_Code, Capcha, Cookies As String

        Dim UserAuthorization As String
        Function AuthorizationTest() As Boolean
            Cookies = Microsoft.Win32.Registry.GetValue("HKEY_CURRENT_USER\Software\RemoteFork\Plugins\RuTracker\", "Cookies", "")
            If Cookies = "" Then Cookies = "bb_ssl=1"
            Dim Request As System.Net.HttpWebRequest = System.Net.HttpWebRequest.Create("https://rutracker.org/forum/index.php")
            Request.Method = "GET"
            Request.Headers.Add("Cookie", Cookies)
            Request.Host = "rutracker.org"
            Request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36"
            Request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8"
            Request.Headers.Add(Net.HttpRequestHeader.AcceptLanguage, "ru-ru,ru;q=0.8,en-us;q=0.5,en;q=0.3")
            Request.Headers.Add(Net.HttpRequestHeader.AcceptEncoding, "gzip,deflate")
            Request.Headers.Add(Net.HttpRequestHeader.AcceptCharset, "windows-1251,utf-8;q=0.7,*;q=0.7")
            Request.KeepAlive = True
            Request.Referer = "https://rutracker.org/forum/index.php"

            Request.ContentType = "application/x-www-form-urlencoded"
            Request.AllowAutoRedirect = False
            Request.AutomaticDecompression = Net.DecompressionMethods.GZip
            If ProxyEnablerRuTr = True Then Request.Proxy = New System.Net.WebProxy(ProxyServr, ProxyPort)


            Dim Response As Net.HttpWebResponse = Request.GetResponse
            Dim Stream As IO.Stream = Response.GetResponseStream
            Stream = Response.GetResponseStream
            Dim Reader As New System.IO.StreamReader(Stream, System.Text.Encoding.GetEncoding(1251))
            Dim OtvetServera As String = Reader.ReadToEnd.Replace(vbLf, " ")
            Reader.Close()
            Stream.Close()

            Dim Reg As New System.Text.RegularExpressions.Regex("(>Вход</span>).*?(</span>)")
            Dim Matchs As System.Text.RegularExpressions.MatchCollection = Reg.Matches(OtvetServera)

            If Matchs.Count > 0 Then
                Microsoft.Win32.Registry.SetValue("HKEY_CURRENT_USER\Software\RemoteFork\Plugins\RuTracker\", "Cookies", "bb_ssl=1")
                Return False
            Else
                Reg = New System.Text.RegularExpressions.Regex("(<span class=""logged-in-as-cap"">).*?(</div>)")
                Matchs = Reg.Matches(OtvetServera)
                If Matchs.Count > 0 Then
                    Microsoft.Win32.Registry.SetValue("HKEY_CURRENT_USER\Software\RemoteFork\Plugins\RuTracker\", "Cookies", Cookies)
                    UserAuthorization = Matchs(0).Value
                    Return True
                End If
            End If
            Return Nothing
        End Function

        Function AuthorizationRuTr(context As IPluginContext) As PluginApi.Plugins.Playlist
            Dim items As New System.Collections.Generic.List(Of Item)
            Cookies = "bb_ssl=1"
            Dim Request As System.Net.HttpWebRequest = System.Net.HttpWebRequest.Create("https://rutracker.org/forum/login.php?redirect=tracker.php")
            Request.Method = "POST"
            Request.Headers.Add("Cookie", Cookies)
            Request.Host = "rutracker.org"
            Request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36"
            Request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8"
            Request.Headers.Add(Net.HttpRequestHeader.AcceptLanguage, "ru-ru,ru;q=0.8,en-us;q=0.5,en;q=0.3")
            Request.Headers.Add(Net.HttpRequestHeader.AcceptEncoding, "gzip,deflate")
            Request.Headers.Add(Net.HttpRequestHeader.AcceptCharset, "windows-1251,utf-8;q=0.7,*;q=0.7")
            Request.KeepAlive = True
            Request.Referer = "https://rutracker.org/forum/index.php"
            Request.ContentType = "application/x-www-form-urlencoded"
            Request.AllowAutoRedirect = False
            Request.AutomaticDecompression = Net.DecompressionMethods.GZip
            If ProxyEnablerRuTr = True Then Request.Proxy = New System.Net.WebProxy(ProxyServr, ProxyPort)

            Dim StringData As String
            If Capcha = "" Then
                StringData = "redirect=tracker.php&login_username=" & Login & "&login_password=" & Password & "&login=Вход"
            Else
                StringData = "redirect=tracker.php&login_username=" & Login & "&login_password=" & Password & "&cap_sid=" & Cap_Sid & "&" & Cap_Code & "=" & Capcha & "&login=%C2%F5%EE%E4"
            End If
            Capcha = ""
            Dim Stream As IO.Stream = Request.GetRequestStream
            Dim ByteData() As Byte = System.Text.Encoding.GetEncoding(1251).GetBytes(StringData)
            Stream.Write(ByteData, 0, ByteData.Length)
            Stream.Close()


            Dim Response As Net.HttpWebResponse = Request.GetResponse
            Stream = Response.GetResponseStream
            Dim Reader As New System.IO.StreamReader(Stream, System.Text.Encoding.GetEncoding(1251))
            Dim OtvetServera As String = Reader.ReadToEnd.Replace(vbLf, " ")

            If Not String.IsNullOrEmpty(Response.Headers("Set-Cookie")) Then

                Cookies = Response.Headers("Set-Cookie")
                Request = System.Net.HttpWebRequest.Create("https://rutracker.org/forum/index.php")
                Request.Method = "GET"
                Request.Headers.Add("Cookie", Cookies)
                Request.Host = "rutracker.org"
                Request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36"
                Request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8"
                Request.Headers.Add(Net.HttpRequestHeader.AcceptLanguage, "ru-ru,ru;q=0.8,en-us;q=0.5,en;q=0.3")
                Request.Headers.Add(Net.HttpRequestHeader.AcceptEncoding, "gzip,deflate")
                Request.Headers.Add(Net.HttpRequestHeader.AcceptCharset, "windows-1251,utf-8;q=0.7,*;q=0.7")
                Request.KeepAlive = True
                Request.Referer = "https://rutracker.org/forum/login.php?redirect=tracker.php"
                Request.Headers.Add(Net.HttpRequestHeader.Cookie, "spylog_test=1")
                Request.ContentType = "application/x-www-form-urlencoded"
                Request.AllowAutoRedirect = False
                Request.AutomaticDecompression = Net.DecompressionMethods.GZip
                If ProxyEnablerRuTr = True Then Request.Proxy = New System.Net.WebProxy(ProxyServr, ProxyPort)

                Response = Request.GetResponse
                Stream = Response.GetResponseStream
                Reader = New System.IO.StreamReader(Stream, System.Text.Encoding.GetEncoding(1251))
                OtvetServera = Reader.ReadToEnd.Replace(vbLf, " ")
                Reader.Close()
                Stream.Close()

                Dim Reg As New System.Text.RegularExpressions.Regex("(<a href=""profile.php?mode=register""><b>).*?(</span>)")
                Dim Matchs As System.Text.RegularExpressions.MatchCollection = Reg.Matches(OtvetServera)

                If Matchs.Count > 0 Then
                    Microsoft.Win32.Registry.SetValue("HKEY_CURRENT_USER\Software\RemoteFork\Plugins\RuTracker\", "Cookies", "bb_ssl=1")
                    Return SetLogin(context)
                Else
                    Reg = New System.Text.RegularExpressions.Regex("(<span class=""logged-in-as-cap"">).*?(</div>)")
                    Matchs = Reg.Matches(OtvetServera)
                    If Matchs.Count > 0 Then
                        Microsoft.Win32.Registry.SetValue("HKEY_CURRENT_USER\Software\RemoteFork\Plugins\RuTracker\", "Cookies", Cookies)
                        Return GetTopListRuTr(context)
                    End If
                End If
            End If

            Dim AdressCapha As String
            Dim Regex As New System.Text.RegularExpressions.Regex("(?<=<td class=""tRight nowrap"">Код:</td> 					<td> 			<div><img src=""//).*?(.jpg)")
            Dim Matches As System.Text.RegularExpressions.MatchCollection = Regex.Matches(OtvetServera)
            If Matches.Count > 0 Then
                AdressCapha = "http://" & Regex.Matches(OtvetServera)(0).Value
                Regex = New System.Text.RegularExpressions.Regex("(?<=name=""cap_sid"" value="").*?(?="">)")
                Cap_Sid = Regex.Matches(OtvetServera)(0).Value
                Regex = New System.Text.RegularExpressions.Regex("(cap_code_).*?(?="")")
                Cap_Code = Regex.Matches(OtvetServera)(0).Value


                Regex = New System.Text.RegularExpressions.Regex("(<h4 class=""warnColor1 tCenter mrg_16"">).*?(</h4>)")
                Matches = Regex.Matches(OtvetServera)

                Dim ItemCap As New Item
                With ItemCap
                    .Name = "Capcha"
                    .SearchOn = "Введите код"
                    .Link = "RuTr_Capcha_Key"
                    .Description = Matches(0).Value & "<img src=""" & AdressCapha & """ width=""120"" height=""72"">"
                    .ImageLink = AdressCapha
                End With
                items.Add(ItemCap)

            End If





            PlayList.IsIptv = False
            Return PlayListPlugPar(items, context)
        End Function

        Function SetLogin(context As IPluginContext) As PluginApi.Plugins.Playlist
            Dim items As New System.Collections.Generic.List(Of Item)
            Dim Item As New Item

            With Item
                .Name = "Login"
                .Link = "RuTr_Login"
                .Type = ItemType.DIRECTORY
                .SearchOn = "Login"
                .ImageLink = ICO_Login
            End With
            items.Add(Item)
            PlayList.IsIptv = False
            Return PlayListPlugPar(items, context)
        End Function


        Function SetPassword(context As IPluginContext) As PluginApi.Plugins.Playlist
            Dim items As New System.Collections.Generic.List(Of Item)
            Dim Item As New Item

            Item = New Item
            With Item
                .Name = "Password"
                .Link = "RuTr_Password"
                .Type = ItemType.DIRECTORY
                .SearchOn = "Password"
                .ImageLink = ICO_Password
            End With
            items.Add(Item)
            PlayList.IsIptv = False
            Return PlayListPlugPar(items, context)
        End Function
#End Region
        Public Function GetTorrentPageRuTr(context As IPluginContext, ByVal URL As String) As PluginApi.Plugins.Playlist

            Dim RequestGet As System.Net.WebRequest = Net.WebRequest.Create(URL)
            If ProxyEnablerRuTr = True Then RequestGet.Proxy = New System.Net.WebProxy(ProxyServr, ProxyPort)
            RequestGet.Method = "GET"
            RequestGet.Headers.Add("Cookie", Cookies)

            Dim Response As Net.WebResponse = RequestGet.GetResponse
            Dim dataStream As System.IO.Stream = Response.GetResponseStream()
            Dim reader As New System.IO.StreamReader(dataStream, Text.Encoding.GetEncoding(1251))
            Dim responseFromServer As String = reader.ReadToEnd
            reader.Close()
            dataStream.Close()
            Response.Close()



            Dim Regex As New System.Text.RegularExpressions.Regex("(?<=<p><a href="").*?(?="")")
            Dim TorrentPath As String = TrackerServer & "/forum/" & Regex.Matches(responseFromServer)(0).Value


            Dim RequestTorrent As System.Net.WebRequest = Net.WebRequest.Create(TorrentPath)
            If ProxyEnablerRuTr = True Then RequestTorrent.Proxy = New System.Net.WebProxy(ProxyServr, ProxyPort)
            RequestTorrent.Method = "GET"
            RequestTorrent.Headers.Add("Cookie", Cookies)

            Response = RequestTorrent.GetResponse
            dataStream = Response.GetResponseStream()
            reader = New System.IO.StreamReader(dataStream, System.Text.Encoding.GetEncoding(1251))
            Dim FileTorrent As String = reader.ReadToEnd
            System.IO.File.WriteAllText(System.IO.Path.GetTempPath & "TorrentTemp.torrent", FileTorrent, System.Text.Encoding.GetEncoding(1251))
            reader.Close()
            dataStream.Close()
            Response.Close()


            Dim PlayListtoTorrent() As TorrentPlayList = GetFileList(System.IO.Path.GetTempPath & "TorrentTemp.torrent")

            Dim items As New System.Collections.Generic.List(Of Item)

            Dim Description As String = FormatDescriptionFileRuTr(responseFromServer)
            For Each PlayListItem As TorrentPlayList In PlayListtoTorrent

                Dim Item As New Item
                With Item
                    .Name = PlayListItem.Name
                    .ImageLink = "http://s1.iconbird.com/ico/1012/AmpolaIcons/w256h2561350597291videofile.png"
                    .Link = PlayListItem.Link
                    .Type = ItemType.FILE
                    .Description = Description
                End With
                items.Add(Item)
            Next

            Return PlayListPlugPar(items, context)
        End Function

        Function FormatDescriptionFileRuTr(ByVal HTML As String) As String

            HTML = HTML.Replace(Microsoft.VisualBasic.vbLf, "")

            Dim Title As String
            Try
                Dim Regex As New System.Text.RegularExpressions.Regex("(?<=<title>).*?(</title>)")
                Title = Regex.Matches(HTML)(0).Value
            Catch ex As Exception
                Title = ex.Message
            End Try


            Dim SidsPirs As String
            Try
                Dim Regex As New System.Text.RegularExpressions.Regex("(<td class=""borderless bCenter pad_8"">).*?(?=</div>)")
                SidsPirs = "<p>" & Regex.Matches(HTML)(0).Value
            Catch ex As Exception
                SidsPirs = ex.Message
            End Try


            Dim ImagePath As String
            Try
                Dim Regex As New System.Text.RegularExpressions.Regex("(?<=<var class=""postImg postImgAligned img-right"" title="").*?(?="">)")
                ImagePath = Regex.Matches(HTML)(0).Value
            Catch ex As Exception
            End Try


            Dim InfoFile As String
            Try
                Dim Regex As New System.Text.RegularExpressions.Regex("(<span class=""post-b"">).*(?=<div class=""sp-wrap"">)")
                InfoFile = Regex.Matches(HTML)(0).Value
            Catch ex As Exception
                InfoFile = ex.Message
            End Try

            Return "<html><captionn align=""left"" valign=""top""><strong>" & Title & "</strong></caption><style>.leftimg {float:left;margin: 7px 7px 7px 0;</style>" & SidsPirs & "<table><tbody><tr><th align=""left"" width=""220"" valign=""top""><img src=""" & ImagePath & """width=""220"" height=""290"" class=""leftimg"" <font face=""Arial Narrow"" size=""3"">" & InfoFile & "</tr></tbody></table><div></font></div></html>"

        End Function


        Public Function GetTopListRuTr(context As IPluginContext) As PluginApi.Plugins.Playlist
            If AuthorizationTest() = False Then Return SetLogin(context)

            Dim items As New System.Collections.Generic.List(Of Item)
            Dim Item As New Item

            With Item
                .Name = "Поиск"
                .Link = "Search_rutracker"
                .Type = ItemType.DIRECTORY
                .SearchOn = "Поик на RuTracker"
                .ImageLink = ICO_Search
                .Description = "<html><font face=""Arial"" size=""5""><b>" & .Name & "</font></b><p><img src=""https://rutrk.org/logo/logo.gif"" /> <p>" & UserAuthorization
            End With
            items.Add(Item)

            Item = New Item
            With Item
                .Name = ""
                .Link = ""
                .Type = ItemType.FILE
                .ImageLink = ICO_Pusto
            End With
            items.Add(Item)

            Item = New Item
            With Item
                .Name = "Выйти с RuTracker"
                .Link = "RuTrNonAuthorization"
                .Type = ItemType.DIRECTORY
                .ImageLink = ICO_Delete
            End With
            items.Add(Item)

            PlayList.IsIptv = False
            Return PlayListPlugPar(items, context)
        End Function

        Public Function SearchListRuTr(context As IPluginContext, ByVal search As String) As PluginApi.Plugins.Playlist
            'Dim Kino As String = "100,101,1105,1165,1213,1235,124,1245,1246,1247,1248,1250,1386,1387,1388,1389,1390,1391,1478,1543,1576,1577,1642,1666,1670,185,187,1900,1991,208,209,2090,2091,2092,2093,2097,2109,212,2198,2199,22,2200,2201,2220,2221,2258,2339,2343,2365,2459,2491,2540,281,282,312,313,33,352,376,4,404,484,505,511,514,521,539,549,572,599,656,7,709,822,893,905,921,922,923,924,925,926,927,928,93,930,934,941"
            'Dim Dokumentals As String = ",103,1114,113,114,115,1186,1278,1280,1281,1327,1332,137,1453,1467,1468,1469,1475,1481,1482,1484,1485,1495,1569,1959,2076,2107,2110,2112,2123,2159,2160,2163,2164,2166,2168,2169,2176,2177,2178,2323,2380,24,249,251,2537,2538,294,314,373,393,46,500,532,552,56,670,671,672,752,821,827,851,876,882,939,97,979,98"
            'Dim Sport As String = ",1188,126,1287,1319,1323,1326,1343,1470,1491,1527,1551,1608,1610,1613,1614,1615,1616,1617,1620,1621,1623,1630,1667,1668,1675,1697,1952,1986,1987,1997,1998,2001,2002,2003,2004,2005,2006,2007,2008,2009,2010,2014,2069,2073,2075,2079,2111,2124,2171,2425,2514,255,256,257,259,260,262,263,283,343,486,528,550,626,660,751,845,854,875,978"
            'Dim RequestPost As System.Net.WebRequest = System.Net.WebRequest.Create(TrackerServer & "/forum/tracker.php?f=" & Kino & Dokumentals & Sport & "&nm=" & search)
            Dim RequestPost As System.Net.WebRequest = System.Net.WebRequest.Create(TrackerServer & "/forum/tracker.php?nm=" & search)

            If ProxyEnablerRuTr = True Then RequestPost.Proxy = New System.Net.WebProxy(ProxyServr, ProxyPort)
            RequestPost.Method = "POST"
            RequestPost.ContentType = "text/html; charset=windows-1251"
            RequestPost.Headers.Add("Cookie", Cookies)
            RequestPost.ContentType = "application/x-www-form-urlencoded"
            Dim myStream As System.IO.Stream = RequestPost.GetRequestStream
            Dim DataStr As String = "prev_new=0&prev_oop=0&o=10&s=2&pn=&nm=" & search
            Dim DataByte() As Byte = System.Text.Encoding.GetEncoding(1251).GetBytes(DataStr)
            myStream.Write(DataByte, 0, DataByte.Length)
            myStream.Close()

            Dim Response As System.Net.WebResponse = RequestPost.GetResponse
            Dim dataStream As System.IO.Stream = Response.GetResponseStream
            Dim reader As New System.IO.StreamReader(dataStream, System.Text.Encoding.GetEncoding(1251))
            Dim ResponseFromServer As String = reader.ReadToEnd()


            Dim items As New System.Collections.Generic.List(Of Item)
            Dim Regex As New System.Text.RegularExpressions.Regex("(<tr class=""tCenter hl-tr"">).*?(</tr>)")
            Dim Result As System.Text.RegularExpressions.MatchCollection = Regex.Matches(ResponseFromServer.Replace(Microsoft.VisualBasic.vbLf, " "))

            If Result.Count > 0 Then

                For Each Match As System.Text.RegularExpressions.Match In Result
                    Dim Item As New Item
                    Regex = New System.Text.RegularExpressions.Regex("(?<=<a data-topic_id="").*?(?="")")
                    Dim LinkID As String = Regex.Matches(Match.Value)(0).Value
                    Item.Link = TrackerServer & "/forum/viewtopic.php?t=" & LinkID & ";PAGEFILMRUTR"
                    Regex = New System.Text.RegularExpressions.Regex("(?<=" & LinkID & """>).*?(?=</a>)")
                    Item.Name = Regex.Matches(Match.Value)(0).Value
                    Item.ImageLink = "http://s1.iconbird.com/ico/1012/AmpolaIcons/w256h2561350597291utorrent2.png"
                    Item.Description = GetDescriptionSearhRuTr(Match.Value, Item.Name, LinkID)
                    items.Add(Item)
                Next
            Else
                Dim Item As New Item
                Item.Name = "Ничего не найдено"
                Item.Link = ""

                items.Add(Item)
            End If

            Return PlayListPlugPar(items, context)
        End Function

        Function GetDescriptionSearhRuTr(ByVal HTML As String, ByVal NameFilm As String, ByVal LinkID As String) As String

            Dim SizeFile As String
            Try
                Dim Regex As New System.Text.RegularExpressions.Regex("(?<=" & LinkID & """>).*?(?=<)")
                SizeFile = "<p> Размер: <b>" & Regex.Matches(HTML)(1).Value & "</b>"
            Catch ex As Exception
            End Try

            Dim DobavlenFile As String
            Try
                Dim Regex As New System.Text.RegularExpressions.Regex("(?<=<span class=""tor-icon tor-approved"">&radic;</span></span>&nbsp;).*?(?=</p>)")
                DobavlenFile = "<p><b>" & Regex.Matches(HTML)(0).Value & "</b>"
            Catch ex As Exception
            End Try

            Dim Seeders As String
            Try
                Dim Regex As New System.Text.RegularExpressions.Regex("(?<=title=""Сидов"">).*?(?=<)")
                Seeders = "<p> Seeders: <b> " & Regex.Matches(HTML)(0).Value & "</b>"
            Catch ex As Exception
            End Try

            Dim Leechers As String
            Try
                Dim Regex As New System.Text.RegularExpressions.Regex("(?<=title=""Личей"">).*?(?=<)")
                Leechers = "<p> Leechers: <b> " & Regex.Matches(HTML)(0).Value & "</b>"
            Catch ex As Exception
            End Try

            Return "<html><font face=""Arial"" size=""5""><b>" & NameFilm & "</font></b><p><font face=""Arial Narrow"" size=""4"">" & SizeFile & DobavlenFile & Seeders & Leechers & "</font></html>"
        End Function

        Function PlayListPlugPar(ByVal items As System.Collections.Generic.List(Of Item), ByVal context As IPluginContext, Optional ByVal next_page_url As String = "") As PluginApi.Plugins.Playlist
            If next_page_url <> "" Then
                Dim pluginParams = New NameValueCollection()
                pluginParams(PLUGIN_PATH) = next_page_url
                PlayList.NextPageUrl = context.CreatePluginUrl(pluginParams)
            End If
            PlayList.Timeout = "60" 'sec

            PlayList.Items = items.ToArray()
            For Each Item As Item In PlayList.Items
                If ItemType.DIRECTORY = Item.Type Then
                    Dim pluginParams = New NameValueCollection()
                    pluginParams(PLUGIN_PATH) = Item.Link
                    Item.Link = context.CreatePluginUrl(pluginParams)
                End If
            Next
            Return PlayList
        End Function
#Region "AceTorrent"
        Structure TorrentPlayList
            Dim IDX As String
            Dim Name As String
            Dim Link As String
            Dim Description As String
            Dim ImageLink As String
        End Structure


        Function GetID(ByVal PathTorrent As String) As String
            Dim WC As New System.Net.WebClient
            WC.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:47.0) Gecko/20100101 Firefox/47.0")
            WC.Encoding = System.Text.Encoding.UTF8
            Dim FileTorrent() As Byte = WC.DownloadData(PathTorrent)

            Dim FileTorrentString As String = System.Convert.ToBase64String(FileTorrent)
            FileTorrent = System.Text.Encoding.Default.GetBytes(FileTorrentString)

            Dim request As System.Net.HttpWebRequest = System.Net.HttpWebRequest.Create("http://api.torrentstream.net/upload/raw")
            request.Method = "POST"
            request.ContentType = "application/octet-stream\r\n"
            request.ContentLength = FileTorrent.Length
            Dim dataStream As System.IO.Stream = request.GetRequestStream
            dataStream.Write(FileTorrent, 0, FileTorrent.Length)
            dataStream.Close()

            Dim response As System.Net.HttpWebResponse = request.GetResponse()
            dataStream = response.GetResponseStream()
            Dim reader As New System.IO.StreamReader(dataStream)
            Dim responseFromServer As String = reader.ReadToEnd()

            Dim responseSplit() As String = responseFromServer.Split("""")
            Dim ID As String = responseSplit(3)
            Return ID
        End Function
        Dim FunctionsGetTorrentPlayList As String = "GetFileListM3U"
        Function GetFileList(ByVal PathTorrent As String) As TorrentPlayList()

            Dim WC As New System.Net.WebClient
            WC.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:47.0) Gecko/20100101 Firefox/47.0")
            WC.Encoding = System.Text.Encoding.UTF8

            Dim ID As String = GetID(PathTorrent)
            Dim PlayListTorrent() As TorrentPlayList = Nothing
            Dim AceMadiaInfo As String

            Select Case FunctionsGetTorrentPlayList
                Case "GetFileListJSON"
GetFileListJSON:
                    Dim CodeZnaki() As String = {"\U0430", "\U0431", "\U0432", "\U0433", "\U0434", "\U0435", "\U0451", "\U0436", "\U0437", "\U0438", "\U0439", "\U043A", "\U043B", "\U043C", "\U043D", "\U043E", "\U043F", "\U0440", "\U0441", "\U0442", "\U0443",
                    "\U0444", "\U0445", "\U0446", "\U0447", "\U0448", "\U0449", "\U044A", "\U044B", "\U044C", "\U044D", "\U044E", "\U044F", "\U0410", "\U0411", "\U0412", "\U0413", "\U0414", "\U0415", "\U0401", "\U0416", "\U0417", "\U0418", "\U0419", "\U041A",
                    "\U041B", "\U041C", "\U041D", "\U041E", "\U041F", "\U0420", "\U0421", "\U0422", "\U0423", "\U0424", "\U0425", "\U0426", "\U0427", "\U0428", "\U0429", "\U042A", "\U042B", "\U042C", "\U042D", "\U042E", "\U042F", "\U00AB", "\U00BB", "U2116"}
                    Dim DecodeZnaki() As String = {"а", "б", "в", "г", "д", "е", "ё", "ж", "з", "и", "й", "к", "л", "м", "н", "о", "п", "р", "с", "т", "у", "ф", "х", "ц", "ч", "ш", "щ", "ъ", "ы", "ь", "э", "ю", "я",
                    "А", "Б", "В", "Г", "Д", "Е", "Ё", "Ж", "З", "И", "Й", "К", "Л", "М", "Н", "О", "П", "Р", "С", "Т", "У", "Ф", "Х", "Ц", "Ч", "Ш", "Щ", "Ъ", "Ы", "Ь", "Э", "Ю", "Я", "«", "»", "№"}

                    AceMadiaInfo = WC.DownloadString("http://" & IPAdress & ":" & PortAce & "/server/api?method=get_media_files&content_id=" & ID)
                    For I As Integer = 0 To 68
                        AceMadiaInfo = Microsoft.VisualBasic.Strings.Replace(AceMadiaInfo, Microsoft.VisualBasic.Strings.LCase(CodeZnaki(I)), DecodeZnaki(I))
                    Next
                    WC.Dispose()

                    Dim PlayListJson As String = AceMadiaInfo
                    PlayListJson = Microsoft.VisualBasic.Strings.Replace(PlayListJson, ",", Nothing)
                    PlayListJson = Microsoft.VisualBasic.Strings.Replace(PlayListJson, ":", Nothing)
                    PlayListJson = Microsoft.VisualBasic.Strings.Replace(PlayListJson, "}", Nothing)
                    PlayListJson = Microsoft.VisualBasic.Strings.Replace(PlayListJson, "{", Nothing)
                    PlayListJson = Microsoft.VisualBasic.Strings.Replace(PlayListJson, "result", Nothing)
                    PlayListJson = Microsoft.VisualBasic.Strings.Replace(PlayListJson, "error", Nothing)
                    PlayListJson = Microsoft.VisualBasic.Strings.Replace(PlayListJson, "null", Nothing)
                    PlayListJson = Microsoft.VisualBasic.Strings.Replace(PlayListJson, """""", """")
                    PlayListJson = Microsoft.VisualBasic.Strings.Replace(PlayListJson, """ """, """")



                    Dim ListSplit() As String = PlayListJson.Split("""")

                    ReDim PlayListTorrent((ListSplit.Length / 2) - 2)

                    Dim N As Integer
                    For I As Integer = 1 To ListSplit.Length - 2
                        PlayListTorrent(N).IDX = ListSplit(I)
                        PlayListTorrent(N).Name = ListSplit(I + 1)
                        PlayListTorrent(N).Link = "http://" & IPAdress & ":" & PortAce & "/ace/getstream?id=" & ID & "&_idx=" & PlayListTorrent(N).IDX
                        PlayListTorrent(N).ImageLink = ICO_VideoFile
                        I += 1
                        N += 1
                    Next


                Case "GetFileListM3U"
                    AceMadiaInfo = WC.DownloadString("http://" & IPAdress & ":" & PortAce & "/ace/manifest.m3u8?id=" & ID & "&format=json&use_api_events=1&use_stop_notifications=1")
                    If AceMadiaInfo.StartsWith("{""response"": {""event_url"": """) = True Then
                        GoTo GetFileListJSON
                    End If
                    If AceMadiaInfo.StartsWith("{""response"": null, ""error"": """) = True Then
                        ReDim PlayListTorrent(0)
                        PlayListTorrent(0).Name = "ОШИБКА: " & New System.Text.RegularExpressions.Regex("(?<={""response"": null, ""error"": "").*?(?="")").Matches(AceMadiaInfo)(0).Value
                        PlayListTorrent(0).ImageLink = ICO_Error
                        Return PlayListTorrent
                    End If

                    '"Получение потока в формате HLS
                    AceMadiaInfo = WC.DownloadString("http://" & IPAdress & ":" & PortAce & "/ace/manifest.m3u8?id=" & ID)
                    Dim Regex As New System.Text.RegularExpressions.Regex("(?<=EXTINF:-1,).*(.*)")
                    Dim Itog As System.Text.RegularExpressions.MatchCollection = Regex.Matches(AceMadiaInfo)

                    ReDim PlayListTorrent(Itog.Count - 1)
                    Dim N As Integer
                    For Each Match As System.Text.RegularExpressions.Match In Itog
                        PlayListTorrent(N).Name = Match.Value
                        PlayListTorrent(N).ImageLink = ICO_VideoFile
                        N += 1
                    Next

                    N = 0
                    Regex = New System.Text.RegularExpressions.Regex("(http:).*(?=.*?)")
                    Itog = Regex.Matches(AceMadiaInfo)
                    For Each Match As System.Text.RegularExpressions.Match In Itog
                        PlayListTorrent(N).Link = Match.Value
                        N += 1
                    Next
            End Select


            Return PlayListTorrent
        End Function


#End Region

    End Class
End Namespace