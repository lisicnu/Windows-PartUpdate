
using System.IO;

namespace Eden.Update
{
    public class DownloadFileInfo
    {
        #region The private fields
        string _downloadUrl;
        string _fileName;
        int _size;
        #endregion

        #region The public property
        public string DownloadUrl { get { return _downloadUrl; } }
        public string FileFullName { get { return _fileName; } }

        public string FileName { get { return Path.GetFileName(FileFullName); } }

        public string LastVer { get; set; }

        public int Size { get { return _size; } }
        #endregion

        #region The constructor of DownloadFileInfo
        public DownloadFileInfo(string url, string fullName, string ver, int size)
        {
            _downloadUrl = url;
            _fileName = fullName;
            LastVer = ver;
            _size = size;
        }

        public override string ToString()
        {
            return string.Format("FullName=[{0}],DownloadUrl=[{1}],LastVer=[{2}],Size=[{3}]", FileFullName, DownloadUrl, LastVer, Size);
        }

        #endregion

        public static string GetFolderUrl(DownloadFileInfo file)
        {
            if (file == null || string.IsNullOrEmpty(file.FileFullName)) return "";

            string folderPathUrl = string.Empty;

            if (file.FileFullName.IndexOf("/") != -1)
            {
                string[] ExeGroup = file.FileFullName.Split('/');
                for (int i = 0; i < ExeGroup.Length - 1; i++)
                {
                    folderPathUrl += "\\" + ExeGroup[i];
                }
                if (!Directory.Exists(CommonUnitity.SystemBinUrl + Constant.Tempfoldername + folderPathUrl))
                {
                    Directory.CreateDirectory(CommonUnitity.SystemBinUrl + Constant.Tempfoldername + folderPathUrl);
                }
            }
            return folderPathUrl;
        }

        //        public static string GetFolderUrl(DownloadFileInfo file)
        //        {
        //            if (file == null || string.IsNullOrEmpty(file.DownloadUrl)) return "";
        //
        //            string folderPathUrl = string.Empty;
        //
        //            int folderPathPoint = file.DownloadUrl.IndexOf("/", 15) + 1;
        //
        //            string filepathstring = file.DownloadUrl.Substring(folderPathPoint);
        //
        //            int folderPathPoint1 = filepathstring.IndexOf("/");
        //
        //            string filepathstring1 = filepathstring.Substring(folderPathPoint1 + 1);
        //
        //            if (filepathstring1.IndexOf("/") != -1)
        //            {
        //                string[] ExeGroup = filepathstring1.Split('/');
        //                for (int i = 0; i < ExeGroup.Length - 1; i++)
        //                {
        //                    folderPathUrl += "\\" + ExeGroup[i];
        //                }
        //                if (!Directory.Exists(CommonUnitity.SystemBinUrl + Constant.Tempfoldername + folderPathUrl))
        //                {
        //                    Directory.CreateDirectory(CommonUnitity.SystemBinUrl + Constant.Tempfoldername + folderPathUrl);
        //                }
        //            }
        //            return folderPathUrl;
        //        }
    }
}
