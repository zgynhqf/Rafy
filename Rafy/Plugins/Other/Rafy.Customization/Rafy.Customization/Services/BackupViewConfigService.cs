/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：201202
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 201202
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.MetaModel.XmlConfig;
using Rafy.UI;
using Rafy.Web;

namespace Rafy.Customization
{
    [Serializable]
    [JsonService(ClientName = "Rafy.BackupViewConfigService")]
    [Contract, ContractImpl]
    public class BackupViewConfigService : Service
    {
        public BackupViewConfigService()
        {
            this.DataPortalLocation = Rafy.DataPortal.DataPortalLocation.Local;
        }

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

                var dir = Path.GetDirectoryName(file);
                var fileName = Path.GetFileName(file);
                var newFile = Path.Combine(dir, fileName.Replace(".xml", "_Backup.xml "));

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
                    Type = UIEnvironment.BranchProvider.HasBranch ? BlockConfigType.Customization : BlockConfigType.Config
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
}