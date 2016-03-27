/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120504
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120504
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy;
using Rafy.Domain;
using Rafy.RBAC.Old.Audit;
using Rafy.Web;
using Rafy.MetaModel.View;

namespace Rafy.RBAC.Old
{
    /// <summary>
    /// 记录系统日志的服务
    /// </summary>
    [Serializable]
    [JsonService]
    [Contract, ContractImpl]
    public class LogService : Service
    {
        public string Title { get; set; }
        public string Type { get; set; }
        public string FriendlyContent { get; set; }
        public string CoderContent { get; set; }
        public string ModuleName { get; set; }
        public int? EntityId { get; set; }

        protected override void Execute()
        {
            if (this.Title == null) throw new ArgumentNullException("this.Title");
            if (this.Type == null) throw new ArgumentNullException("this.Type");

            AuditLogService.LogAsync(new AuditLogItem()
            {
                Title = this.Title,
                Type = (AuditLogType)Enum.Parse(typeof(AuditLogType), this.Type, true),
                FriendlyContent = this.FriendlyContent ?? string.Empty,
                PrivateContent = this.CoderContent ?? string.Empty,
                ModuleName = this.ModuleName ?? string.Empty,
                EntityId = this.EntityId,
            });
        }
    }
}