using System;
using System.Windows.Forms;

namespace CybersecurityAwarenessBotGUI;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
