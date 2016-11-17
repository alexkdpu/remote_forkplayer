using System;
using System.Windows.Forms;
using RemoteFork.Forms;

namespace RemoteFork {
    internal static class Program {
        [STAThread]
        private static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Main main = new Main {
                WindowState = FormWindowState.Minimized
            };
            Application.Run(main);
        }
    }
}
