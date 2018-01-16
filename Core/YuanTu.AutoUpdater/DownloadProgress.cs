using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace YuanTu.AutoUpdater
{
    public partial class DownloadProgress : Form
    {
        #region The constructor of DownloadProgress

        public DownloadProgress(List<DownloadFileInfo> downloadFileListTemp)
        {
            InitializeComponent();

            downloadFileList = downloadFileListTemp;
            allFileList = new List<DownloadFileInfo>();
            foreach (var file in downloadFileListTemp)
            {
                allFileList.Add(file);
            }
        }

        #endregion

        #region The private fields

        private readonly List<DownloadFileInfo> allFileList;
        private readonly List<DownloadFileInfo> downloadFileList;
        private WebClient clientDownload;
        private ManualResetEvent evtDownload;
        private ManualResetEvent evtPerDonwload;
        private bool isFinished;

        #endregion

        #region The method and event

        private long nDownloadedTotal;
        private long total;

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (!isFinished &&
                DialogResult.No ==
                MessageBox.Show(ConstFile.CANCELORNOT, ConstFile.MESSAGETITLE, MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question))
            {
                e.Cancel = true;
            }
            if (clientDownload != null)
                clientDownload.CancelAsync();

            evtDownload.Set();
            evtPerDonwload.Set();
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            evtDownload = new ManualResetEvent(true);
            evtDownload.Reset();
            ThreadPool.QueueUserWorkItem(ProcDownload);
        }

        private void ProcDownload(object o)
        {
            var tempFolderPath = Path.Combine(CommonUnitity.SystemBinUrl, ConstFile.TEMPFOLDERNAME);
            if (!Directory.Exists(tempFolderPath))
            {
                Directory.CreateDirectory(tempFolderPath);
            }


            evtPerDonwload = new ManualResetEvent(false);

            foreach (var file in downloadFileList)
            {
                total += file.Size;
            }
            try
            {
                while (!evtDownload.WaitOne(0, false))
                {
                    if (downloadFileList.Count == 0)
                        break;

                    var file = downloadFileList[0];


                    //Debug.WriteLine(String.Format("Start Download:{0}", file.FileName));

                    ShowCurrentDownloadFileName(file.FileName);

                    //Download
                    clientDownload = new WebClient();

                    //Added the function to support proxy
                    clientDownload.Proxy = WebRequest.GetSystemWebProxy();
                    clientDownload.Proxy.Credentials = CredentialCache.DefaultCredentials;
                    clientDownload.Credentials = CredentialCache.DefaultCredentials;
                    //End added

                    clientDownload.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs e) =>
                    {
                        try
                        {
                            SetProcessBar(e.ProgressPercentage, (int) ((nDownloadedTotal + e.BytesReceived)*100/total));
                        }
                        catch (Exception ex)
                        {
                            //log the error message,you can use the application's log code
                            CommonUnitity.log(ex.Message + "\n" + ex.StackTrace);
                        }
                    };

                    clientDownload.DownloadFileCompleted += (object sender, AsyncCompletedEventArgs e) =>
                    {
                        try
                        {
                            DealWithDownloadErrors();
                            var dfile = e.UserState as DownloadFileInfo;
                            nDownloadedTotal += dfile.Size;
                            SetProcessBar(0, (int) (nDownloadedTotal*100/total));
                            evtPerDonwload.Set();
                        }
                        catch (Exception ex)
                        {
                            //log the error message,you can use the application's log code
                            CommonUnitity.log(ex.Message + "\n" + ex.StackTrace);
                        }
                    };

                    evtPerDonwload.Reset();

                    //Download the folder file
                    var tempFolderPath1 = CommonUnitity.GetFolderUrl(file);
                    if (!string.IsNullOrEmpty(tempFolderPath1))
                    {
                        tempFolderPath = Path.Combine(CommonUnitity.SystemBinUrl, ConstFile.TEMPFOLDERNAME);
                        tempFolderPath += tempFolderPath1;
                    }
                    else
                    {
                        tempFolderPath = Path.Combine(CommonUnitity.SystemBinUrl, ConstFile.TEMPFOLDERNAME);
                    }

                    clientDownload.DownloadFileAsync(new Uri(file.DownloadUrl),
                        Path.Combine(tempFolderPath, file.FileName), file);

                    //Wait for the download complete
                    evtPerDonwload.WaitOne();

                    clientDownload.Dispose();
                    clientDownload = null;

                    //Remove the downloaded files
                    downloadFileList.Remove(file);
                }
            }
            catch (Exception ex)
            {
                CommonUnitity.log(ex.Message + "\n" + ex.StackTrace);
                ShowErrorAndRestartApplication();
                //throw;
            }

            //When the files have not downloaded,return.
            if (downloadFileList.Count > 0)
            {
                return;
            }

            //Test network and deal with errors if there have 
            DealWithDownloadErrors();

            //Debug.WriteLine("All Downloaded");
            foreach (var file in allFileList)
            {
                var tempUrlPath = CommonUnitity.GetFolderUrl(file);
                var oldPath = string.Empty;
                var newPath = string.Empty;
                try
                {
                    if (!string.IsNullOrEmpty(tempUrlPath))
                    {
                        oldPath = Path.Combine(CommonUnitity.SystemBinUrl + tempUrlPath.Substring(1), file.FileName);
                        newPath = Path.Combine(CommonUnitity.SystemBinUrl + ConstFile.TEMPFOLDERNAME + tempUrlPath,
                            file.FileName);
                    }
                    else
                    {
                        oldPath = Path.Combine(CommonUnitity.SystemBinUrl, file.FileName);
                        newPath = Path.Combine(CommonUnitity.SystemBinUrl + ConstFile.TEMPFOLDERNAME, file.FileName);
                    }

                    //just deal with the problem which the files EndsWith xml can not download
                    var f = new FileInfo(newPath);
                    if (!file.Size.ToString().Equals(f.Length.ToString()) && !file.FileName.EndsWith(".xml"))
                    {
                        ShowErrorAndRestartApplication();
                    }


                    //Added for dealing with the config file download errors
                    var newfilepath = string.Empty;
                    if (newPath.Substring(newPath.LastIndexOf(".") + 1).Equals(ConstFile.CONFIGFILEKEY))
                    {
                        if (File.Exists(newPath))
                        {
                            if (newPath.EndsWith("_"))
                            {
                                newfilepath = newPath;
                                newPath = newPath.Substring(0, newPath.Length - 1);
                                oldPath = oldPath.Substring(0, oldPath.Length - 1);
                            }
                            File.Move(newfilepath, newPath);
                        }
                    }
                    //End added

                    if (File.Exists(oldPath))
                    {
                        MoveFolderToOld(oldPath, newPath);
                    }
                    else
                    {
                        //Edit for config_ file
                        if (!string.IsNullOrEmpty(tempUrlPath))
                        {
                            if (!Directory.Exists(CommonUnitity.SystemBinUrl + tempUrlPath.Substring(1)))
                            {
                                Directory.CreateDirectory(CommonUnitity.SystemBinUrl + tempUrlPath.Substring(1));


                                MoveFolderToOld(oldPath, newPath);
                            }
                            else
                            {
                                MoveFolderToOld(oldPath, newPath);
                            }
                        }
                        else
                        {
                            MoveFolderToOld(oldPath, newPath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //log the error message,you can use the application's log code
                    CommonUnitity.log(ex.Message + "\n" + ex.StackTrace);
                }
            }

            //After dealed with all files, clear the data
            allFileList.Clear();

            if (downloadFileList.Count == 0)
                Exit(true);
            else
                Exit(false);

            evtDownload.Set();
        }

        //To delete or move to old files
        private void MoveFolderToOld(string oldPath, string newPath)
        {
            if (File.Exists(oldPath + ".old"))
                File.Delete(oldPath + ".old");

            if (File.Exists(oldPath))
                File.Move(oldPath, oldPath + ".old");


            File.Move(newPath, oldPath);
            //File.Delete(oldPath + ".old");
        }

        private void ShowCurrentDownloadFileName(string name)
        {
            if (labelCurrentItem.InvokeRequired)
            {
                ShowCurrentDownloadFileNameCallBack cb = ShowCurrentDownloadFileName;
                Invoke(cb, name);
            }
            else
            {
                labelCurrentItem.Text = name;
            }
        }

        private void SetProcessBar(int current, int total)
        {
            if (progressBarCurrent.InvokeRequired)
            {
                SetProcessBarCallBack cb = SetProcessBar;
                Invoke(cb, current, total);
            }
            else
            {
                progressBarCurrent.Value = current;
                progressBarTotal.Value = total;
            }
        }

        private void Exit(bool success)
        {
            if (InvokeRequired)
            {
                ExitCallBack cb = Exit;
                Invoke(cb, success);
            }
            else
            {
                isFinished = success;
                DialogResult = success ? DialogResult.OK : DialogResult.Cancel;
                Close();
            }
        }

        private void OnCancel(object sender, EventArgs e)
        {
            //bCancel = true;
            //evtDownload.Set();
            //evtPerDonwload.Set();
            ShowErrorAndRestartApplication();
        }

        private void DealWithDownloadErrors()
        {
            try
            {
                //Test Network is OK or not.
                var config = Config.LoadConfig(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConstFile.FILENAME));
                var client = new WebClient();
                client.DownloadString(config.ServerUrl);
            }
            catch (Exception ex)
            {
                //log the error message,you can use the application's log code
                CommonUnitity.log(ex.Message + "\n" + ex.StackTrace);
                ShowErrorAndRestartApplication();
            }
        }

        private void ShowErrorAndRestartApplication()
        {
            MessageBox.Show(ConstFile.NOTNETWORK, ConstFile.MESSAGETITLE, MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            CommonUnitity.RestartApplication();
        }

        private delegate void ExitCallBack(bool success);

        private delegate void SetProcessBarCallBack(int current, int total);

        private delegate void ShowCurrentDownloadFileNameCallBack(string name);

        #endregion
    }
}