using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;　　//１、添加命名空间

namespace USBMonitor
{
    class Clipboard
    {
        [DllImport("User32.dll")]
        protected static extern int SetClipboardViewer(int hWndNewViewer);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

        [DllImport("user32.dll")]
        public static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

        IntPtr nextClipboardViewer;
        protected void WndProc(ref Message m)
        {
            const int WM_DRAWCLIPBOARD = 0x308;
            const int WM_CHANGECBCHAIN = 0x030D;
            switch (m.Msg)
            {
                case WM_CHANGECBCHAIN:
                    if (m.WParam == nextClipboardViewer) { nextClipboardViewer = m.LParam; }
                    else { SendMessage(nextClipboardViewer, m.Msg, m.WParam, m.LParam); }
                    break;
                case WM_DRAWCLIPBOARD:
                    System.Windows.Forms.Clipboard.Clear();　//清空剪贴板内容

                    //将WM_DRAWCLIPBOARD 消息传递到下一个观察链中的窗口
                    SendMessage(nextClipboardViewer, m.Msg, m.WParam, m.LParam);
                    //将WM_DRAWCLIPBOARD 消息传递到下一个观察链中的窗口 

                    break;
                default:
                    break;
            }
        }
    }
}
