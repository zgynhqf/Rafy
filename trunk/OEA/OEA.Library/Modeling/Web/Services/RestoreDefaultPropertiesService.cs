using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.MetaModel;
using System.IO;
using OEA.MetaModel.View;
using OEA.Web;
using OEA.MetaModel.XmlConfig;
using System.Diagnostics;

namespace OEA.Library.Modeling.Web
{
    [Serializable]
    [ClientServiceName("OEA.BackupViewConfigService")]
    public class BackupViewConfigService : Service
    {
        [ServiceInput]
        public string Model { get; set; }
        [ServiceInput]
        public string ViewName { get; set; }

        [ServiceOutput]
        public bool Success { get; set; }

        protected override void Execute()
        {
            var key = this.GetInputKey();
            if (key != null)
            {
                var file = key.GetFilePath();

                var fileName = Path.GetFileName(file);
                var newFile = Path.Combine(Path.GetTempPath(), fileName.Replace(".xml", "_Backup.xml "));

                this.Success = RenameFile(file, newFile);
            }
        }

        protected BlockConfigKey GetInputKey()
        {
            if (!string.IsNullOrEmpty(this.Model))
            {
                var key = new BlockConfigKey
                {
                    EntityType = ClientEntities.Find(this.Model).EntityType,
                    ExtendView = this.ViewName,
                    Type = OEAEnvironment.CustomerProvider.IsCustomizing ? BlockConfigType.Customization : BlockConfigType.Config
                };

                var dv = ViewConfigurationModel.ViewNameProperty.GetMeta(typeof(ViewConfigurationModel)).DefaultValue;
                if (key.ExtendView == dv) { key.ExtendView = null; }

                return key;
            }

            return null;
        }

        protected virtual bool RenameFile(string file, string newFile)
        {
            if (File.Exists(file))
            {
                File.Copy(file, newFile, true);
                File.Delete(file);

                return true;
            }

            return false;
        }
    }

    [Serializable]
    [ClientServiceName("OEA.RestoreViewConfigService")]
    public class RestoreViewConfigService : BackupViewConfigService
    {
        protected override bool RenameFile(string file, string newFile)
        {
            if (File.Exists(newFile))
            {
                File.Copy(newFile, file, true);
                File.Delete(newFile);

                return true;
            }

            return false;
        }
    }

    [Serializable]
    [ClientServiceName("OEA.GetBlockConfigFileService")]
    public class GetBlockConfigFileService : BackupViewConfigService
    {
        [ServiceOutput]
        public bool Opened { get; set; }

        protected override void Execute()
        {
            var key = this.GetInputKey();
            if (key != null)
            {
                var file = key.GetFilePath();
                if (File.Exists(file))
                {
                    Process.Start(file);
                    Opened = true;
                }
            }
        }
    }
}