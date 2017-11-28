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

    <PluginAttribute(Id:="rutor", Version:="0.1", Author:="ORAMAN", Name:="RuTor", Description:="", ImageLink:="http://mega-tor.org/s/logo.jpg")>
    Public Class RuTor
        Implements IPlugin
#Region "Иконки"
        Dim LOGO_Tracker As String = "/s/logo.jpg"
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
        Dim ICO_Search2 As String = "http://s1.iconbird.com/ico/0912/MetroUIDock/w512h5121347464996Search.png"
        Dim ICO_Error As String = "http://s1.iconbird.com/ico/0912/ToolbarIcons/w256h2561346685474SymbolError.png"
        Dim ICO_Error2 As String = "http://errorfix48.ru/uploads/posts/2014-09/1409846068_400px-warning_icon.png"
        Dim ICO_Save As String = "http://s1.iconbird.com/ico/2013/6/355/w128h1281372334742check.png"
        Dim ICO_Delete As String = "http://s1.iconbird.com/ico/2013/6/355/w128h1281372334742delete.png"
        Dim ICO_Pusto As String = "https://avatanplus.com/files/resources/mid/5788db3ecaa49155ee986d6e.png"
        Dim ICO_Login As String = "http://s1.iconbird.com/ico/1012/AmpolaIcons/w256h2561350597246portal2.png"
        Dim ICO_Password As String = "http://s1.iconbird.com/ico/0612/GooglePlusInterfaceIcons/w128h1281338911371password.png"
        Dim ICO_LoginKey As String = "http://s1.iconbird.com/ico/0912/ToolbarIcons/w256h2561346685464Login.png"
#End Region
        Dim IPAdress As String
        Dim PortRemoteFork As String = "8027"
        Dim PortAce As String = "6878"
        Dim AceProxEnabl As Boolean
        Dim PLUGIN_PATH As String = "pluginPath"
        Dim PlayList As New PluginApi.Plugins.Playlist
        Dim next_page_url As String
        Dim ProxyServr As String = "proxy.antizapret.prostovpn.org"
        Dim ProxyPort As Integer = 3128
        Dim ProxyEnablerRuTor As Boolean = True
        Dim TrackerServerRuTor As String = "http://mega-tor.org"

        Public Function GetList(context As IPluginContext) As Playlist Implements IPlugin.GetList
            IPAdress = context.GetRequestParams.Get("host").Split(":")(0)

            Dim path = context.GetRequestParams().Get(PLUGIN_PATH)
            path = (If((path Is Nothing), "plugin", "plugin;" & path))

            Select Case path
                Case "plugin"
                    Return GetTopListRuTor(context)
            End Select

            Dim PathSpliter() As String = path.Split(";")

            If context.GetRequestParams.Get("search") <> Nothing Then
                Select Case path
                    Case "plugin;Search_RuTor"
                        Return GetPAGERUTOR(context, TrackerServerRuTor & "/search/0/0/100/2/" & context.GetRequestParams()("search"))
                End Select
            End If

            Select Case PathSpliter(PathSpliter.Length - 1)
                Case "PAGERUTOR"
                    Return GetPAGERUTOR(context, PathSpliter(PathSpliter.Length - 2))
                Case "PAGEFILMRUTOR"
                    Return GetTorrentPageRuTor(context, PathSpliter(PathSpliter.Length - 2))

            End Select
            Return Nothing
        End Function



