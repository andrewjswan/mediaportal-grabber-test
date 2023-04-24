using System;
using System.Windows.Forms;

namespace TestGrabberMP1
{
    static class TestGrabberMP1
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FrmTestGrabberMP1());
        }
    }
}
