using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;

namespace Icobo
{
    class Program
    {
        private static void ShowAlert(string alert)
        {
            // log
            File.WriteAllText(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\IcoboAlert.txt",
                alert,
                System.Text.Encoding.UTF8
            );
            Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\IcoboAlert.txt").WaitForExit();
            File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\IcoboAlert.txt");
        }
        [DllImport("shell32.dll", EntryPoint="SHDefExtractIconW")]
        private static extern int SHDefExtractIconW([MarshalAs(UnmanagedType.LPTStr)] string pszIconFile, int iIndex, uint uFlags, ref IntPtr phiconLarge, ref IntPtr phiconSmall, uint nIconSize);

        private static int GennyIconSize(int low, int high)
        {
            return high << 16 | (low & 65535);
        }

        public static Icon GetIconFromExe(string exe, int size)
        {
            IntPtr hIconPtr = IntPtr.Zero;
            IntPtr hTrashThis = IntPtr.Zero;
            try
            {
                int worx = SHDefExtractIconW(exe, 0, 0U, ref hTrashThis, ref hIconPtr, checked((uint)GennyIconSize(16, size)));
                hTrashThis = IntPtr.Zero;
                return (Icon)Icon.FromHandle(hIconPtr).Clone();
            }
            catch (Exception exc)
            {
                return null;
            }
        }

        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                ShowAlert("--- Instructions ---\nClose this Notepad window and drag an EXE onto icobo.exe to extract its icons.\nIf it encounters an error, a Notepad window like this one will appear.\nOtherwise, an Explorer window should appear with the icons inside of it.");
                return;
            }
            try
            {
                List<Icon> icons = new List<Icon>();
                icons.Add(GetIconFromExe(args[0], 32));
                icons.Add(GetIconFromExe(args[0], 48));
                icons.Add(GetIconFromExe(args[0], 64));
                icons.Add(GetIconFromExe(args[0], 128));
                icons.Add(GetIconFromExe(args[0], 256));

                string DocsName = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string DirName = DocsName + "\\Extracted Icons\\" + Path.GetFileName(args[0].Replace(".exe", ""));
                if (!Directory.Exists(DocsName + "\\Extracted Icons\\"))
                {
                    Directory.CreateDirectory(DocsName + "\\Extracted Icons");
                }

                if (Directory.Exists(DirName)) {
                    DirName = DirName + " (id" + Directory.GetDirectories(DirName+"\\..\\").Length + ")";
                }
                Directory.CreateDirectory(DirName);

                foreach (Icon icon in icons)
                {
                    if (icon != null)
                    {
                        icon.ToBitmap().Save(DirName + "\\icon_" + icon.Width.ToString() + ".png");
                    }
                    else
                    {
                        ShowAlert("This EXE doesn't seem to have any icons.");
                        return;
                    }
                }

                Process.Start(DirName);
            }
            catch (Exception exc)
            {
                ShowAlert(exc.Message);
            }
        }
    }
}
