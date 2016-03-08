using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using RemoteFork.Properties;

namespace RemoteFork {
    public partial class Main : Form {
        private HttpServer httpServer;
        private Thread thread;
        public static HashSet<string> Devices = new HashSet<string>();

        #region Form

        public Main() {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e) {
            tbIp.Text = Settings.Default.GetIpAuto ? Tools.GetIPAddress() : Settings.Default.IpIPAddress;
            cbDlna.Checked = Settings.Default.Dlna;
            cbAutoIp.Checked = Settings.Default.GetIpAuto;
            cbAutoStart.Checked = Settings.Default.ServerAutoStart;

            if (Settings.Default.GetIpAuto) {
                tbIp.ReadOnly = true;
            }
            if (Settings.Default.ServerAutoStart) {
                bStartServer.PerformClick();
            }
            notifyIcon1.Visible = true;
            HideForm();
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e) {
            StopServer();
        }

        private void Main_Resize(object sender, EventArgs e) {
            switch (WindowState) {
                case FormWindowState.Minimized:
                    HideForm();
                    break;
            }
        }

        private void ShowForm() {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void HideForm() {
            Hide();
            WindowState = FormWindowState.Minimized;
        }

        #endregion Form

        #region Server

        private void StartServer() {
            httpServer = new MyHttpServer(IPAddress.Parse(tbIp.Text), int.Parse(tbPort.Text));
            thread = new Thread(httpServer.Listen);
            thread.Start();
        }

        private void StopServer() {
            if (httpServer != null) {
                httpServer.Stop();
            }
        }

        #endregion Server

        #region Settings

        private void bStartServer_Click(object sender, EventArgs e) {
            toolStripStatusLabel1.Text = "Запуск сервера..";
            try {
                System.Net.WebRequest reqGET = System.Net.WebRequest.Create(@"http://getlist2.obovse.ru/remote/index.php?v=1.2" + @"&do=list&localip=" + tbIp.Text + ":"+tbPort.Text);
                System.Net.WebResponse resp = reqGET.GetResponse();
                System.IO.Stream stream = resp.GetResponseStream();
                System.IO.StreamReader sr = new System.IO.StreamReader(stream);
                string res = sr.ReadToEnd();
                String str = @"";
                if (res.Split('|')[0].ToString() == "new_version")
                {
                    str = res.Split('|')[1].ToString();
                    MessageBox.Show(str);
                }
                StartServer();
                bStartServer.Enabled = false;
                bStopServer.Enabled = true;
                toolStripStatusLabel1.Text = "Сервер запущен";
            } catch (Exception ex) {
                toolStripStatusLabel1.Text = "Ошибка!";
                Console.WriteLine(ex);
            }
        }

        private void bStopServer_Click(object sender, EventArgs e) {
            toolStripStatusLabel1.Text = "Остановка сервера..";
            try {
                StopServer();
                bStartServer.Enabled = true;
                bStopServer.Enabled = false;
                toolStripStatusLabel1.Text = "Сервер остановлен";
            } catch (Exception ex) {
                toolStripStatusLabel1.Text = "Ошибка!";
                Console.WriteLine(ex);
            }
        }

        private void cbAutoStart_CheckedChanged(object sender, EventArgs e) {
            Settings.Default.ServerAutoStart = cbAutoStart.Checked;
        }

        private void cbAutoIp_CheckedChanged(object sender, EventArgs e) {
            if (cbAutoIp.Checked) {
                Settings.Default.GetIpAuto = true;
                tbIp.ReadOnly = true;
                tbIp.Text = Tools.GetIPAddress();
            } else {
                Settings.Default.GetIpAuto = false;
                tbIp.ReadOnly = false;
                Settings.Default.IpIPAddress = tbIp.Text;
            }
            Settings.Default.Save();
        }

        private void tbIp_TextChanged(object sender, EventArgs e) {
            IPAddress ip;
            if (IPAddress.TryParse(tbIp.Text, out ip)) {
                Settings.Default.IpIPAddress = tbIp.Text;
                Settings.Default.Save();
            }
        }

        private void cbDlna_CheckedChanged(object sender, EventArgs e) {
            Settings.Default.Dlna = cbDlna.Checked;
            Settings.Default.Save();
        }

        #endregion Settings

        #region notifyIcon

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                ShowForm();
            }
        }

        private readonly List<ToolStripMenuItem> deviceMenuItems = new List<ToolStripMenuItem>();

        private void loadPlaylistToolStripMenuItem1_MouseHover(object sender, EventArgs e) {
            Console.WriteLine("read Sets");
            foreach (var deviceMenuItem in deviceMenuItems) {
                loadPlaylistToolStripMenuItem1.DropDownItems.Remove(deviceMenuItem);
            }
            deviceMenuItems.Clear();

            if (Devices.Count > 0) {
                foreach (var device in Devices) {
                    string[] array = device.Split('|');
                    string name = array[0] + " (" + array[2] + ")";

                    var item = new ToolStripMenuItem {
                        Name = "device" + array[0] + "ToolStripMenuItem",
                        Tag = device,
                        Text = name
                    };
                    item.Click += devicesToolStripMenuItem1_Click;

                    deviceMenuItems.Add(item);
                    loadPlaylistToolStripMenuItem1.DropDownItems.Add(item);
                }

                devicesToolStripMenuItem1.Visible = false;
            } else {
                devicesToolStripMenuItem1.Text = "Нет активных устройств";
                devicesToolStripMenuItem1.Visible = true;
            }
        }

        private async void devicesToolStripMenuItem1_Click(object sender, EventArgs e) {
            ToolStripMenuItem clickedItem = (ToolStripMenuItem)sender;
            if (openFileDialog1.ShowDialog() == DialogResult.OK) {
                StreamReader streamReader = new StreamReader(openFileDialog1.FileName);
                string text = streamReader.ReadToEnd();
                streamReader.Close();
                if (text.Length < 102401 &&
                    (text.Contains("EXTM3U") || text.Contains("<title>") || text.Contains("http://"))) {
                    string url = "http://forkplayer.tv/remote/index.php?do=uploadfile&fname=" +
                                 openFileDialog1.FileName + "&initial=" + clickedItem.Tag;

                    var data = new Dictionary<string, string> { { "text", text } };
                    string text2 = await HttpUtility.PostRequest(url, data);

                    MessageBox.Show(text2);
                } else {
                    MessageBox.Show("Неверный файл плейлиста!");
                }
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e) {
            ShowForm();
        }

        private void openTestToolStripMenuItem_Click(object sender, EventArgs e) {
            Process.Start("http://" + tbIp.Text + ":" + tbPort.Text + "/test");
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            try {
                StopServer();
            } finally {
                Close();
            }
        }

        #endregion notifyIcon
    }
}
