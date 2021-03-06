using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace FileChangeNotifier
{
    public partial class frmNotifier : Form
    {
        private StringBuilder m_Sb;
        private StringBuilder textcontent;
        private bool m_bDirty;
        private System.IO.FileSystemWatcher m_Watcher;
        private bool m_bIsWatching;
        private NotifyIcon trayIcon;
        private string destRootPath = @"D:\2016 projects\LEASE\LEASE201612\stuff\bridge_stuff\20170504\no2";

        // It will be used at multithread construcstion
        private List<string> listCopyFile; 

        public frmNotifier()
        {
            InitializeComponent();
            m_Sb = new StringBuilder();
            textcontent = new StringBuilder();
            textcontent.Append(string.Empty);
            m_bDirty = false;
            m_bIsWatching = false;
            listCopyFile = new List<string>();

            // Initialize Tray Icon
            trayIcon = new NotifyIcon()
            {
                Icon = FileSystemWatcher.Properties.Resources.Icon,
                ContextMenu = new ContextMenu(new MenuItem[] {
                new MenuItem("Exit", Exit)
            }),
                Visible = true
            };
            trayIcon.DoubleClick += this.trayIcon_DoubleClick;
        }

        /// <summary>
        /// Closing Method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Exit(object sender, EventArgs e)
        {
            // Hide tray icon, otherwise it will remain shown until user mouses over it
            trayIcon.Visible = true;
            this.Close();
            Environment.Exit(0);
        }

        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            this.Show();
        }

        private void btnWatchFile_Click(object sender, EventArgs e)
        {


            if (m_bIsWatching)
            {
                m_bIsWatching = false;
                m_Watcher.EnableRaisingEvents = false;
                m_Watcher.Dispose();
                btnWatchFile.BackColor = Color.LightSkyBlue;
                btnWatchFile.Text = "Start Watching";

            }
            else
            {
                m_bIsWatching = true;
                btnWatchFile.BackColor = Color.Red;
                btnWatchFile.Text = "Stop Watching";

                m_Watcher = new System.IO.FileSystemWatcher();
                if (rdbDir.Checked)
                {
                    m_Watcher.Filter = "*.*";
                    m_Watcher.Path = txtFile.Text + "\\";
                }
                else
                {
                    m_Watcher.Filter = txtFile.Text.Substring(txtFile.Text.LastIndexOf('\\') + 1);
                    m_Watcher.Path = txtFile.Text.Substring(0, txtFile.Text.Length - m_Watcher.Filter.Length);
                }

                if (chkSubFolder.Checked)
                {
                    m_Watcher.IncludeSubdirectories = true;
                }

                m_Watcher.NotifyFilter = NotifyFilters.LastWrite
                                     | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.CreationTime;
                m_Watcher.Changed += new FileSystemEventHandler(OnChanged);
                m_Watcher.Created += new FileSystemEventHandler(OnChanged);
                m_Watcher.Deleted += new FileSystemEventHandler(OnChanged);
                m_Watcher.Renamed += new RenamedEventHandler(OnRenamed);
                m_Watcher.EnableRaisingEvents = true;
            }
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            textcontent.Remove(0, textcontent.Length);
            textcontent.Append(e.FullPath.ToString());
            string fullpath = e.FullPath.ToString();
            if(!String.IsNullOrEmpty(fullpath))
            listCopyFile.Add(fullpath);

            string fullpath2 = "";
            FileAttributes attr = File.GetAttributes(fullpath);
            if (!((attr & FileAttributes.Directory) == FileAttributes.Directory))
            {
                if (String.Compare(fullpath2, fullpath) != 0)
                {
                    //fileCopy(fullpath, destRootPath);
                    fullpath2 = fullpath;
                }
            }
            textcontent.Append(Environment.NewLine);
            using (StreamWriter sw = File.AppendText(@"D:\2016 projects\LEASE\LEASE201612\stuff\bridge_stuff\20170504\textfile\demo.TXT"))
            {
                sw.WriteLine(textcontent.ToString());
            }
            if (!m_bDirty)
            {
                m_Sb.Remove(0, m_Sb.Length);
                m_Sb.Append(e.FullPath);
                m_Sb.Append(" ");
                m_Sb.Append(e.ChangeType.ToString());
                m_Sb.Append("    ");
                m_Sb.Append(DateTime.Now.ToString());
                m_bDirty = true;
                trayIcon.Visible = true;
                trayIcon.ShowBalloonTip(5000, "Warning", m_Sb.ToString(), ToolTipIcon.Info);
                //map newst file from server to local
                //fileCopy(e.FullPath, destRootPath);
            }
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            textcontent.Remove(0, textcontent.Length);
            if (!String.IsNullOrEmpty(e.FullPath))
                textcontent.Append(e.FullPath.ToString());
            textcontent.Append(Environment.NewLine);
            using (StreamWriter sw = File.AppendText(@"D:\2016 projects\LEASE\LEASE201612\stuff\bridge_stuff\20170504\textfile\demo.TXT"))
            {
                sw.WriteLine(textcontent.ToString());
            }
            if (!m_bDirty)
            {
                m_Sb.Remove(0, m_Sb.Length);
                m_Sb.Append(e.OldFullPath);
                m_Sb.Append(" ");
                m_Sb.Append(e.ChangeType.ToString());
                m_Sb.Append(" ");
                m_Sb.Append("to ");
                m_Sb.Append(e.Name);
                m_Sb.Append("    ");
                m_Sb.Append(DateTime.Now.ToString());
                m_bDirty = true;

                if (rdbFile.Checked)
                {
                    m_Watcher.Filter = e.Name;
                    m_Watcher.Path = e.FullPath.Substring(0, e.FullPath.Length - m_Watcher.Filter.Length);
                    trayIcon.Visible = true;
                    trayIcon.ShowBalloonTip(5000, "Warning", m_Sb.ToString(), ToolTipIcon.Info);
                    //map newst file from server to local
                    //File.Delete(e.OldFullPath);
                    //fileCopy(e.Name, destRootPath);
                }
            }
        }

        /// <summary>
        /// Timer tick event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmrEditNotify_Tick(object sender, EventArgs e)
        {
            if (m_bDirty)
            {
                lstNotification.BeginUpdate();
                lstNotification.Items.Add(m_Sb.ToString());
                lstNotification.EndUpdate();
                m_bDirty = false;
                while (listCopyFile.Count > 0)
                {
                    //frmNotifier formNotifier = new frmNotifier();
                    //Thread copyFileThread = new Thread(() => frmNotifier.fileCopy(listCopyFile[0], destRootPath));
                    //copyFileThread.Start();
                    //listCopyFile.RemoveAt(0);
                    //Thread.Sleep(500);
                    fileCopy(listCopyFile[0], destRootPath);
                    listCopyFile.RemoveAt(0);
                }

            }
        }

        /// <summary>
        /// Chose the path you want to watch
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBrowseFile_Click(object sender, EventArgs e)
        {
            if (rdbDir.Checked)
            {
                DialogResult resDialog = dlgOpenDir.ShowDialog();
                if (resDialog.ToString() == "OK")
                {
                    txtFile.Text = dlgOpenDir.SelectedPath;
                }
            }
            else
            {
                DialogResult resDialog = dlgOpenFile.ShowDialog();
                if (resDialog.ToString() == "OK")
                {
                    txtFile.Text = dlgOpenFile.FileName;
                }
            }
        }

        /// <summary>
        /// Save logs to a file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLog_Click(object sender, EventArgs e)
        {
            DialogResult resDialog = dlgSaveFile.ShowDialog();
            if (resDialog.ToString() == "OK")
            {
                FileInfo fi = new FileInfo(dlgSaveFile.FileName);
                StreamWriter sw = fi.CreateText();
                foreach (string sItem in lstNotification.Items)
                {
                    sw.WriteLine(sItem);
                }
                sw.Close();
            }
        }

        /// <summary>
        /// File watching mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rdbFile_CheckedChanged(object sender, EventArgs e)
        {
            if (rdbFile.Checked == true)
            {
                chkSubFolder.Enabled = false;
                chkSubFolder.Checked = false;
            }
        }

        /// <summary>
        /// Directory watching mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rdbDir_CheckedChanged(object sender, EventArgs e)
        {
            if (rdbDir.Checked == true)
            {
                chkSubFolder.Enabled = true;
            }
        }

        /// <summary>
        /// Application will run at tray back ground
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmNotifier_FormClosing(object sender, FormClosingEventArgs e)
        {
            trayIcon.Visible = true;
            this.Hide();
            e.Cancel = true;
        }

        /// <summary>
        /// This will be happen when you double click to icon in system tray
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trayIcon_DoubleClick(object sender, EventArgs e)
        {
            this.Show();
            this.Visible = true;
            this.WindowState = FormWindowState.Normal;
            this.TopMost = true;
        }

        /// <summary>
        /// Copy file from source path to destioation path
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="destFolderPath"></param>
        private static void fileCopy(string sourcePath, string destFolderPath)
        {
            //in case path is folder
            if (Directory.Exists(sourcePath))
            {
                string[] fileList = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);
                foreach (string s in fileList)
                {
                    try
                    {
                        FileInfo fileinfo = new FileInfo(sourcePath);
                        destFolderPath = destFolderPath + "\\" + s.Substring(sourcePath.Length - 1, (s.Length - sourcePath.Length));
                        File.Copy(s, destFolderPath, true);

                    }
                    catch (IOException ex)
                    {
                        MessageBox.Show(ex.Message.ToString());
                        return;
                    }
                }
            }

            // in case path is file 
            else
            {
                try
                {
                    FileInfo fileinfo = new FileInfo(sourcePath);
                    destFolderPath = destFolderPath + "\\" + sourcePath.Substring(sourcePath.IndexOf("wwwroot") + 7, (sourcePath.Length - sourcePath.IndexOf("wwwroot") - 7));
                    while (IsFileLocked(fileinfo))
                    {
                        Thread.Sleep(500);
                    }
                    File.Copy(sourcePath, destFolderPath, true);
                }

                catch (IOException ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                     return;
                }
            }
        }

        /// <summary>
        /// Show messagebox of content when you double click to list box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lstNotification_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = this.lstNotification.IndexFromPoint(e.Location);
            if (index != System.Windows.Forms.ListBox.NoMatches)
            {
                MessageBox.Show(lstNotification.Items[index].ToString(), "Message content");
            }
        }

        /// <summary>
        /// check if in case file in another process
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;
            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            return false;
        }

    }
}