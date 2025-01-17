﻿#region License Information (GPL v3)

/*
    ShareX - A program that allows you to take screenshots and share any file type
    Copyright (c) 2007-2019 ShareX Team

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)

using ShareX.HelpersLib;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShareX.IndexerLib
{
    public partial class DirectoryIndexerForm : Form
    {
        public delegate void UploadRequestedEventHandler(string source);
        public event UploadRequestedEventHandler UploadRequested;

        public IndexerSettings Settings { get; set; }
        public string Source { get; private set; }

        public DirectoryIndexerForm(IndexerSettings settings)
        {
            InitializeComponent();
            ShareXResources.ApplyThemeToForm(this);

            Settings = settings;
            pgSettings.SelectedObject = Settings;
        }

        private async void DirectoryIndexerForm_Load(object sender, EventArgs e)
        {
            await BrowseFolder();
        }

        private async void btnBrowseFolder_Click(object sender, EventArgs e)
        {
            await BrowseFolder();
        }

        private async Task BrowseFolder()
        {
            if (Helpers.BrowseFolder(txtFolderPath))
            {
                await IndexFolder();
            }
        }

        private void txtFolderPath_TextChanged(object sender, EventArgs e)
        {
            btnIndexFolder.Enabled = txtFolderPath.TextLength > 0;
        }

        private async void btnIndexFolder_Click(object sender, EventArgs e)
        {
            await IndexFolder();
        }

        private async Task IndexFolder()
        {
            string folderPath = txtFolderPath.Text;

            if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
            {
                btnIndexFolder.Enabled = false;
                btnUpload.Enabled = false;

                await Task.Run(() =>
                {
                    Source = Indexer.Index(folderPath, Settings);
                });

                if (!IsDisposed)
                {
                    if (!string.IsNullOrEmpty(Source))
                    {
                        tcMain.SelectedTab = tpPreview;

                        if (Settings.Output == IndexerOutput.Html)
                        {
                            txtPreview.Visible = false;
                            wbPreview.Visible = true;
                            wbPreview.DocumentText = Source;
                        }
                        else
                        {
                            wbPreview.Visible = false;
                            txtPreview.Visible = true;
                            txtPreview.Text = Source;
                        }

                        btnUpload.Enabled = true;
                    }

                    btnIndexFolder.Enabled = true;
                }
            }
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Source))
            {
                OnUploadRequested(Source);
                Close();
            }
        }

        protected void OnUploadRequested(string source)
        {
            if (UploadRequested != null)
            {
                UploadRequested(source);
            }
        }
    }
}