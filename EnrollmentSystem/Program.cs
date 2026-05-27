using System;
using System.Windows.Forms;
using EnrollmentSystem.Data;
using EnrollmentSystem.Forms;

namespace EnrollmentSystem
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            DataManager.Initialize();
            Application.Run(new LoginForm());
        }
    }
}
