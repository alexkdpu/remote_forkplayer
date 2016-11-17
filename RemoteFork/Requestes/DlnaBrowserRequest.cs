using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using RemoteFork.Forms;
using RemoteFork.Properties;
using RemoteFork.Server;

namespace RemoteFork.Requestes {
    internal class DlnaBrowserRequest : ProcessorRequest {
        public DlnaBrowserRequest(string text, HttpProcessor processor) : base(text, processor) {
        }

        public override string Execute() {
            string hostText = string.Format("http://{0}/", processor.Host);

            var result = new StringBuilder();

            result.AppendLine("#EXTM3U");

            if (text == "/") {
                if (Settings.Default.DlnaFilterType == 1) {
                    if (Settings.Default.DlnaDirectories != null) {
                        foreach (string directory in Settings.Default.DlnaDirectories) {
                            if (Directory.Exists(directory)) {
                                string urlText = HttpUtility.UrlEncode(directory + "\\");

                                result.AppendLine(string.Format("#EXTINF:-1,{0}\n{1}treeview?{2}",
                                    directory, hostText, urlText));

                                Logger.Debug("DlnaBrowserRequest->Filtering directory: {0}", directory);
                            }
                        }
                    }
                } else {
                    var drives = DriveInfo.GetDrives();

                    foreach (var drive in drives.Where(i => DlnaConfigurate.CheckAccess(i.Name))) {
                        if (drive.IsReady) {
                            string text5 = string.Format("{0} ({1} свободно из {2})", drive.Name,
                                Tools.FSize(drive.AvailableFreeSpace), Tools.FSize(drive.TotalSize));

                            string driveName = string.Format("{0}<br>Метка диска: {1}<br>Тип носителя: {2}", text5,
                                drive.VolumeLabel, drive.DriveType);

                            result.AppendLine(string.Format("#EXTINF:-1,{0}\n{1}treeview?{2}", driveName, hostText,
                                drive.Name));

                            Logger.Debug("DlnaBrowserRequest->Drive: {0}", driveName);
                        }
                    }
                }

                if (Settings.Default.UserUrls != null && Settings.Default.UserUrls.Count > 0) {
                    result.AppendLine(string.Format("#EXTINF:-1,{0}\n{1}treeview?{2}.m3u",
                        "Пользовательские ссылки", hostText, "urls"));

                    Logger.Debug("DlnaBrowserRequest->User urls: {0}", Settings.Default.UserUrls.Count);
                }

                var plugin = new PluginRequest("/treeview?plugin", processor);
                result.AppendLine(plugin.Execute());
            } else if (text == "urls.m3u") {
                if (Settings.Default.UserUrls != null && Settings.Default.UserUrls.Count > 0) {
                    foreach (string url in Settings.Default.UserUrls) {
                        result.AppendLine(string.Format("#EXTINF:-1,{0}\n{1}", url.Split('\\').Last().Split('/').Last(),
                            url));
                    }
                }
            } else {
                string[] array = Directory.GetDirectories(text);

                foreach (string directory in array.Where(DlnaConfigurate.CheckAccess)) {
                    string urlText = HttpUtility.UrlEncode(directory + "\\");

                    result.AppendLine(string.Format("#EXTINF:-1,{0}\n{1}treeview?{2}", directory.Split('\\').Last(),
                        hostText, urlText));

                    Logger.Debug("DlnaBrowserRequest->Directory: {0}", directory);
                }
                array = Directory.GetFiles(text);
                foreach (string file in array.Where(DlnaConfigurate.CheckAccess)) {
                    string urlText = HttpUtility.UrlEncode(file.Replace("\\", "\\\\"));
                    FileInfo fileInfo = new FileInfo(file);

                    result.AppendLine(string.Format("#EXTINF:-1,{0} ({1})\n{2}{3}", file.Split('\\').Last(),
                        Tools.FSize(fileInfo.Length), hostText, urlText));

                    Logger.Debug("DlnaBrowserRequest->Directory: {0}", file);
                }
            }

            return result.ToString();
        }
    }
}
