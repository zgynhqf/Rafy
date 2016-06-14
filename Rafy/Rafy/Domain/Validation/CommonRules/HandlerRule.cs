/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140719
 * 说明：见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140719 21:31
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.MetaModel;

namespace Rafy.Domain.Validation
{
    /// <summary>
    /// 一个使用指定代理方法的验证器。
    /// </summary>
    public class HandlerRule : ValidationRule
    {
        /// <summary>
        /// 验证逻辑代理方法。
        /// </summary>
        public RuleHandler Handler { get; set; }

        /// <summary>
        /// 验证逻辑是否需要连接数据源。
        /// </summary>
        public bool NeedDataSource { get; set; }

        protected override bool ConnectToDataSource
        {
            get { return this.NeedDataSource; }
        }

        protected override void Validate(Entity entity, RuleArgs e)
        {
            this.Handler(entity, e);
        }

        internal string GetKeyString()
        {
            return string.Format(@"rule://{0}/{1}",
                Handler.Method.DeclaringType.FullName,
                Handler.Method.Name
                );
        }
    }
}
