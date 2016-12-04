using System;
using System.Collections.Generic;
using Eden.Share.DataStructs;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace Eden.Update
{
    public partial class DownloadProgress : Form
    {
        #region The private fields
        private bool _isFinished;
        private List<DownloadFileInfo> _downloadFileList;
        private List<DownloadFileInfo> _allFileList;
        private ManualResetEvent _evtDownload;
        private ManualResetEvent _evtPerDonwload;
        private WebClient _clientDownload;
        #endregion

        #region The constructor of DownloadProgress
        public DownloadProgress(List<DownloadFileInfo> downloadFileListTemp)
        {
            InitializeComponent();

            _downloadFileList = downloadFileListTemp;
            _allFileList = new List<DownloadFileInfo>();
            foreach (DownloadFileInfo file in downloadFileListTemp)
            {
                _allFileList.Add(file);
            }
        }
        #endregion

        #region The method and event
        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_isFinished && DialogResult.No == MessageBox.Show(Constant.Cancelornot, Constant.Messagetitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question))
            {
                e.Cancel = true;
            }
            else
            {
                if (_clientDownload != null)
                    _clientDownload.CancelAsync();

                _evtDownload.Set();
                _evtPerDonwload.Set();
            }
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            buttonOk.Visible = !CommonUnitity.ForceUpdate;
            _evtDownload = new ManualResetEvent(true);
            _evtDownload.Reset();
            ThreadPool.QueueUserWorkItem(ProcDownload);
        }

        long _total;
        long _nDownloadedTotal;

        private void ProcDownload(object o)
        {
            string tempFolderPath = Path.Combine(CommonUnitity.SystemBinUrl, Constant.Tempfoldername);
            if (!Directory.Exists(tempFolderPath))
            {
                Directory.CreateDirectory(tempFolderPath);
            }

            _evtPerDonwload = new ManualResetEvent(false);

            foreach (DownloadFileInfo file in _downloadFileList)
            {
                _total += file.Size;
            }

            CommonUnitity.OnLog(this, new EventArgs<string>("tempFolderPath=" + tempFolderPath));
            try
            {
                while (!_evtDownload.WaitOne(0, false))
                {
                    if (_downloadFileList.Count == 0)
                        break;

                    DownloadFileInfo file = _downloadFileList[0];

                    CommonUnitity.OnLog(this, new EventArgs<string>(String.Format("Start Download: url={0}, fullname={1}", file.DownloadUrl, file.FileFullName)));

                    ShowCurrentDownloadFileName(file.FileName);

                    //Download
                    _clientDownload = new WebClient();

                    //Added the function to support proxy
                    _clientDownload.Proxy = WebProxy.GetDefaultProxy();
                    _clientDownload.Proxy.Credentials = CredentialCache.DefaultCredentials;
                    _clientDownload.Credentials = CredentialCache.DefaultCredentials;
                    //End added

                    _clientDownload.DownloadProgressChanged += (sender, e) =>
                    {
                        try
                        {
                            SetProcessBar(e.ProgressPercentage, (int)((_nDownloadedTotal + e.BytesReceived) * 100 / _total));
                        }
                        catch
                        {
                            CommonUnitity.OnLog(this, new EventArgs<string>("progress changed failed." + e.ProgressPercentage + "/" + e.TotalBytesToReceive));
                            //log the error message,you can use the application's log code
                        }

                    };

                    _clientDownload.DownloadFileCompleted += (sender, e) =>
                    {
                        try
                        {
                            DealWithDownloadErrors();
                            DownloadFileInfo dfile = e.UserState as DownloadFileInfo;
                            _nDownloadedTotal += dfile.Size;
                            SetProcessBar(0, (int)(_nDownloadedTotal * 100 / _total));
                            _evtPerDonwload.Set();

                            CommonUnitity.OnLog(this, new EventArgs<string>("downloadFinished:" + dfile.FileFullName));
                        }
                        catch (Exception ex)
                        {
                            CommonUnitity.OnLog(this, new EventArgs<string>(ex.ToString()));
                            //log the error message,you can use the application's log code
                        }

                    };

                    _evtPerDonwload.Reset();

                    //Download the folder file
                    string tempFolderPath1 = DownloadFileInfo.GetFolderUrl(file);
                    if (!string.IsNullOrEmpty(tempFolderPath1))
                    {
                        tempFolderPath = Path.Combine(CommonUnitity.SystemBinUrl, Constant.Tempfoldername);
                        tempFolderPath += tempFolderPath1;
                    }
                    else
                    {
                        tempFolderPath = Path.Combine(CommonUnitity.SystemBinUrl, Constant.Tempfoldername);
                    }

                    _clientDownload.DownloadFileAsync(new Uri(file.DownloadUrl),
                        Path.Combine(tempFolderPath, file.FileName), file);

                    //Wait for the download complete
                    _evtPerDonwload.WaitOne();

                    _clientDownload.Dispose();
                    _clientDownload = null;

                    //Remove the downloaded files
                    _downloadFileList.Remove(file);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine(ex.Source);
                CommonUnitity.OnLog(this, new EventArgs<string>(ex.StackTrace));
                ShowErrorAndExitApp();
                //throw;
            }

            //When the files have not downloaded,return.
            if (_downloadFileList.Count > 0)
            {
                return;
            }

            //Test network and deal with errors if there have 
            DealWithDownloadErrors();

            //Debug.WriteLine("All Downloaded");
            foreach (DownloadFileInfo file in _allFileList)
            {
                string tempUrlPath = DownloadFileInfo.GetFolderUrl(file);
                string oldPath = string.Empty;
                string newPath = string.Empty;
                try
                {
                    if (!string.IsNullOrEmpty(tempUrlPath))
                    {
                        oldPath = Path.Combine(CommonUnitity.SystemBinUrl + tempUrlPath.Substring(1), file.FileName);
                        newPath = Path.Combine(CommonUnitity.SystemBinUrl + Constant.Tempfoldername + tempUrlPath, file.FileName);
                    }
                    else
                    {
                        oldPath = Path.Combine(CommonUnitity.SystemBinUrl, file.FileName);
                        newPath = Path.Combine(CommonUnitity.SystemBinUrl + Constant.Tempfoldername, file.FileName);
                    }

//                    //just deal with the problem which the files EndsWith xml can not download
//                    FileInfo f = new FileInfo(newPath);
//                    if (!file.Size.ToString().Equals(f.Length.ToString()) && !file.FileName.EndsWith(".xml"))
//                    {
//                        Console.WriteLine("download failed: {0}", f.FullName);
//                        ShowErrorAndExitApp();
//                    }

                    //Added for dealing with the config file download errors
                    string newfilepath = string.Empty;
                    if (newPath.Substring(newPath.LastIndexOf(".") + 1).Equals(Constant.Configfilekey))
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
                catch (Exception exp)
                {
                    CommonUnitity.OnLog(this, new EventArgs<string>(exp.ToString()));
                    //log the error message,you can use the application's log code
                }
            }

            //After dealed with all files, clear the data
            _allFileList.Clear();

            if (_downloadFileList.Count == 0)
                Exit(true);
            else
                Exit(false);

            _evtDownload.Set();
        }

        //To delete or move to old files
        void MoveFolderToOld(string oldPath, string newPath)
        {
            if (File.Exists(oldPath + ".old"))
                File.Delete(oldPath + ".old");

            if (File.Exists(oldPath))
                File.Move(oldPath, oldPath + ".old");

            File.Delete(oldPath);
            File.Move(newPath, oldPath);
            //File.Delete(oldPath + ".old");
        }

        delegate void ShowCurrentDownloadFileNameCallBack(string name);
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
        
        delegate void SetProcessBarCallBack(int current, int total);
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

        delegate void ExitCallBack(bool success);
        private void Exit(bool success)
        {
            if (InvokeRequired)
            {
                ExitCallBack cb = Exit;
                Invoke(cb, success);
            }
            else
            {
                _isFinished = success;
                DialogResult = success ? DialogResult.OK : DialogResult.Cancel;
                Close();
            }
        }

        private void OnCancel(object sender, EventArgs e)
        {
            //bCancel = true;
            //evtDownload.Set();
            //evtPerDonwload.Set();
            ExitCode = ExitCode.SkipUpdate;
            ShowErrorAndExitApp();
        }

        private void DealWithDownloadErrors()
        {
            try
            {
                //Test Network is OK or not.
                Config config = Config.LoadConfig(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constant.Filename));
                WebClient client = new WebClient();
                client.DownloadString(config.ServerUrl);
            }
            catch (Exception ex)
            {
                CommonUnitity.OnLog(this, new EventArgs<string>("New Update url not can't be accessed." + ex));
                //log the error message,you can use the application's log code
                ShowErrorAndExitApp();
            }
        }

        private void ShowErrorAndExitApp()
        {
            MessageBox.Show(Constant.Notnetwork, Constant.Messagetitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
            CommonUnitity.ExitApp(ExitCode.UpdateError);
            ExitCode= ExitCode.UpdateError;
        }

        public ExitCode ExitCode { get; set; }

        #endregion
    }
}