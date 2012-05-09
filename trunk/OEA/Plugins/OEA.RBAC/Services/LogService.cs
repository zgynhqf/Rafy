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
using hxy.Common;
using OEA.Library;
using OEA.MetaModel.Audit;
using OEA.Web;

namespace OEA.RBAC
{
    /// <summary>
    /// 记录系统日志的服务
    /// </summary>
    public class LogService : Service
    {
        [ServiceInput]
        public string Title { get; set; }
        [ServiceInput]
        public string Type { get; set; }
        [ServiceInput]
        public string FriendlyContent { get; set; }
        [ServiceInput]
        public string CoderContent { get; set; }
        [ServiceInput]
        public string ModuleName { get; set; }
        [ServiceInput]
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
