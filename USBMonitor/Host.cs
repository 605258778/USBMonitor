using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Management;
using System.Threading;
using System.Diagnostics;
using System.Linq;
using Excel = Microsoft.Office.Interop.Excel;
using System.Collections;
using System.Text;
using DeviceManagement;
using ReactiveUI;

namespace USBMonitor
{
    public partial class Host : Form
    {
        public string title = Application.ProductName;
        public string dir = Application.StartupPath + @"\USBMonitorData\";
        public string[] white;
        public string[] black;
        public string[] blackdisk;
        public string[] blackid;
        public const int DIF_REMOVE = (0x00000005);
        public string diskser = "";
        public string diskname = "";
        Hashtable diskMap = new Hashtable();
        Hashtable watcherMap = new Hashtable();
        public string disk;
        public string filePath;
        public string copytime;
        public string copyname;
        public string copydiskdir;
        

        ManagementObject diskinfo;
        public string path;
        public ReactiveCommand<object> Eject { get; private set; }
        static FileSystemWatcher watcher = new FileSystemWatcher();//文件监听
        /****************引入系统相关API******************/
        [DllImport("user32.dll")]
        public static extern bool AddClipboardFormatListener(IntPtr hwnd);
        [DllImport("user32.dll")]
        public static extern bool RemoveClipboardFormatListener(IntPtr hwnd);
        //释放设备的访问
        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern IntPtr SetupDiGetClassDevs(ref Guid ClassGuid, uint Enumerator, IntPtr HwndParent, DIGCF Flags);
        public const int DIGCF_ALLCLASSES = (0x00000004);
        public const int DIGCF_PRESENT = (0x00000002);
        public const int INVALID_HANDLE_VALUE = -1;
        public const int SPDRP_DEVICEDESC = (0x00000000);
        public const int MAX_DEV_LEN = 200;
        public const int DEVICE_NOTIFY_WINDOW_HANDLE = (0x00000000);
        public const int DEVICE_NOTIFY_SERVICE_HANDLE = (0x00000001);
        public const int DEVICE_NOTIFY_ALL_INTERFACE_CLASSES = (0x00000004);
        public const int DBT_DEVTYP_DEVICEINTERFACE = (0x00000005);
        public const int DBT_DEVNODES_CHANGED = (0x0007);
        public const int WM_DEVICECHANGE = (0x0219);
        public enum DIGCF
        {
            DIGCF_DEFAULT = 0x1,
            DIGCF_PRESENT = 0x2,
            DIGCF_ALLCLASSES = 0x4,
            DIGCF_PROFILE = 0x8,
            DIGCF_DEVICEINTERFACE = 0x10
        }
        IntPtr hDevInfo;
        Guid guidHID = Guid.Empty;
        public Host()
        {
            InitializeComponent();
            setIconX(0);
            if (!string.IsNullOrEmpty(Properties.Settings.Default.dir))
            {
                dir = Properties.Settings.Default.dir + "\\";
            }
            try
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), title + " 初始化失败", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Environment.Exit(1);
            }
            if (!string.IsNullOrEmpty(Properties.Settings.Default.black))
            {
                black = Properties.Settings.Default.black.Split(',');
            }
            else
            {
                black = new string[0];
            }
            if (!string.IsNullOrEmpty(Properties.Settings.Default.blackdisk))
            {
                blackdisk = Properties.Settings.Default.blackdisk.Split(',');
            }
            else
            {
                blackdisk = new string[0];
            }
            if (!string.IsNullOrEmpty(Properties.Settings.Default.blackid))
            {
                blackid = Properties.Settings.Default.blackid.Split(',');
            }
            else
            {
                blackid = new string[0];
            }
            if (!string.IsNullOrEmpty(Properties.Settings.Default.white))
            {
                white = Properties.Settings.Default.white.Split(',');
            } else
            {
                white = new string[0];
            }

