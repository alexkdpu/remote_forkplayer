using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using PluginApi.Plugins;
using RemoteFork.Forms;
using RemoteFork.Plugins;
using RemoteFork.Properties;
using RemoteFork.Server;

namespace RemoteFork.Requestes {
    internal class DlnaBrowserRequest : ProcessorRequest {
        public DlnaBrowserRequest(string text, HttpProcessor processor) : base(text, processor) {
        }

        public override string Execute() {
            string hostText = $"http://{processor.Host}/";

            List<Item> result = new List<Item>();

            if (text == "/") {
                if (Settings.Default.DlnaFilterType == 1) {
                    if (Settings.Default.DlnaDirectories != null) {
                        foreach (string directory in Settings.Default.DlnaDirectories) {
                            if (Directory.Exists(directory)) {
                                string urlText = HttpUtility.UrlEncode(directory + "\\");

                                result.Add(new Item {
                                    Name = directory,
                                    Link = $"{hostText}treeview?{urlText}",
                                    Type = ItemType.DIRECTORY
                                });

                                Logger.Debug("DlnaBrowserRequest->Filtering directory: {0}", directory);
                            }
                        }
                    }
                } else {
                    var drives = DriveInfo.GetDrives();

                    foreach (var drive in drives.Where(i => DlnaConfigurate.CheckAccess(i.Name))) {
                        if (drive.IsReady) {
                            string mainText = $"{drive.Name} ({Tools.FSize(drive.AvailableFreeSpace)} свободно из {Tools.FSize(drive.TotalSize)})";

                            result.Add(new Item {
                                Name = $"{mainText}<br>Метка диска: {drive.VolumeLabel}<br>Тип носителя: {drive.DriveType}",
                                Link = $"{hostText}treeview?{drive.Name}",
                                Type = ItemType.DIRECTORY
                            });

                            Logger.Debug("DlnaBrowserRequest->Drive: {0}", $"{mainText}<br>Метка диска: {drive.VolumeLabel}<br>Тип носителя: {drive.DriveType}");
                        }
                    }
                }

                if (Settings.Default.UserUrls != null && Settings.Default.UserUrls.Count > 0) {
                    result.Add(new Item {
                        Name = "Пользовательские ссылки",
                        Link = $"{hostText}treeview?urls.m3u",
                        Type = ItemType.DIRECTORY
                    });

                    Logger.Debug("DlnaBrowserRequest->User urls: {0}", Settings.Default.UserUrls.Count);
                }

                foreach (var plugin in PluginManager.Instance.GetPlugins()) {
                    string urlText = HttpUtility.UrlEncode($"{plugin.Key}\\");

                    result.Add(new Item {
                        Name = plugin.Value.Name,
                        Link = $"{hostText}treeview?plugin{urlText}.xml",
                        ImageLink = plugin.Value.ImageLink,
                        Type = ItemType.DIRECTORY
                    });

                    Logger.Debug("PluginRequest->List {0}", plugin.Value.Name);
                }
            } else if (text == "urls.m3u") {
                if (Settings.Default.UserUrls != null && Settings.Default.UserUrls.Count > 0) {
                    foreach (string url in Settings.Default.UserUrls) {
                        result.Add(new Item {
                            Name = url.Split('\\').Last().Split('/').Last(),
                            Link = $"{url}",
                            Type = ItemType.DIRECTORY
                        });
                    }
                }
            } else {
                string[] array = Directory.GetDirectories(text);

                foreach (string directory in array.Where(DlnaConfigurate.CheckAccess)) {
                    string urlText = HttpUtility.UrlEncode(directory + "\\");

                    result.Add(new Item {
                        Name = directory.Split('\\').Last(),
                        Link = $"{hostText}treeview?{urlText}",
                        Type = ItemType.DIRECTORY
                    });

                    Logger.Debug("DlnaBrowserRequest->Directory: {0}", directory);
                }
                array = Directory.GetFiles(text);
                foreach (string file in array.Where(DlnaConfigurate.CheckAccess)) {
                    string urlText = HttpUtility.UrlEncode(file.Replace("\\", "\\\\"));
                    FileInfo fileInfo = new FileInfo(file);

                    result.Add(new Item {
                        Name = $"{file.Split('\\').Last()} ({Tools.FSize(fileInfo.Length)})",
                        Link = $"{hostText}{urlText}",
                        Type = ItemType.FILE
                    });

                    Logger.Debug("DlnaBrowserRequest->Directory: {0}", file);
                }
            }

            return ResponseSerializer.ToM3U(result.ToArray());
        }
    }
}
