Option Explicit On
Imports System.Linq
Imports PluginApi.Plugins
Imports RemoteFork.Plugins
Imports System.Collections.Generic
Imports System.Collections.Specialized
Imports System.Drawing
Imports Microsoft.VisualBasic
Imports System


Namespace RemoteFork.Plugins
    <PluginAttribute(Id:="kinorgoru", Version:="0.1", Author:="ORAMAN", Name:="KinoRGOru", Description:="Фильмы Русского Географического Общества", ImageLink:="http://grozny-inform.ru/LoadedImages/2017/02/10/Bez_nazvaniya.png")>
    Public Class kinorgoru
        Implements IPlugin

        Dim IPAdress As String
        Dim PortRemoteFork As String = "8027"
        Dim PLUGIN_PATH As String = "pluginPath"
        Dim PlayList As New PluginApi.Plugins.Playlist
        Dim next_page_url As String
        Dim IDPlagin As String = "kinorgoru"
        Dim URLPortal As String = "http://kino.rgo.ru"

#Region "Иконки"
        Dim ICO_VideoFile As String = "http://s1.iconbird.com/ico/1012/AmpolaIcons/w256h2561350597291videofile.png"
        Dim ICO_Search As String = "http://s1.iconbird.com/ico/0612/MustHave/w256h2561339195991Search256x256.png"
        Dim ICO_Search2 As String = "http://s1.iconbird.com/ico/0912/MetroUIDock/w512h5121347464996Search.png"
        Dim ICO_Pusto As String = "https://avatanplus.com/files/resources/mid/5788db3ecaa49155ee986d6e.png"
        Dim LOGO As String = "http://grozny-inform.ru/LoadedImages/2017/02/10/Bez_nazvaniya.png"
        '  Dim Items As System.Collections.Generic.List(Of Item)
        Dim ReGex As System.Text.RegularExpressions.Regex
        Dim ReGexName As System.Text.RegularExpressions.Regex
        Dim ReGexLink As System.Text.RegularExpressions.Regex
        Dim ReGexImage As System.Text.RegularExpressions.Regex
        Dim ReGexDescription As System.Text.RegularExpressions.Regex
