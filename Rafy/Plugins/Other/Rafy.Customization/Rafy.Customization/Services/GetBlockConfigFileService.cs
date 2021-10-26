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
using Rafy.Web;

namespace Rafy.Customization
{
    [Serializable]
    [JsonService(ClientName = "Rafy.GetBlockConfigFileService")]
    [Contract, ContractImpl]
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