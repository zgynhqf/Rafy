/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140724
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140724 17:24
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.MetaModel
{
    /// <summary>
    /// 规则的元数据。
    /// </summary>
    public class RuleMeta : MetaBase
    {
        private int _Priority;
        /// <summary>
        /// 优先级。
        /// </summary>
        public int Priority
        {
            get { return this._Priority; }
            set { this.SetValue(ref this._Priority, value); }
        }

        private RuleLevel _Level = RuleLevel.Error;
        /// <summary>
        /// 规则的级别。默认值为：Error。
        /// </summary>
        public RuleLevel Level
        {
            get { return this._Level; }
            set { this.SetValue(ref this._Level, value); }
        }

        private EntityStatusScopes _Scope = EntityStatusScopes.Add | EntityStatusScopes.Update;
        /// <summary>
        /// 表示规则的作用范围。默认值为：AddOrUpdate。
        /// </summary>
        public EntityStatusScopes Scope
        {
            get { return this._Scope; }
            set { this.SetValue(ref this._Scope, value); }
        }

        /// <summary>
        /// 判断当前规则是否与指定的范围有重叠。
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        public bool HasScope(EntityStatusScopes scope)
        {
            return (this.Scope & scope) != 0;
        }
    }
}