            nicon.Visible = Program.showicon;
            if (!Properties.Settings.Default.multirun)
            {
                Process[] processcollection = Process.GetProcessesByName(Application.ProductName);
                if (processcollection.Length >= 2)
                {
                    msg("已经有一个 USBMonitor 实例在运行中！本实例即将退出。如果您需要多重运行本程序，请在设置中打开 \"允许多重运行\" 开关", ToolTipIcon.Error);
                    Thread.Sleep(3000);
                    Environment.Exit(9);
                }
            }
            if (Properties.Settings.Default.firstrun)
            {
                msg("欢迎使用 USBMonitor ! 右键单击此图标可进行设置");
                Properties.Settings.Default.firstrun = false;
                Properties.Settings.Default.Save();
            }
        }

        public delegate void setIconInvoke(int v);
        public void setIcon(int v)
        {
            setIconInvoke i = new setIconInvoke(setIconX);
            Invoke(i, v);
        }

        private void setIconX(int v)
        {
            if (v == 0)
            {
                nicon.Text = title + " - 空闲";
                nicon.Icon = Icon = Properties.Resources.icon_small;
                EnableToolStripMenuItem.Enabled = EnableToolStripMenuItem.Checked = true;
                EnableToolStripMenuItem.Text = "已启用 (&E)";
            }
            else if (v == 1)
            {
                nicon.Text = title + " - 正在工作";
                nicon.Icon = Icon = Properties.Resources.working_small;
                EnableToolStripMenuItem.Text = "正在工作 (&E)";
                EnableToolStripMenuItem.Enabled = false;
            }
        }

        public void msg(string str, string t = "", ToolTipIcon msgtype = ToolTipIcon.Info)
        {
            string tit = string.IsNullOrEmpty(t) ? title : title + " - " + t;
            if (nicon.Visible && !Properties.Settings.Default.hidemsg)
            {
                nicon.ShowBalloonTip(1000, tit, str, msgtype);
            }
            Program.log(tit + "：" + str);
        }

        public void msg(string str, ToolTipIcon msgtype)
        {
            msg(str, "", msgtype);
        }

        public void error(string msg, string title = "错误")
        {
            Program.log(title + "：" + msg.Replace("\r\n", " "), 2);
            if (nicon.Visible)
            {
                MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void success(string msg, string title = "操作完成")
        {
            Program.log(title + "：" + msg.Replace("\r\n", " "));
            if (nicon.Visible)
            {
                MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ExitXToolStripMenuItem_Click(object sender, EventArgs e)
        {
            nicon.Dispose();
            Environment.Exit(0);
        }
        public const int WM_DEVICEDELETEBEFOR = 0x218;//U盘插入后，OS的底层会自动检测到，然后向应用程序发送“硬件设备状态改变“的消息
        public const int DBT_DEVICEARRIVAL = 0x8000;  //就是用来表示U盘可用的。一个设备或媒体已被插入一块，现在可用。
        public const int DBT_DEVICEQUERYREMOVE = 0x8001;  //审批要求删除一个设备或媒体作品。任何应用程序也不能否认这一要求，并取消删除。
        public const int DBT_DEVICEQUERYREMOVEFAILED = 0x8002;  //请求删除一个设备或媒体片已被取消。
        public const int DBT_DEVICEREMOVECOMPLETE = 0x8004;  //一个设备或媒体片已被删除。
        public const int DBT_DEVICEREMOVEPENDING = 0x8003;  //一个设备或媒体一块即将被删除。不能否认的。
        public const int DBT_DEVICEREMOVEBEFORE = 0x13;  //一个设备或媒体一块即将被删除。不能否认的。
        public const int WM_CLIPBOARDUPDATE = 0x031D; //粘贴板
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_CLIPBOARDUPDATE)
            {
                this.setSingleThread();
            }
            else if (m.Msg == WM_DEVICECHANGE)
            {
                int wp = m.WParam.ToInt32();
                //存储设备插/拔/弹
                if (wp == DBT_DEVICEARRIVAL || wp == DBT_DEVICEQUERYREMOVE || wp == DBT_DEVICEREMOVECOMPLETE || wp == DBT_DEVICEREMOVEPENDING || wp == DBT_DEVICEQUERYREMOVEFAILED)
                {
                    DEV_BROADCAST_HDR dbhdr = (DEV_BROADCAST_HDR)Marshal.PtrToStructure(m.LParam, typeof(DEV_BROADCAST_HDR));
                    if (dbhdr.dbch_devicetype == 2)
                    {
                        DEV_BROADCAST_VOLUME dbv = (DEV_BROADCAST_VOLUME)Marshal.PtrToStructure(m.LParam, typeof(DEV_BROADCAST_VOLUME));
                        if (dbv.dbcv_flags == 0)
                        {
                            char[] volums = GetVolumes(dbv.dbcv_unitmask);
                            disk = volums[0].ToString() + ":";
                            path = dir + "USB接口使用记录"+ DateTime.Today.Year + DateTime.Today.Month + DateTime.Today.Day + ".xlsx";
                            if (wp == DBT_DEVICEARRIVAL) //存储设备插入
                            {
                                WatcherStrat(disk, "*.*");
                                hDevInfo = SetupDiGetClassDevs(ref guidHID, 0, IntPtr.Zero, DIGCF.DIGCF_PRESENT | DIGCF.DIGCF_DEVICEINTERFACE);
                                if (!File.Exists(path))
                                {
                                    this.CreateExcelFile(path);
                                }
                                AddClipboardFormatListener(this.Handle);
                                Console.WriteLine("开始监听剪切板");
                                Console.WriteLine(dir);
                                diskinfo = new ManagementObject("win32_logicaldisk.deviceid=\"" + disk + "\"");
                                string diskdir;
                                object diskserdata = diskinfo.Properties["VolumeSerialNumber"].Value;
                                object disknamedata = diskinfo.Properties["VolumeName"].Value;
                                if (disknamedata != null)
                                {
                                    diskname = disknamedata.ToString();
                                }
                                diskser = diskserdata.ToString();
                                diskdir = diskser;
                                msg("欢迎光临贵州省林业调查规划院," + System.Environment.NewLine + "复制请使用Ctrl+C或右键复制粘贴。" + System.Environment.NewLine + "若弹出时提示占用，请右击停用。谢谢合作！");
                                diskClass diskObj = new diskClass();
                                if (diskname == "" || diskname == null)
                                {
                                    diskname = "可移动磁盘（" + disk + ")";
                                }
                                diskObj.diskname = diskname;
                                diskObj.diskdir = diskdir;
                                diskMap.Add(disk, diskObj);
                                this.WriteToExcel(path, diskname, DateTime.Now.ToString(), diskdir, "插入");
                                hDevInfo = SetupDiGetClassDevs(ref guidHID, 0, IntPtr.Zero, DIGCF.DIGCF_PRESENT | DIGCF.DIGCF_DEVICEINTERFACE);
                                WatchStartOrSopt(true);
                            }
                            else  //存储设备拔/弹出
                            {
                                try
                                {
                                    diskClass diskStr = (diskClass)diskMap[disk];
                                    this.WriteToExcel(path, diskStr.diskname, DateTime.Now.ToString(), diskStr.diskdir, "拔出");
                                    diskMap.Remove(disk);
                                    RemoveClipboardFormatListener(this.Handle);
                                    Console.WriteLine("结束监听剪切板");
                                }
                                catch (Exception) { }
                            }
                        }
                    }
                }
            }
            base.WndProc(ref m);
        }

        /// <summary>
        /// 根据驱动器掩码返回驱动器号数组
        /// </summary>
        /// <param name="Mask">掩码</param>
        /// <returns>返回驱动器号数组</returns>
        public static char[] GetVolumes(UInt32 Mask)
        {
            List<char> Volumes = new List<char>();

            for (int i = 0; i < 32; i++)
            {
                uint p = (uint)Math.Pow(2, i);
                if ((p | Mask) == p)
                {
                    Volumes.Add((char)('A' + i));
                }
            }
            return Volumes.ToArray();
        }

        private void EnableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EnableToolStripMenuItem.Checked = !EnableToolStripMenuItem.Checked;
            EnableToolStripMenuItem.Text = EnableToolStripMenuItem.Checked ? "已启用 (&E)" : "未启用 (&E)";
        }
        [StructLayout(LayoutKind.Sequential)]
        struct DEV_BROADCAST_HDR
        {
            public UInt32 dbch_size;
            public UInt32 dbch_devicetype;
            public UInt32 dbch_reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct DEV_BROADCAST_VOLUME
        {
            public UInt32 dbcv_size;
            public UInt32 dbcv_devicetype;
            public UInt32 dbcv_reserved;
            public UInt32 dbcv_unitmask;
            public UInt16 dbcv_flags;
        }

        private bool checkExt(string ext)
        {
            if (string.IsNullOrEmpty(ext) && Properties.Settings.Default.copynoext) return true;
            string extn = ext.Substring(1);
            switch (Properties.Settings.Default.mode)
            {
                case 1: //黑
                    if (black.Contains(extn))
                    {
                        return false;
                    }
                    return true;
                case 2: //白
                    if (white.Contains(extn))
                    {
                        return true;
                    }
                    return false;
                default:
                    return true;
            }
        }

        private void HideHToolStripMenuItem_Click(object sender, EventArgs e)
        {
            nicon.Visible = nicon.Visible ? false : true;
        }

        private void SettingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Thread th = new Thread(() =>
            {
                Setting dia = new Setting();
                dia.host = this;
                Application.Run(dia);
            });
            th.SetApartmentState(ApartmentState.STA);
            th.Start();
        }

        private void FileStripMenuItem_Click(object sender, EventArgs e)
        {
            openFile();
        }

        public void openFile()
        {
            try
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                Process.Start("explorer.exe", "\"" + dir + "\"");
            }
            catch (Exception ex)
            {
                error("打开失败：" + ex.ToString());
            }
        }
        private void clearLog_Click(object sender, EventArgs e)
        {
            Program.logger.Clear();
        }

        private void diskUUIDList_Click(object sender, EventArgs e)
        {
            (new diskUUID()).Show();
        }
        private void CreateExcelFile(string FileName)
        {
            //create  
            object Nothing = System.Reflection.Missing.Value;
            var app = new Excel.Application();
            app.Visible = false;
            Excel.Workbook workBook = app.Workbooks.Add(Nothing);
            Excel.Worksheet worksheet = (Excel.Worksheet)workBook.Sheets[1];
            Excel.Range range = (Excel.Range)worksheet.get_Range("A1", "D1");
            range.Font.Size = 15;
            range.Font.Name = "黑体";
            worksheet.Name = "record";
            range.ColumnWidth = 25;
            range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            range.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            //headline  
            worksheet.Cells[1, 1] = "设备名称";
            worksheet.Cells[1, 2] = "发生时间";
            worksheet.Cells[1, 3] = "设备序列号";
            worksheet.Cells[1, 4] = "操作类型";

            worksheet.SaveAs(FileName, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Excel.XlSaveAsAccessMode.xlNoChange, Type.Missing, Type.Missing, Type.Missing);
            workBook.Close(false, Type.Missing, Type.Missing);
            app.Quit();
        }

        private void WriteToExcel(string excelName, string USBName, string occurDate, string serial, string content)
        {
            object Nothing = System.Reflection.Missing.Value;
            var app = new Excel.Application();
            app.Visible = false;
            Excel.Workbook mybook = app.Workbooks.Open(excelName, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing);
            Excel.Worksheet mysheet = (Excel.Worksheet)mybook.Worksheets[1];
            mysheet.Activate();
            Console.WriteLine(excelName + USBName + occurDate + serial + content);
            //get activate sheet max row count  
            int maxrow = mysheet.UsedRange.Rows.Count + 1;
            Excel.Range range = (Excel.Range)mysheet.get_Range("A"+ maxrow, "D"+ maxrow);
            range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            range.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;

            mysheet.Cells[maxrow, 1] = USBName;
            mysheet.Cells[maxrow, 2] = occurDate;
            mysheet.Cells[maxrow, 3] = serial;
            mysheet.Cells[maxrow, 4] = content;
            mybook.Save();
            mybook.Close(false, Type.Missing, Type.Missing);
            mybook = null;
            //quit excel app  
            app.Quit();
        }
        public void setSingleThread()
        {
            Thread thread1 = new Thread(new ThreadStart(getData));
            thread1.SetApartmentState(ApartmentState.STA);     //<--
            thread1.Start();
        }
        public  void getData()
        {
            try
            {
                //根据.net自带Clipboard类，获取裁切板中的数据
                if (Clipboard.ContainsFileDropList())
                {
                    System.Collections.Specialized.StringCollection file = Clipboard.GetFileDropList();
                    IList fileIList = (IList)file;
                    if (fileIList.Count > 0)
                    {
                        Console.WriteLine(fileIList[0]);
                        filePath = fileIList[0].ToString();
                       if (filePath.StartsWith(disk))
                        {
                            Console.WriteLine("U盘数据被复制");
                            diskClass diskStr = (diskClass)diskMap[disk];
                            if (copytime!= DateTime.Now.ToString() || copyname != filePath || copydiskdir != diskStr.diskdir)
                            {
                                copyname = filePath;
                                copytime = DateTime.Now.ToString();
                                copydiskdir = diskStr.diskdir;
                                this.WriteToExcel(path, diskStr.diskname, DateTime.Now.ToString(), diskStr.diskdir, "拷入:" + filePath);
                            }
                        }
                    }
                }
                
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

        }
        private void WatcherStrat(string StrWarcherPath, string FilterType)
        {
            //初始化监听
            watcher.BeginInit();
            //设置监听文件类型
            watcher.Filter = FilterType;
            //设置需要监听的更改类型(如:文件或者文件夹的属性,文件或者文件夹的创建时间;NotifyFilters枚举的内容)
            watcher.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.Security | NotifyFilters.Size;
            //设置监听的路径
            Thread.Sleep(1000);
            watcher.Path = StrWarcherPath;
            //注册创建文件或目录时的监听事件
            watcher.Created += new FileSystemEventHandler(watch_created);
            //注册当指定目录的文件或者目录发生改变的时候的监听事件
            //watcher.Changed += new FileSystemEventHandler(watch_changed);
            //注册当删除目录的文件或者目录的时候的监听事件
            //watcher.Deleted += new FileSystemEventHandler(watch_deleted);
            //当指定目录的文件或者目录发生重命名的时候的监听事件
            //watcher.Renamed += new RenamedEventHandler(watch_renamed);
            //结束初始化
            watcher.EndInit();
        }
        private void watch_created(object sender, FileSystemEventArgs e)
        {
            //事件内容
            Console.WriteLine("create:" + e.FullPath);
            diskClass diskStr = (diskClass)diskMap[disk];
            if (filePath == e.FullPath)
            {
                if (copytime != DateTime.Now.ToString() || copyname != filePath || copydiskdir != diskStr.diskdir)
                {
                    copyname = filePath;
                    copytime = DateTime.Now.ToString();
                    copydiskdir = diskStr.diskdir;
                    this.WriteToExcel(path, diskStr.diskname, DateTime.Now.ToString(), diskStr.diskdir, "拷入:" + e.FullPath);
                }
            }
            else
            {
                if (copytime != DateTime.Now.ToString() || copyname != filePath || copydiskdir != diskStr.diskdir)
                {
                    copyname = filePath;
                    copytime = DateTime.Now.ToString();
                    copydiskdir = diskStr.diskdir;
                    this.WriteToExcel(path, diskStr.diskname, DateTime.Now.ToString(), diskStr.diskdir, "拷出:" + e.FullPath);
                }
                
            }
        }

        /// <summary>
        /// 当指定目录的文件或者目录发生改变的时候的监听事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void watch_changed(object sender, FileSystemEventArgs e)
        {
            //事件内容
            Console.WriteLine("change:" + e.FullPath);
            diskClass diskStr = (diskClass)diskMap[disk];
            this.WriteToExcel(path, diskStr.diskname, DateTime.Now.ToString(), diskStr.diskdir, "修改:" + e.FullPath);
        }
        /// <summary>
        /// 当删除目录的文件或者目录的时候的监听事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void watch_deleted(object sender, FileSystemEventArgs e)
        {
            //事件内容
            Console.WriteLine("del:" + e.FullPath);
            diskClass diskStr = (diskClass)diskMap[disk];
            this.WriteToExcel(path, diskStr.diskname, DateTime.Now.ToString(), diskStr.diskdir, "删除:" + e.FullPath);
        }
        /// <summary>
        /// 当指定目录的文件或者目录发生重命名的时候的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void watch_renamed(object sender, RenamedEventArgs e)
        {
            //事件内容
            Console.WriteLine("rename:" + e.FullPath);
            diskClass diskStr = (diskClass)diskMap[disk];
            this.WriteToExcel(path, diskStr.diskname, DateTime.Now.ToString(), diskStr.diskdir, "重命名:" + e.FullPath);
        }
        /// <summary>
        /// 启动或者停止监听
        /// </summary>
        /// <param name="IsEnableRaising">True:启用监听,False:关闭监听</param>
        private void WatchStartOrSopt(bool IsEnableRaising)
        {
            watcher.EnableRaisingEvents = IsEnableRaising;
        }
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            WatchStartOrSopt(false);
            //RemoveDriveTools.EjectImpl(@"G:");
        }
    }
    public partial class diskClass
    {
        public string diskname = null;
        public string diskdir = null;
    }
}
