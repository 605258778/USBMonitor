﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Management;
using System.Threading;
using System.Diagnostics;
using System.Linq;
using System.ComponentModel;
using Excel = Microsoft.Office.Interop.Excel;
using System.Collections;
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
        Hashtable diskMap = new Hashtable();
        public Dictionary<string, Thread> copyThread = new Dictionary<string, Thread>(); //正在复制文件的线程 分区号=>线程

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

        public const int WM_DEVICECHANGE = 0x219;//U盘插入后，OS的底层会自动检测到，然后向应用程序发送“硬件设备状态改变“的消息
        public const int DBT_DEVICEARRIVAL = 0x8000;  //就是用来表示U盘可用的。一个设备或媒体已被插入一块，现在可用。
        public const int DBT_DEVICEQUERYREMOVE = 0x8001;  //审批要求删除一个设备或媒体作品。任何应用程序也不能否认这一要求，并取消删除。
        public const int DBT_DEVICEQUERYREMOVEFAILED = 0x8002;  //请求删除一个设备或媒体片已被取消。
        public const int DBT_DEVICEREMOVECOMPLETE = 0x8004;  //一个设备或媒体片已被删除。
        public const int DBT_DEVICEREMOVEPENDING = 0x8003;  //一个设备或媒体一块即将被删除。不能否认的。

        protected override void DefWndProc(ref Message m)
        {
            if (m.Msg == WM_DEVICECHANGE)
            {
                
                int wp = m.WParam.ToInt32();
                //存储设备插/拔/弹
                if (wp == DBT_DEVICEARRIVAL || wp == DBT_DEVICEQUERYREMOVE || wp == DBT_DEVICEREMOVECOMPLETE || wp == DBT_DEVICEREMOVEPENDING) {
                    DEV_BROADCAST_HDR dbhdr = (DEV_BROADCAST_HDR)Marshal.PtrToStructure(m.LParam, typeof(DEV_BROADCAST_HDR));
                    if (dbhdr.dbch_devicetype == 2)
                    {
                        DEV_BROADCAST_VOLUME dbv = (DEV_BROADCAST_VOLUME)Marshal.PtrToStructure(m.LParam, typeof(DEV_BROADCAST_VOLUME));
                        if (dbv.dbcv_flags == 0)
                        {
                            char[] volums = GetVolumes(dbv.dbcv_unitmask);
                            string disk = volums[0].ToString() + ":";


                            string path = dir+"USB接口使用记录.xlsx";
                            if (!File.Exists(path))
                            {
                                this.CreateExcelFile(path);
                            }

                            if (wp == DBT_DEVICEARRIVAL) //存储设备插入
                            {
                                Console.WriteLine(dir);
                                ManagementObject diskinfo = new ManagementObject("win32_logicaldisk.deviceid=\"" + disk + "\"");
                                string diskser = "";
                                string diskname = "";
                                string diskdir;

                                object diskserdata = diskinfo.Properties["VolumeSerialNumber"].Value;
                                object disknamedata = diskinfo.Properties["VolumeName"].Value;
                                if (disknamedata != null)
                                {
                                    diskname = disknamedata.ToString();
                                }
                                if (diskserdata == null)
                                {
                                    if (string.IsNullOrEmpty(diskname))
                                    {
                                        diskdir = disk.Substring(0, 1);
                                    }
                                    else
                                    {
                                        diskdir = disk.Substring(0, 1) + " - " + diskname;
                                    }
                                    msg(disk, "存储设备已插入");
                                    Program.log("获取存储设备序列号失败，文件目录将命名为：" + diskdir);
                                }
                                else
                                {
                                    diskser = diskserdata.ToString();
                                    diskdir = diskser;
                                    msg(disk + " - " + diskser, "存储设备已插入");
                                }
                                diskClass diskObj = new diskClass();
                                diskObj.diskname = diskname;
                                diskObj.diskdir = diskdir;
                                diskMap.Add(disk, diskObj);
                                this.WriteToExcel(path, diskname, DateTime.Now.ToString(), diskdir, "插入");
                            }
                            else  //存储设备拔/弹出
                            {
                                try
                                {
                                    diskClass diskStr = (diskClass)diskMap[disk];
                                    this.WriteToExcel(path, diskStr.diskname, DateTime.Now.ToString(), diskStr.diskdir, "拔出");
                                    diskMap.Remove(disk);
                                }
                                catch (Exception) { }
                            }
                        }
                    }
                }
            }
            base.DefWndProc(ref m);
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

        /// <summary>
        /// 复制文件夹（及文件夹下所有子文件夹和文件）
        /// </summary>
        /// <param name="sourcePath">待复制的文件夹路径</param>
        /// <param name="destinationPath">目标路径</param>
        public void CopyDirectory(string sourcePath, string destinationPath)
        {
            try
            {
                DirectoryInfo info = new DirectoryInfo(sourcePath);
                Directory.CreateDirectory(destinationPath);
                foreach (FileSystemInfo fsi in info.GetFileSystemInfos())
                {
                    String destName = Path.Combine(destinationPath, fsi.Name);

                    if (fsi is FileInfo)
                    {   //如果是文件，复制文件
                        try
                        {
                            FileInfo fi1 = new FileInfo(fsi.FullName);
                            if (checkExt(fi1.Extension))
                            {
                                Program.log("复制文件：" + fsi.FullName);
                                if (File.Exists(destName))
                                {
                                    switch (Properties.Settings.Default.conflict)
                                    {
                                        case 0:
                                            FileInfo fi2 = new FileInfo(destName);
                                            if (fi1.LastWriteTime > fi2.LastWriteTime)
                                            {
                                                File.Copy(fsi.FullName, destName, true);
                                            }
                                            break;
                                        case 1:
                                            destName = (new Random()).Next(0, 9999999) + "-" + destName;
                                            File.Copy(fsi.FullName, destName);
                                            break;
                                        case 2:
                                            File.Copy(fsi.FullName, destName, true);
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                else
                                {
                                    File.Copy(fsi.FullName, destName);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Program.log("复制文件：" + destName + "：失败：" + ex.ToString(), 2);
                        }
                    }
                    else //如果是文件夹，新建文件夹，递归
                    {
                        try
                        {
                            Directory.CreateDirectory(destName);
                            CopyDirectory(fsi.FullName, destName);
                        }
                        catch (Exception ex)
                        {
                            Program.log("创建目录：" + destName + "：失败：" + ex.ToString(), 2);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Program.log("复制目录失败，设备可能被强行拔出：" + ex.ToString());
            }
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

        private void LogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openLogFile();
        }

        public void openLogFile()
        {
            if (!File.Exists(dir + "EventViewer.xml"))
            {
                File.WriteAllText(dir + "EventViewer.xml", Properties.Resources.EventViewer);
            }
            try
            {
                Process.Start("eventvwr.exe", "/v:\"" + dir + "EventViewer.xml" + "\"");
            }
            catch (Exception ex)
            {
                error("打开日志查看器失败：" + ex.ToString());
            }
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

        public void openBlog()
        {
            
        }

        private void BlogMenuItem_Click(object sender, EventArgs e)
        {
            openBlog();
        }

        private void nameMenuItem_Click(object sender, EventArgs e)
        {
            openPage();
        }

        public void openPage()
        {
           
        }

        private void nicon_MouseDoubleClick(object sender, MouseEventArgs e)
        {

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
            worksheet.Name = "record";
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
            //open  
            object Nothing = System.Reflection.Missing.Value;
            var app = new Excel.Application();
            app.Visible = false;
            Excel.Workbook mybook = app.Workbooks.Open(excelName, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing);
            Excel.Worksheet mysheet = (Excel.Worksheet)mybook.Worksheets[1];
            mysheet.Activate();
            Console.WriteLine(excelName + USBName + occurDate + serial + content);
            //get activate sheet max row count  
            int maxrow = mysheet.UsedRange.Rows.Count + 1;
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
    }
    public partial class diskClass{
        public string diskname = null;
        public string diskdir = null;
    }
}
