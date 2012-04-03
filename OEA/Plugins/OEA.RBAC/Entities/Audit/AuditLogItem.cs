using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.MetaModel.Attributes;

namespace OEA.MetaModel.Audit
{
    /// <summary>
    /// 日志项
    /// </summary>
    [Serializable]
    public class AuditLogItem
    {
        private string _title;

        private string _privateContent;

        private string _friendlyContent;

        private string _user;

        private string _moduleName;

        private string _machineName;

        private AuditLogType _type;

        private DateTime _logTime;

        private int? _entityId;

        public AuditLogItem()
        {
            this._user = "匿名用户";
            var currentUser = OEA.ApplicationContext.User;
            if (currentUser != null && currentUser.Identity.IsAuthenticated)
            {
                this._user = currentUser.Identity.Name;
            }
            this._title = string.Empty;
            this._privateContent = string.Empty;
            this._moduleName = string.Empty;
            this._machineName = Environment.MachineName;
            this._type = AuditLogType.None;
            this._logTime = DateTime.Now;
        }

        /// <summary>
        /// 日志标题
        /// </summary>
        public string Title
        {
            get { return this._title; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                this._title = value;
            }
        }

        /// <summary>
        /// 显示给用户的内容
        /// </summary>
        public string FriendlyContent
        {
            get { return this._friendlyContent; }
            set { this._friendlyContent = value; }
        }

        /// <summary>
        /// 日志内容
        /// </summary>
        public string PrivateContent
        {
            get { return this._privateContent; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                this._privateContent = value;
            }
        }

        /// <summary>
        /// 触发者
        /// </summary>
        public string User
        {
            get { return this._user; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                this._user = value;
            }
        }

        public string MachineName
        {
            get { return this._machineName; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                this._machineName = value;
            }
        }

        /// <summary>
        /// 模块名
        /// </summary>
        public string ModuleName
        {
            get { return this._moduleName; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                this._moduleName = value;
            }
        }

        /// <summary>
        /// 日志类型
        /// </summary>
        public AuditLogType Type
        {
            get { return this._type; }
            set { this._type = value; }
        }

        /// <summary>
        /// 记录日志的时间
        /// </summary>
        public DateTime LogTime
        {
            get { return this._logTime; }
            set { this._logTime = value; }
        }

        /// <summary>
        /// 当前正在操作的对象
        /// </summary>
        public int? EntityId
        {
            get { return this._entityId; }
            set { this._entityId = value; }
        }

        public override string ToString()
        {
            var result = new StringBuilder();

            result.AppendLine(this._title);
            result.AppendLine(this._friendlyContent);
            result.AppendLine(this._logTime.ToString());
            result.AppendLine(this._machineName);
            result.AppendLine(this._moduleName);
            result.AppendLine(this._type.ToString());
            result.AppendLine(this._user);
            result.AppendLine("*********************************");
            result.AppendLine();

            return result.ToString();
        }
    }

    /// <summary>
    /// 日志类型
    /// 
    /// 这个类型，只能加新项，而不要删除旧项。
    /// </summary>
    public enum AuditLogType
    {
        [Label("未指定类型")]
        None = 0,
        [Label("执行命令")]
        Command = 1,
        [Label("打开模块")]
        OpenModule = 2,
        [Label("登录")]
        Login = 3,
        [Label("下拉选值")]
        DropDownValue = 4
    }
}
