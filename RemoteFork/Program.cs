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
#if DEBUG
                WindowState = FormWindowState.Normal
#else
                WindowState = FormWindowState.Minimized
#endif
            };
            Application.Run(main);
        }
    }
}
