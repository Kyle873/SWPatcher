﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Linq;
using System.Windows.Forms;
using SWPatcher.General;
using SWPatcher.Helpers;
using SWPatcher.Helpers.GlobalVar;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Runtime.Serialization.Formatters.Binary;
using SWPatcher.Downloading;

namespace SWPatcher.Forms
{
    public partial class Main : Form
    {
        public enum States
        {
            Idle = 0,
            CheckingVersion,
            Downloading,
            Patching
        }

        private readonly BackgroundWorker WorkerPatch;
        public List<SWFile> SWFiles { get; private set; }
        public States _state;
        public Downloader Downloader { get; private set; }

        public States State
        {
            get
            {
                return _state;
            }
            private set
            {
                if (_state != value)
                {
                    switch (value)
                    {
                        case States.Idle:
                            comboBoxLanguages.Enabled = true;
                            buttonLastest.Enabled = true;
                            buttonLastest.Text = Strings.FormText.Download;
                            buttonPatch.Enabled = true;
                            buttonPatch.Text = Strings.FormText.Patch;
                            toolStripStatusLabel.Text = Strings.FormText.Status.Idle;
                            toolStripProgressBar.Value = toolStripProgressBar.Minimum;
                            break;
                        case States.CheckingVersion:
                            comboBoxLanguages.Enabled = false;
                            buttonLastest.Enabled = false;
                            buttonLastest.Text = Strings.FormText.Download;
                            buttonPatch.Enabled = false;
                            buttonPatch.Text = Strings.FormText.Patch;
                            toolStripStatusLabel.Text = Strings.FormText.Status.Check;
                            toolStripProgressBar.Value = toolStripProgressBar.Minimum;
                            break;
                        case States.Downloading:
                            comboBoxLanguages.Enabled = false;
                            buttonLastest.Enabled = true;
                            buttonLastest.Text = Strings.FormText.Cancel;
                            buttonPatch.Enabled = false;
                            buttonPatch.Text = Strings.FormText.Patch;
                            toolStripStatusLabel.Text = Strings.FormText.Status.Download;
                            toolStripProgressBar.Value = toolStripProgressBar.Minimum;
                            break;
                        case States.Patching:
                            comboBoxLanguages.Enabled = false;
                            buttonLastest.Enabled = false;
                            buttonLastest.Text = Strings.FormText.Download;
                            buttonPatch.Enabled = true;
                            buttonPatch.Text = Strings.FormText.Cancel;
                            toolStripStatusLabel.Text = Strings.FormText.Status.Patch;
                            toolStripProgressBar.Value = toolStripProgressBar.Minimum;
                            break;
                    }
                    _state = value;
                }
            }
        }

        public Main()
        {
            SWFiles = new List<SWFile>();
            this.WorkerPatch = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            this.WorkerPatch.DoWork += new DoWorkEventHandler(workerPatch_DoWork);
            this.WorkerPatch.ProgressChanged += new ProgressChangedEventHandler(workerPatch_ProgressChanged);
            this.WorkerPatch.RunWorkerCompleted += new RunWorkerCompletedEventHandler(workerPatch_RunWorkerCompleted);
            this.Downloader = Downloader.Instance;  
            this.Downloader.DownloaderProgressChanged += new DownloaderProgressChangedEventHandler(Downloader_DownloaderProgressChanged);
            this.Downloader.DownloaderCompleted += new DownloaderCompletedEventHandler(Downloader_DownloaderCompleted);
            InitializeComponent();
            this.Text = AssemblyAccessor.Title + " " + AssemblyAccessor.Version;
            this.buttonLastest.Text = Strings.FormText.Download;
            this.buttonPatch.Text = Strings.FormText.Patch;
            this.toolStripStatusLabel.Text = Strings.FormText.Status.Idle;
        }

        private void Downloader_DownloaderProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Downloader_DownloaderCompleted(object sender, AsyncCompletedEventArgs e)
        {
            
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog(this);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            RestoreBackup();
            if (CheckForProgramUpdate() || CheckForProgramFolderMalfunction(Path.GetDirectoryName(Paths.PatcherRoot)) || CheckForSWPath() || CheckForGameClientUpdate() || (comboBoxLanguages.DataSource = GetAllAvailableLanguages()) == null)
                return;
        }

        private void buttonLastest_Click(object sender, EventArgs e)
        {
            this.Downloader = Downloader.Instance;
            if (this.State == States.Downloading)
            {
                Downloader.Cancel();
                this.State = States.Idle;
                return;
            }
            SWFiles.Clear();
            this.Downloader.DownloadList.Clear();
            this.State = States.Downloading;
            if (Downloader.DownloadList.Count > 0)
                Downloader.Run(this.comboBoxLanguages.SelectedItem.ToString());
        }

        private void buttonPatch_Click(object sender, EventArgs e)
        {
            this.State = States.Patching;
            WorkerPatch.RunWorkerAsync(this.comboBoxLanguages.SelectedItem.ToString());
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
