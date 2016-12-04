
namespace Eden.Update
{
    public class RemoteFile
    {
        /// <summary>
        /// 本地文件相对路径(包含名称). [下载完成之后需要拷贝]
        /// </summary>
        public string LocalPath { get; set; }
        /// <summary>
        /// 下载地址, 绝对路径
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// 下载文件的版本.
        /// </summary>
        public string LastVer { get; set; }
        /// <summary>
        /// 下载文件的大小
        /// </summary>
        public int Size { get; set; }
        /// <summary>
        /// 下载之后是否需要重启
        /// </summary>
        public bool NeedRestart { get; set; }
        /// <summary>
        /// 下载文件的md5值, comput with SHA1
        /// </summary>
        public string Md5 { get; set; }

        public RemoteFile()
        {
            LocalPath = string.Empty;
            Url = string.Empty;
            LastVer = string.Empty;
            Size = 0;
            Md5 = string.Empty;
        }

        public override string ToString()
        {
            return string.Format("Url=[{0}],LocalPath=[{1}],LastVer=[{2}],Size=[{3}],md5=[{4}]",
                Url, LocalPath, LastVer, Size, Md5);
        }
    }
}
