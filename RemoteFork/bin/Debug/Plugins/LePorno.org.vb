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

    <PluginAttribute(Id:="leporno", Version:="0.1", Author:="ORAMAN", Name:="LePorno", Description:="", ImageLink:="http://leporno.org/images/logo/logo.jpg")>
    Public Class LePorno
        Implements IPlugin

        Dim IPAdress As String
        Dim PortRemoteFork As String = "8027"
        Dim PortAce As String = "6878"
        Dim PlayList As New PluginApi.Plugins.Playlist
        Dim next_page_url As String
        Dim AceProxEnabl As Boolean
        Dim PLUGIN_PATH As String = "pluginPath"
        Dim ProxyServr As String = "proxy.antizapret.prostovpn.org"
        Dim ProxyPort As Integer = 3128
        Dim ProxyEnablerRuTr As Boolean = False
        Dim TrackerServer As String = "http://leporno.org"
        Dim Cookies As String = "__atuvc=1%7C12; __atuvs=58d086844135367c000; __lnkrntdmcvrd=-1; lporg_data=a%3A3%3A%7Bs%3A2%3A%22uk%22%3Bs%3A12%3A%22jOK1f9tGdb18%22%3Bs%3A3%3A%22uid%22%3Bi%3A610574%3Bs%3A3%3A%22sid%22%3Bs%3A20%3A%22uAkgLWyIT0fm0Fxw1ncS%22%3B%7D; lporg_isl=1490061422; _gat=1; _ga=GA1.2.1286628131.1490060932"

        Dim ICO_Folder As String = "http://s1.iconbird.com/ico/1012/AmpolaIcons/w256h2561350597246folder.png"
        Dim ICO_Settings As String = "http://s1.iconbird.com/ico/1112/DiagramPreview/w128h1281354120955diagram45.png"
        Dim ICO_Settings2 As String = "http://s1.iconbird.com/ico/2013/7/395/w128h1281374340707Settings.PNG"
        Dim ICO_SettingsFolder As String = "http://s1.iconbird.com/ico/2013/6/355/w128h1281372334739settings.png"
        Dim ICO_SettingsParam As String = "http://s1.iconbird.com/ico/1212/Smilebyjordanfc/w90h901355053543setting.png"
        Dim ICO_VideoFile As String = "http://s1.iconbird.com/ico/1012/AmpolaIcons/w256h2561350597291videofile.png"
        Dim ICO_MusicFile As String = "http://s1.iconbird.com/ico/1012/AmpolaIcons/w256h2561350597240aimp.png"
        Dim ICO_TorrentFile As String = "http://s1.iconbird.com/ico/1012/AmpolaIcons/w256h2561350597291utorrent2.png"
        Dim ICO_ImageFile As String = "http://s1.iconbird.com/ico/1012/AmpolaIcons/w256h2561350597278imagefile.png"
        Dim ICO_M3UFile As String = "http://s1.iconbird.com/ico/0912/VannillACreamIconSet/w128h1281348320736M3U.png"
        Dim ICO_NNMClub As String = "http://s1.iconbird.com/ico/0912/MorphoButterfly/w128h1281348669898RhetenorMorpho.png"
        Dim ICO_Search As String = "http://s1.iconbird.com/ico/0612/MustHave/w256h2561339195991Search256x256.png"
        Dim ICO_Error As String = "http://s1.iconbird.com/ico/0912/ToolbarIcons/w256h2561346685474SymbolError.png"
        Dim ICO_Save As String = "http://s1.iconbird.com/ico/2013/6/355/w128h1281372334742check.png"
        Dim ICO_Delete As String = "http://s1.iconbird.com/ico/2013/6/355/w128h1281372334742delete.png"
        Dim ICO_Pusto As String = "https://avatanplus.com/files/resources/mid/5788db3ecaa49155ee986d6e.png"

        Public Function GetList(context As IPluginContext) As Playlist Implements IPlugin.GetList
            IPAdress = context.GetRequestParams.Get("host").Split(":")(0)

            Dim path = context.GetRequestParams().Get(PLUGIN_PATH)
            path = (If((path Is Nothing), "plugin", "plugin;" & path))





            Select Case path
                Case "plugin"
                    Return GetTopList(context)
            End Select

            Dim PathSpliter() As String = path.Split(";")

            If context.GetRequestParams("search") <> Nothing Then
                Search__Param = PathSpliter(PathSpliter.Length - 1)
                Return SearchList(context, context.GetRequestParams("search"))
            End If

            Select Case PathSpliter(PathSpliter.Length - 1)
                'Case "PAGERUTR"
                '    Return GetPAGERUTR(context, PathSpliter(PathSpliter.Length - 2))
                Case "PAGEFILM"
                    Return GetTorrentPage(context, PathSpliter(PathSpliter.Length - 2))
            End Select



            Return GetTopList(context)
        End Function

        Public Function GetTorrentPage(context As IPluginContext, ByVal URL As String) As PluginApi.Plugins.Playlist

            Dim RequestGet As System.Net.WebRequest = Net.WebRequest.Create(URL)
            If ProxyEnablerRuTr = True Then RequestGet.Proxy = New System.Net.WebProxy(ProxyServr, ProxyPort)
            RequestGet.Method = "GET"
            RequestGet.Headers.Add("Cookie", Cookies)

            Dim Response As Net.WebResponse = RequestGet.GetResponse
            Dim dataStream As System.IO.Stream = Response.GetResponseStream()
            Dim reader As New System.IO.StreamReader(dataStream, Text.Encoding.UTF8)
            Dim responseFromServer As String = reader.ReadToEnd
            reader.Close()
            dataStream.Close()
            Response.Close()


            Dim Regex As New System.Text.RegularExpressions.Regex("(save.php).*?(?="")")

            Dim TorrentPath As String = TrackerServer & "/" & Regex.Matches(responseFromServer)(0).Value



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

            Dim Description As String = FormatDescriptionFile(responseFromServer)
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

        Function FormatDescriptionFile(ByVal HTML As String) As String


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
                Dim Regex As New System.Text.RegularExpressions.Regex("(<td colspan=""2"" class=""catTitle"">).*?(?=<td colspan=""2"" class=""row2 pad_2"">)")
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

        Public Function GetTopList(context As IPluginContext) As PluginApi.Plugins.Playlist

            Dim items As New System.Collections.Generic.List(Of Item)
            Dim Item As New Item
            With Item
                .Name = "Поиск"
                .Link = "Search_all"
                .Type = ItemType.DIRECTORY
                .SearchOn = "Поиск"
                .ImageLink = ICO_Search
                .Description = "<html><font face=""Arial"" size=""5""><b>" & .Name & "</font></b><p><img src=""http://leporno.org/images/logo/logo.jpg"" />"
            End With
            items.Add(Item)
            Return PlayListPlugPar(items, context)
        End Function
        Dim Search__Param As String
        Public Function SearchList(context As IPluginContext, ByVal search As String) As PluginApi.Plugins.Playlist
            Dim items As New System.Collections.Generic.List(Of Item)
            Try
                Dim RequestPost As System.Net.HttpWebRequest = System.Net.HttpWebRequest.Create(TrackerServer & "/tracker.php#results")

                If ProxyEnablerRuTr = True Then RequestPost.Proxy = New System.Net.WebProxy(ProxyServr, ProxyPort)
                RequestPost.Method = "POST"
                RequestPost.ContentType = "text/html; charset=UTF8"
                RequestPost.Headers.Add("Cookie", Cookies)
                RequestPost.ContentType = "application/x-www-form-urlencoded"
                RequestPost.KeepAlive = True
                RequestPost.AllowAutoRedirect = False
                Dim myStream As System.IO.Stream = RequestPost.GetRequestStream

                Dim DataStr As String
                Select Case Search__Param
                    Case "Search_all"
                        DataStr = "prev_allw=1&prev_a=0&prev_dla=0&prev_dlc=0&prev_dld=0&prev_dlw=0&prev_my=0&prev_new=0&prev_sd=0&prev_da=1&prev_dc=0&prev_df=1&prev_ds=0&prev_tor_type=0&f[]=-1&o=10&s=2&tm=-1&sns=-1&df=1&da=1&pn=&nm=" & search & "&allw=1&submit= Поиск"
                End Select

                Dim DataByte() As Byte = System.Text.Encoding.UTF8.GetBytes(DataStr)
                myStream.Write(DataByte, 0, DataByte.Length)
                myStream.Close()

                Dim Response As System.Net.WebResponse = RequestPost.GetResponse
                Dim dataStream As System.IO.Stream = Response.GetResponseStream
                Dim reader As New System.IO.StreamReader(dataStream, System.Text.Encoding.UTF8)
                Dim ResponseFromServer As String = reader.ReadToEnd()
                '  IO.File.WriteAllText("d:\My Desktop\ht.html", ResponseFromServer, System.Text.Encoding.GetEncoding(1251))


                Dim Regex As New System.Text.RegularExpressions.Regex("(<tr class=""tCenter hl-tr"").*?(</tr>)")
                Dim Result As System.Text.RegularExpressions.MatchCollection = Regex.Matches(ResponseFromServer.Replace(Microsoft.VisualBasic.vbLf, " "))

                If Result.Count > 0 Then

                    For Each Match As System.Text.RegularExpressions.Match In Result
                        Dim Item As New Item
                        Regex = New System.Text.RegularExpressions.Regex("(<a class=""genmed tLink"").*?(</a>)")
                        Dim Stroka As String = Regex.Matches(Match.Value)(0).Value
                        Regex = New System.Text.RegularExpressions.Regex("(?<=href="".).*?(?="")")
                        Dim LinkID As String = TrackerServer & Regex.Matches(Stroka)(0).Value
                        Item.Link = LinkID & ";PAGEFILM"
                        Regex = New System.Text.RegularExpressions.Regex("(?<=<b>).*?(?=</b>)")
                        Item.Name = Regex.Matches(Stroka)(0).Value
                        Item.ImageLink = "http://s1.iconbird.com/ico/1012/AmpolaIcons/w256h2561350597291utorrent2.png"
                        ' Item.Description = GetDescriptionSearh(Match.Value, Item.Name, LinkID)
                        Item.Description = Match.Value.Replace("./templates/default/images/icon_minipost.gif", "http://leporno.org/images/logo/logo.jpg").Replace("alt=""Пост"" />", "alt=""Пост"" /><p>")
                        items.Add(Item)
                    Next
                Else
                    Dim Item As New Item
                    Item.Name = "Ничего не найдено"
                    Item.Type = ItemType.FILE
                    Item.ImageLink = ICO_Pusto

                    items.Add(Item)
                End If


            Catch ex As Exception
                Dim Item As New Item
                Item.Name = "Ошибка"
                Item.Type = ItemType.FILE
                Item.ImageLink = ICO_Pusto
                Item.Description = ex.Message
                items.Add(Item)
            End Try

            Return PlayListPlugPar(items, context)
        End Function

        Function GetDescriptionSearh(ByVal HTML As String, ByVal NameFilm As String, ByVal LinkID As String) As String
            IO.File.WriteAllText("d:\My Desktop\ht.html", HTML, System.Text.Encoding.UTF8)
            Dim SizeFile As String
            Try
                Dim Regex As New System.Text.RegularExpressions.Regex("(?<="">).*?(?=</a>)")
                SizeFile = "<p> Размер: <b>" & Regex.Matches(HTML)(4).Value & "</b>"
            Catch ex As Exception
            End Try

            Dim DobavlenFile As String
            Try
                Dim Regex As New System.Text.RegularExpressions.Regex("(<td class=""row4 small nowrap"").*(</td>)")
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
                Playlist.NextPageUrl = context.CreatePluginUrl(pluginParams)
            End If
            Playlist.Timeout = "60" 'sec

            Playlist.Items = items.ToArray()
            For Each Item As Item In Playlist.Items
                If ItemType.DIRECTORY = Item.Type Then
                    Dim pluginParams = New NameValueCollection()
                    pluginParams(PLUGIN_PATH) = Item.Link
                    Item.Link = context.CreatePluginUrl(pluginParams)
                End If
            Next
            Return Playlist
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
GetFileListJSON:    Dim CodeZnaki() As String = {"\U0430", "\U0431", "\U0432", "\U0433", "\U0434", "\U0435", "\U0451", "\U0436", "\U0437", "\U0438", "\U0439", "\U043A", "\U043B", "\U043C", "\U043D", "\U043E", "\U043F", "\U0440", "\U0441", "\U0442", "\U0443",
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