#End Region

        Public Function GetList(context As IPluginContext) As PluginApi.Plugins.Playlist Implements IPlugin.GetList

            IPAdress = context.GetRequestParams.Get("host").Split(":")(0)

            PlayList.source = Nothing
            Dim path = context.GetRequestParams().Get(PLUGIN_PATH)
            path = (If((path Is Nothing), "plugin", "plugin;" & path))

            If context.GetRequestParams("search") <> Nothing Then
                Return GetPage(context, "/films/?search=" & context.GetRequestParams("search"))
            End If


            Select Case path
                Case "plugin"
                    Return GetTopList(context)

            End Select




            Dim PathSpliter() As String = path.Split(";")

            Select Case PathSpliter(PathSpliter.Length - 1)
                Case "PAGE"
                    Return GetPage(context, PathSpliter(PathSpliter.Length - 2))
                Case "PAGE_FILM"
                    Return GetPageFilm(context, PathSpliter(PathSpliter.Length - 2))

            End Select

            Dim items As New System.Collections.Generic.List(Of Item)
            PlayList.IsIptv = "false"
            Return PlayListPlugPar(items, context)

        End Function



        Public Function toSource(ByVal source As String, ByVal context As IPluginContext) As Playlist 'Отдает текст source напрямую в forkplayer игнорируя остальные поля Playlist
            PlayList.source = source
            Return PlayList
        End Function

        Function PlayListPlugPar(ByVal items As System.Collections.Generic.List(Of Item), ByVal context As IPluginContext, Optional ByVal next_page_url As String = "") As PluginApi.Plugins.Playlist
            If next_page_url <> "" Then
                Dim pluginParams = New NameValueCollection()
                pluginParams(PLUGIN_PATH) = next_page_url
                PlayList.NextPageUrl = context.CreatePluginUrl(pluginParams)
            Else
                PlayList.NextPageUrl = Nothing
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

        Function Requesters(ByVal Path As String) As String
            Dim WC As New Net.WebClient
            WC.Headers.Item(Net.HttpRequestHeader.UserAgent) = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36"
            WC.Encoding = System.Text.Encoding.UTF8

            Dim STR As String = WC.DownloadString(URLPortal & Path).Replace(vbCrLf, "").Replace(vbCr, "").Replace(vbLf, "")
            Return STR
        End Function

        Dim xDoc As New System.Xml.XmlDocument()



        Public Function GetTopList(context As IPluginContext) As PluginApi.Plugins.Playlist
            Dim Items As New System.Collections.Generic.List(Of Item)
            Dim Item As New Item
            With Item
                .Name = "Поиск"
                .Link = "search"
                .Type = ItemType.DIRECTORY
                .SearchOn = "Поиск"
                .ImageLink = ICO_Search
                .Description = "<div id=""poster"" style=""float:left;padding:4px; background-color:#EEEEEE;margin:0px 13px 1px 0px;""><img src=""" & LOGO & """ style=""width:180px;float:left;"" /></div><span style=""color:#3090F0"">О проекте:</span><br><font face=""Arial Narrow"" size=""4"">Ежегодно по заказу и на гранты Русского географического общества создаются новые фильмы о географии, истории и тайнах России. Среди них есть видеоуроки, художественные ленты, документальное кино и даже мультфильмы. <p> В Год российского кино Русское географическое общество открывает собственный видеопортал. Здесь представлены все фильмы Общества, созданные при участии известных российских географов, биологов, этнографов, экологов, палеонтологов и путешественников Русского географического общества. Портал фильмов Русского географического общества &ndash; отличная возможность открыть&nbsp; для себя мир науки и географии, отправиться в путешествие по России, увидеть редких животных, познакомиться с невиданными растениями и узнать больше о судьбах великих путешественников."
            End With
            Items.Add(Item)

            '  Категории
            Dim Str As String = Requesters("/category/")
            ReGex = New System.Text.RegularExpressions.Regex("(<a class=""cat-link"" href=""/category/).*?(class=""video-list-all"">)")
            ReGexLink = New System.Text.RegularExpressions.Regex("(?<=<a class=""cat-link"" href="").*?(?="")")
            ReGexImage = New System.Text.RegularExpressions.Regex("(?<=').*?(?=')")
            ReGexName = New System.Text.RegularExpressions.Regex("(?<=<h2>).*?(?=</h2>)")
            ReGexDescription = New System.Text.RegularExpressions.Regex("(<div class=""cat-size""><span>).*?(</div>)")
            For Each GroopItem As System.Text.RegularExpressions.Match In ReGex.Matches(Str)
                Dim ItemGroop As New Item
                With ItemGroop
                    .Link = ReGexLink.Match(GroopItem.Value).Value & ";PAGE"
                    .ImageLink = ReGexImage.Match(GroopItem.Value).Value
                    ' .ImageLink = "http://" & IPAdress & ":8027/proxym3u8B" & Base64Encode(ReGexImage.Match(GroopItem.Value).Value & "OPT:ContentType--image/jpegOPEND:/") & "/"
                    .Name = ReGexName.Match(GroopItem.Value).Value
                    .Type = ItemType.DIRECTORY
                    .Description = "<html><font face=""Arial"" size=""5""><b>" & .Name & "</font></b><p><img  width=""100%""  src=""" & .ImageLink & """ />" & ReGexDescription.Match(GroopItem.Value).Value
                    Items.Add(ItemGroop)
                End With
            Next
            PlayList.IsIptv = "false"
            Return PlayListPlugPar(Items, context)
        End Function

        Public Function GetPage(context As IPluginContext, ByVal URL As String) As PluginApi.Plugins.Playlist
            ReGex = New System.Text.RegularExpressions.Regex("( <div class=""f-box"">).*?(<div class=""like""></div>)")
            ReGexName = New System.Text.RegularExpressions.Regex("(?<=<div class=""f-name-text"">).*?(?=</div>)")
            ReGexLink = New System.Text.RegularExpressions.Regex("(?<=<a class=""f-link"" href="").*?(?="">)")
            ReGexImage = New System.Text.RegularExpressions.Regex("(?<=').*?(?=')")
            ReGexDescription = New System.Text.RegularExpressions.Regex("(<span class=""f-time"">).*(</div>)")
            Dim Str As String = Requesters(URL)
            Dim Items As New System.Collections.Generic.List(Of Item)
            For Each ItemPage As System.Text.RegularExpressions.Match In ReGex.Matches(Str)
                Dim PAgeItem As New Item
                With PAgeItem
                    .Name = Trim(ReGexName.Match(ItemPage.Value).Value)
                    .ImageLink = ReGexImage.Match(ItemPage.Value).Value
                    .Link = ReGexLink.Match(ItemPage.Value).Value & ";PAGE_FILM"
                    ' .Description = "<html><font face=""Arial"" size=""5""><b>" & .Name & "</font></b><p><img  width=""100%""  src=""" & .ImageLink & """ />" & ReGexDescription.Match(ItemPage.Value).Value.Replace("<div class=""f-views icon-font icon_eye"">", "<div class=""f-views icon-font icon_eye"">Просмотров ")
                    .Description = "<html><font face=""Arial"" size=""5""><b>" & .Name & "</font></b><p><div align=""center""><img  src=""" & .ImageLink & """width=""80%"" /></div>" & ReGexDescription.Match(ItemPage.Value).Value.Replace("<div class=""f-views icon-font icon_eye"">", "<div class=""f-views icon-font icon_eye"">Просмотров ")
                    .Type = ItemType.DIRECTORY
                    Items.Add(PAgeItem)
                End With
            Next
            PlayList.IsIptv = "false"
            Return PlayListPlugPar(Items, context)
        End Function


        Public Function GetPageFilm(context As IPluginContext, ByVal URL As String) As PluginApi.Plugins.Playlist
            Dim Items As New System.Collections.Generic.List(Of Item)
            Dim Str As String = Requesters(URL)


            ReGexName = New System.Text.RegularExpressions.Regex("(?<=<h1>).*?(?=</h1>)")
            ReGexImage = New System.Text.RegularExpressions.Regex("(?<=poster="").*?(?="")")
            ReGexDescription = New System.Text.RegularExpressions.Regex("(<div class=""video-description"">).*(?=<div class=""video-materials"">)")
            ReGexLink = New System.Text.RegularExpressions.Regex("(?<=<source src="").*?(?="")")

            Dim Image As String = ReGexImage.Match(Str).Value


            Dim Item As New Item
            With Item
                .Name = ReGexName.Match(Str).Value
                .Link = ReGexLink.Match(Str).Value
                .Type = ItemType.FILE
                .ImageLink = ICO_VideoFile
                .Description = "<div id=""poster"" style=""float:left;padding:4px;        background-color:#EEEEEE;margin:0px 13px 1px 0px;"">" & "<img src=""" & ReGexImage.Match(Str).Value & """ style=""width:180px;float:left;"" /></div><span style=""color:#3090F0"">" & .Name & "<br><span style=""color:#3090F0"">Описание: <br></span><span style=""color:#FFFFFF"">" & replacetags(ReGexDescription.Match(Str).Value) & "</span>"
            End With
            Items.Add(Item)



            PlayList.IsIptv = "false"
            Return PlayListPlugPar(Items, context, next_page_url)
        End Function
        Function Base64Encode(ByVal plainText As String) As String
            Dim plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText)
            Return System.Convert.ToBase64String(plainTextBytes)
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
    End Class
End Namespace
