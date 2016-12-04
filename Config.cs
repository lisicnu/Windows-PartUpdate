using System;
using Eden.Share.Helpers;
using Eden.Share.DataStructs;

namespace Eden.Update
{
    public class Config
    {
        #region The private fields

        public Config()
        {
            ServerUrl = string.Empty;
            Enabled = true;
        }

        #endregion

        #region The public property
        public bool Enabled { get; set; }
        public string ServerUrl { get; set; }

        #endregion

        #region The public method

        public static Config LoadConfig(string file)
        {
            try
            {
                return SerializerHelper.LoadFromXml(file, typeof(Config)) as Config;
            }
            catch (Exception ex)
            {
                CommonUnitity.OnLog(null, new EventArgs<string>(ex.ToString()));
                return new Config();
            }
        }

        public void SaveConfig(string file)
        {
            SerializerHelper.SaveAsXml(this, file);
        }
        #endregion
    }

}
