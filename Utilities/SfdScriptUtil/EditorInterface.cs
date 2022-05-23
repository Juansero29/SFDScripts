using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace SfdScriptUtil
{
    static class EditorInterface
    {

        delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn,
            IntPtr lParam);

        static IEnumerable<IntPtr> EnumerateProcessWindowHandles(int processId)
        {
            var handles = new List<IntPtr>();

            foreach (ProcessThread thread in Process.GetProcessById(processId).Threads)
                EnumThreadWindows(thread.Id,
                    (hWnd, lParam) => { handles.Add(hWnd); return true; }, IntPtr.Zero);

            return handles;
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam,
            StringBuilder lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetWindowTextLength(IntPtr hWnd);

        static string GetWindowTitle(IntPtr hWnd)
        {
            var length = GetWindowTextLength(hWnd) + 1;
            var title = new StringBuilder(length);
            GetWindowText(hWnd, title, length);
            return title.ToString();
        }

        public static void PasteScript(string text)
        {
            IntPtr scriptWindow = (IntPtr)0;
            foreach (IntPtr handle in EnumerateProcessWindowHandles(
    Process.GetProcessesByName("Superfighters Deluxe").First().Id))
            {
                if (GetWindowTitle(handle) == "Script Editor") scriptWindow = handle;
            }

            if (SetForegroundWindow(scriptWindow) != 0)
            {
                string temp = Clipboard.GetText();
                Clipboard.SetText(text);
                SendKeys.SendWait("^a");
                SendKeys.SendWait("{DEL}");
                SendKeys.SendWait("^v");
                Clipboard.SetText(temp);
            }
            else
            {
                Console.WriteLine("Failed to write script data");
            }
        }

        public static void StartMap()
        {
            IntPtr mapEditor = (IntPtr)0;
            foreach (IntPtr handle in EnumerateProcessWindowHandles(
    Process.GetProcessesByName("Superfighters Deluxe").First().Id))
            {
                if (GetWindowTitle(handle) == "Superfighters Deluxe Map Editor") mapEditor = handle;
            }
            
            if (SetForegroundWindow(mapEditor) != 0)
            {
                SendKeys.SendWait("{F5}");
            }
            else
            {
                Console.WriteLine("Failed to open map editor");
            }
        }
    }
}