#Region "RuTor"

        Public Function GetTorrentPageRuTor(context As IPluginContext, ByVal URL As String) As PluginApi.Plugins.Playlist
            Dim items As New System.Collections.Generic.List(Of Item)()
            Dim WC As New System.Net.WebClient
            WC.Encoding = System.Text.Encoding.UTF8
            WC.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36")
            Dim ResponseFromServer As String = WC.DownloadString(URL).Replace(vbLf, " ")

            Dim Regex As New System.Text.RegularExpressions.Regex("(/download/).*?(?="">)")
            Dim TorrentPath As String = TrackerServerRuTor & Regex.Matches(ResponseFromServer)(0).Value
            Dim PlayListtoTorrent() As TorrentPlayList = GetFileList(TorrentPath)

            Dim Description As String = FormatDescriptionFileRuTor(ResponseFromServer)
            For Each PlayListItem As TorrentPlayList In PlayListtoTorrent

                Dim Item As New Item
                With Item
                    .Name = PlayListItem.Name
                    .ImageLink = PlayListItem.ImageLink
                    .Link = PlayListItem.Link
                    .Type = ItemType.FILE
                    .Description = Description
                End With
                items.Add(Item)
            Next

            Regex = New System.Text.RegularExpressions.Regex("(Связанные раздачи).*?(Файлы)")
            Dim Matches As System.Text.RegularExpressions.MatchCollection = Regex.Matches(ResponseFromServer)

            If Matches.Count > 0 Then

                Dim Item As New Item
                With Item
                    .Name = "- СВЯЗАННЫЕ РАЗДАЧИ -"
                    .ImageLink = ICO_Pusto
                    .Type = ItemType.FILE
                End With
                items.Add(Item)


                Regex = New System.Text.RegularExpressions.Regex("(?<=<a href="").*?(?="")")
                Dim MatchesSearchNext As System.Text.RegularExpressions.MatchCollection = Regex.Matches(Matches(0).Value)
                Dim ItemSearchNext As New Item
                With ItemSearchNext
                    .ImageLink = ICO_Search2
                    .Name = "Искать ещё похожие раздачи"
                    .Link = TrackerServerRuTor & MatchesSearchNext(MatchesSearchNext.Count - 1).Value & ";PAGERUTOR"
                End With



                Regex = New System.Text.RegularExpressions.Regex("(<a href=""magnet:).*?(</span></td></tr>)")

                Matches = Regex.Matches(Matches(0).Value)

                For Each Macth As System.Text.RegularExpressions.Match In Matches
                    Item = New Item

                    With Item
                        .ImageLink = ICO_TorrentFile
                        Regex = New System.Text.RegularExpressions.Regex("(?<=<a href="").*?(?="">)")
                        .Link = TrackerServerRuTor & Regex.Matches(Macth.Value)(1).Value & ";PAGEFILMRUTOR"

                        Regex = New System.Text.RegularExpressions.Regex("(?<="">).*?(?=</a>)")
                        .Name = Regex.Matches(Macth.Value)(1).Value

                        Regex = New System.Text.RegularExpressions.Regex("(<td align=""right"">).*?(</td>)")
                        Dim MatchSize As System.Text.RegularExpressions.MatchCollection = Regex.Matches(Macth.Value)
                        Dim SizeFile As String = MatchSize(MatchSize.Count - 1).Value
                        SizeFile = "Размер: " & SizeFile

                        Regex = New System.Text.RegularExpressions.Regex("(?<=alt=""S"" />&nbsp;).*?(?=<)")
                        Dim Seeders As String = "Seeders: " & Regex.Matches(Macth.Value)(0).Value


                        Regex = New System.Text.RegularExpressions.Regex("(?<=alt=""L"" /><span class=""red"">&nbsp;).*?(?=</span>)")
                        Dim Leechers As String = "Leechers: " & Regex.Matches(Macth.Value)(0).Value

                        .Description = "</div><span style=""color:#3090F0"">" & .Name & "</span><p><br>" & SizeFile & "<br><p>" & Seeders & "<br>" & Leechers

                    End With
                    items.Add(Item)
                Next
                items.Add(ItemSearchNext)

            End If





            PlayList.IsIptv = "false"
            Return PlayListPlugPar(items, context)
        End Function

        Function FormatDescriptionFileRuTor(ByVal HTML As String) As String


            Dim Title As String = Nothing
            Try
                Dim Regex As New System.Text.RegularExpressions.Regex("(?<=<h1>).*?(?=<h1>)")
                Title = Regex.Matches(HTML)(0).Value
            Catch ex As Exception
                Title = ex.Message
            End Try


            Dim SidsPirs As String = Nothing
            Try
                Dim Regex As New System.Text.RegularExpressions.Regex("(<td class=""header"">Раздают).*?(?=<td class=""header"" nowrap=""nowrap"">Добавить)")
                SidsPirs = Regex.Matches(HTML)(0).Value
            Catch ex As Exception
                SidsPirs = ex.Message
            End Try


            Dim ImagePath As String = Nothing
            Try
                Dim Regex As New System.Text.RegularExpressions.Regex("(?<=""></a><br /><img src="").*?(?="")")
                ImagePath = Regex.Matches(HTML)(0).Value
            Catch ex As Exception
            End Try


            Dim InfoFile As String = Nothing
            Try
                Dim Regex As New System.Text.RegularExpressions.Regex("(<u|<span style=""font-family:Georgia;"">).*?(?=<div)")
                InfoFile = Regex.Matches(HTML)(0).Value
            Catch ex As Exception
                InfoFile = ex.Message
            End Try

            SidsPirs = replacetags(SidsPirs)
            InfoFile = replacetags(InfoFile)
            Title = replacetags(Title)


            Return "<div id=""poster"" style=""float:left;padding:4px;        background-color:#EEEEEE;margin:0px 13px 1px 0px;"">" & "<img src=""" & ImagePath & """ style=""width:180px;float:left;"" /></div><span style=""color:#3090F0"">" & Title & "</span><br><font face=""Arial Narrow"" size=""4"">" & SidsPirs & "</font><br>" & InfoFile

        End Function

        Public Function replacetags(ByVal s As String) As String
            Try
                Dim rgx As New System.Text.RegularExpressions.Regex("<[^b].*?>")
                s = rgx.Replace(s, "").Replace("<b>", "")
                Return s
            Catch ex As Exception

            End Try
            Return Nothing
        End Function

        Public Function GetPAGERUTOR(context As IPluginContext, ByVal URL As String) As PluginApi.Plugins.Playlist

            Dim items As New System.Collections.Generic.List(Of Item)()
            Dim WC As New System.Net.WebClient
            WC.Encoding = System.Text.Encoding.UTF8
            WC.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36")
            Dim ResponseFromServer As String = WC.DownloadString(URL).Replace(vbLf, " ")

            Dim Regex As New System.Text.RegularExpressions.Regex("(<a href=""magnet).*?(</span></td></tr>)")
            Dim Matches As System.Text.RegularExpressions.MatchCollection = Regex.Matches(ResponseFromServer)




            For Each Macth As System.Text.RegularExpressions.Match In Matches
                Dim Item As New Item
                With Item
                    .ImageLink = ICO_TorrentFile

                    Regex = New System.Text.RegularExpressions.Regex("(?<=<a href="").*?(?="">)")
                    .Link = TrackerServerRuTor & Regex.Matches(Macth.Value)(1).Value & ";PAGEFILMRUTOR"

                    Regex = New System.Text.RegularExpressions.Regex("(?<="">).*?(?=</a>)")
                    .Name = Regex.Matches(Macth.Value)(1).Value

                    Regex = New System.Text.RegularExpressions.Regex("(<td align=""right"">).*?(</td>)")
                    Dim MatchSize As System.Text.RegularExpressions.MatchCollection = Regex.Matches(Macth.Value)
                    Dim SizeFile As String = MatchSize(MatchSize.Count - 1).Value
                    SizeFile = "Размер: " & SizeFile

                    Regex = New System.Text.RegularExpressions.Regex("(?<=alt=""S"" />&nbsp;).*?(?=<)")
                    Dim Seeders As String = "Seeders: " & Regex.Matches(Macth.Value)(0).Value


                    Regex = New System.Text.RegularExpressions.Regex("(?<=alt=""L"" /><span class=""red"">&nbsp;).*?(?=</span>)")
                    Dim Leechers As String = "Leechers: " & Regex.Matches(Macth.Value)(0).Value

                    .Description = "</div><span style=""color:#3090F0"">" & .Name & "</span><p><br>" & SizeFile & "<br><p>" & Seeders & "<br>" & Leechers

                End With
                items.Add(Item)
            Next

            Regex = New System.Text.RegularExpressions.Regex("(?<=&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href="").*?(?=""><b>След)")
            Dim MatchNext As System.Text.RegularExpressions.MatchCollection = Regex.Matches(ResponseFromServer)
            If MatchNext.Count > 0 Then next_page_url = TrackerServerRuTor & MatchNext(0).Value & ";PAGERUTOR" Else next_page_url = Nothing

            PlayList.IsIptv = "false"

            Return PlayListPlugPar(items, context, next_page_url)
        End Function

        Public Function GetTopListRuTor(context As IPluginContext) As PluginApi.Plugins.Playlist
            Dim items As New System.Collections.Generic.List(Of Item)
            Dim Item As New Item


            With Item
                .Name = "Поиск"
                .Link = "Search_RuTor"
                .Type = ItemType.DIRECTORY
                .SearchOn = "Поик на RuTor"
                .ImageLink = ICO_Search
                .Description = "<html><font face=""Arial"" size=""5""><b>" & .Name & "</font></b><p><img src=""" & TrackerServerRuTor & LOGO_Tracker & """ />"
                items.Add(Item)
            End With

            Item = New Item
            With Item
                .Name = "Торренты за последние 24 часа"
                .Link = TrackerServerRuTor & "/;PAGERUTOR"
                .ImageLink = ICO_Folder
                .Description = "<html><font face=""Arial"" size=""5""><b>" & .Name & "</font></b><p><img src=""" & TrackerServerRuTor & LOGO_Tracker & """ />"
                items.Add(Item)
            End With

            Item = New Item
            With Item
                .Name = "Зарубежные фильмы"
                .Link = TrackerServerRuTor & "/browse/0/1/0/0/;PAGERUTOR"
                .ImageLink = ICO_Folder
                .Description = "<html><font face=""Arial"" size=""5""><b>" & .Name & "</font></b><p><img src=""" & TrackerServerRuTor & LOGO_Tracker & """ />"
                items.Add(Item)
            End With

            Item = New Item
            With Item
                .Name = "Наши фильмы"
                .Link = TrackerServerRuTor & "/browse/0/5/0/0/;PAGERUTOR"
                .ImageLink = ICO_Folder
                .Description = "<html><font face=""Arial"" size=""5""><b>" & .Name & "</font></b><p><img src=""" & TrackerServerRuTor & LOGO_Tracker & """ />"
                items.Add(Item)
            End With


            Item = New Item
            With Item
                .Name = "Научно-популярные фильмы"
                .Link = TrackerServerRuTor & "/browse/0/12/0/0/;PAGERUTOR"
                .ImageLink = ICO_Folder
                .Description = "<html><font face=""Arial"" size=""5""><b>" & .Name & "</font></b><p><img src=""" & TrackerServerRuTor & LOGO_Tracker & """ />"
                items.Add(Item)
            End With


            Item = New Item
            With Item
                .Name = "Сериалы"
                .Link = TrackerServerRuTor & "/browse/0/4/0/0/;PAGERUTOR"
                .ImageLink = ICO_Folder
                .Description = "<html><font face=""Arial"" size=""5""><b>" & .Name & "</font></b><p><img src=""" & TrackerServerRuTor & LOGO_Tracker & """ />"
                items.Add(Item)
            End With

            Item = New Item
            With Item
                .Name = "Телевизор"
                .Link = TrackerServerRuTor & "/browse/0/6/0/0/;PAGERUTOR"
                .ImageLink = ICO_Folder
                .Description = "<html><font face=""Arial"" size=""5""><b>" & .Name & "</font></b><p><img src=""" & TrackerServerRuTor & LOGO_Tracker & """ />"
                items.Add(Item)
            End With


            Item = New Item
            With Item
                .Name = "Мультипликация"
                .Link = TrackerServerRuTor & "/browse/0/7/0/0/;PAGERUTOR"
                .ImageLink = ICO_Folder
                .Description = "<html><font face=""Arial"" size=""5""><b>" & .Name & "</font></b><p><img src=""" & TrackerServerRuTor & LOGO_Tracker & """ />"
                items.Add(Item)
            End With

            Item = New Item
            With Item
                .Name = "Аниме"
                .Link = TrackerServerRuTor & "/browse/0/10/0/0/;PAGERUTOR"
                .ImageLink = ICO_Folder
                .Description = "<html><font face=""Arial"" size=""5""><b>" & .Name & "</font></b><p><img src=""" & TrackerServerRuTor & LOGO_Tracker & """ />"
                items.Add(Item)
            End With

            Item = New Item
            With Item
                .Name = "Музыка"
                .Link = TrackerServerRuTor & "/browse/0/2/0/0/;PAGERUTOR"
                .ImageLink = ICO_Folder
                .Description = "<html><font face=""Arial"" size=""5""><b>" & .Name & "</font></b><p><img src=""" & TrackerServerRuTor & LOGO_Tracker & """ />"
                items.Add(Item)
            End With

            Item = New Item
            With Item
                .Name = "Юмор"
                .Link = TrackerServerRuTor & "/browse/0/15/0/0/;PAGERUTOR"
                .ImageLink = ICO_Folder
                .Description = "<html><font face=""Arial"" size=""5""><b>" & .Name & "</font></b><p><img src=""" & TrackerServerRuTor & LOGO_Tracker & """ />"
                items.Add(Item)
            End With

            Item = New Item
            With Item
                .Name = "Спорт и Здоровье"
                .Link = TrackerServerRuTor & "/browse/0/13/0/0/;PAGERUTOR"
                .ImageLink = ICO_Folder
                .Description = "<html><font face=""Arial"" size=""5""><b>" & .Name & "</font></b><p><img src=""" & TrackerServerRuTor & LOGO_Tracker & """ />"
                items.Add(Item)
            End With
            PlayList.IsIptv = False
            Return PlayListPlugPar(items, context)
        End Function
#End Region



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
            WC.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv: 47.0) Gecko/20100101 Firefox/47.0")
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