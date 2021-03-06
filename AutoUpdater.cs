using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using Eden.Share.Extensions;
using Eden.Share.Helpers;
using Eden.Share.DataStructs;

namespace Eden.Update
{
    #region The delegate
    public delegate void ShowHandler();
    #endregion

    public class AutoUpdater : IAutoUpdater
    {
        #region The private fields
        private Config _config = null;
        private RemoteConfig _remoteConfig = null;
        private bool _bNeedRestart = false;
        private bool _bDownload = false;
        List<DownloadFileInfo> _downloadFileListTemp = null;

        #endregion

        #region The public event
        public event ShowHandler OnShow;
        #endregion

        #region The constructor of AutoUpdater
        public AutoUpdater()
        {
            _config = Config.LoadConfig(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constant.Filename));
        }
        #endregion

        #region The public method

        public string RequestUrl(string url)
        {
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(new Uri(url));
            try
            {
                request.ReadWriteTimeout = 30 * 1000;

                using (WebResponse response = request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        return reader.ReadToEnd();
                    } 
                }
               
            }
            catch (Exception e)
            {
                CommonUnitity.OnLog(this, new EventArgs<string>(e.ToString()));
            }

            return "";
        }

        public ExitCode Update()
        {
            if (!_config.Enabled)
            {
                CommonUnitity.OnLog(this, new EventArgs<string>("local config file not allowed update."));
                return ExitCode.Default;
            }

            try
            {
                _remoteConfig =
                    SerializerHelper.Deserialize(typeof(RemoteConfig), RequestUrl(_config.ServerUrl)) as RemoteConfig;
            }
            catch (Exception ex)
            {
                CommonUnitity.OnLog(this, new EventArgs<string>(ex.Message));
                return ExitCode.Default;
            }
            if (_remoteConfig == null)
            {
                CommonUnitity.OnLog(this, new EventArgs<string>("load remote config file failed."));
                return ExitCode.Default;
            }

            if (!_remoteConfig.Enable)
            {
                CommonUnitity.OnLog(this, new EventArgs<string>("remote server not allowed update."));
                return ExitCode.Default;
            }

            if (!CommonUnitity.ForceUpdate)
            {
                CommonUnitity.ForceUpdate = _remoteConfig.ForceUpdate;
            }

            List<DownloadFileInfo> downloadList = new List<DownloadFileInfo>();

            string dir = AppDomain.CurrentDomain.BaseDirectory;
            foreach (RemoteFile remoteFile in _remoteConfig.UpdateFileList)
            {
                string localFile = Path.Combine(dir, remoteFile.LocalPath);
                if (remoteFile.Md5.Equals(HashHelper.ComputeSHA1(localFile), StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }

                CommonUnitity.OnLog(this, new EventArgs<string>("FileToBeDownload:" + remoteFile));
                downloadList.Add(new DownloadFileInfo(remoteFile.Url, remoteFile.LocalPath, remoteFile.LastVer, remoteFile.Size));

                if (remoteFile.NeedRestart)
                    _bNeedRestart = true;
            }

            _downloadFileListTemp = downloadList;

            ExitCode exitCode = 0;
            if (_downloadFileListTemp != null && !_downloadFileListTemp.IsEmpty())
            {
                if (CommonUnitity.ForceUpdate)
                {
                    exitCode = StartDownload(downloadList);
                }
                else
                {
                    DownloadConfirm dc = new DownloadConfirm(downloadList);
                    if (this.OnShow != null)
                        this.OnShow();

                    if (DialogResult.OK == dc.ShowDialog())
                    {
                        exitCode = StartDownload(downloadList);
                    }
                    else
                    {
                        exitCode = ExitCode.SkipUpdate;
                        CommonUnitity.OnLog(this, new EventArgs<string>("Update skipped."));
                        Application.Exit();
                    }
                }
            }
            return exitCode;
        }

        private string LoadVersion(string file)
        {
            FileVersionInfo fileVersion = FileVersionInfo.GetVersionInfo(file);
            return fileVersion.FileVersion;
        }

        public void RollBack()
        {
            foreach (DownloadFileInfo file in _downloadFileListTemp)
            {
                string tempUrlPath = DownloadFileInfo.GetFolderUrl(file);
                string oldPath = string.Empty;
                try
                {
                    if (!string.IsNullOrEmpty(tempUrlPath))
                    {
                        oldPath = Path.Combine(CommonUnitity.SystemBinUrl + tempUrlPath.Substring(1), file.FileName);
                    }
                    else
                    {
                        oldPath = Path.Combine(CommonUnitity.SystemBinUrl, file.FileName);
                    }

                    if (oldPath.EndsWith("_"))
                        oldPath = oldPath.Substring(0, oldPath.Length - 1);

                    MoveFolderToOld(oldPath + ".old", oldPath);

                }
                catch (Exception ex)
                {
                    //log the error message,you can use the application's log code
                }
            }
        }

        #endregion

        #region The private method
        string _newfilepath = string.Empty;
        private void MoveFolderToOld(string oldPath, string newPath)
        {
            if (File.Exists(oldPath) && File.Exists(newPath))
            {
                File.Copy(oldPath, newPath, true);
            }
        }

        private ExitCode StartDownload(List<DownloadFileInfo> downloadList)
        {
            DownloadProgress dp = new DownloadProgress(downloadList);
            if (dp.ShowDialog() == DialogResult.OK)
            {
                //
                if (DialogResult.Cancel == dp.ShowDialog())
                {
                    return dp.ExitCode;
                }
                //Update successfully
                _config.SaveConfig(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constant.Filename));

//                if (_bNeedRestart)
                {
                    //Delete the temp folder
                    Directory.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constant.Tempfoldername), true);

                    MessageBox.Show(Constant.Applytheupdate, Constant.Messagetitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    CommonUnitity.ExitApp(ExitCode.SuccessUpdate);
                }
                return ExitCode.SuccessUpdate;
            }
            return dp.ExitCode;
        }
        #endregion

    }
}
