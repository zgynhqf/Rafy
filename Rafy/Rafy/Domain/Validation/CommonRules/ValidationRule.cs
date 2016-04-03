/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140719
 * 说明：见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140719 11:48
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.ManagedProperty;
using Rafy.MetaModel;

namespace Rafy.Domain.Validation
{
    /// <summary>
    /// 验证规则对象
    /// 
    /// 注意，验证规则的对象对于一个实体来说，是单例的。
    /// </summary>
    public abstract class ValidationRule : IValidationRule
    {
        /// <summary>
        /// 子类重写此属性指示本规则中是否需要连接数据仓库。
        /// </summary>
        protected virtual bool ConnectToDataSource
        {
            get { return false; }
        }

        /// <summary>
        /// 子类重写此方法实现验证规则逻辑。
        /// 当验证出错时，需要设置 e.BrokenDescription。
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="e"></param>
        protected abstract void Validate(Entity entity, RuleArgs e);

        /// <summary>
        /// 如果当前 Rafy 运行时环境中，已经拥有 UI 层界面的元数据，则获取属性对应的的显示名称，并进行翻译后返回。
        /// 否则，直接返回以下格式的字符串，方便替换：[属性名称]。（服务端一般都没有 UI 层元数据。）
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        public static string Display(IManagedProperty property)
        {
            return RuleArgs.Display(property);
        }

        /// <summary>
        /// 如果当前 Rafy 运行时环境中，已经拥有 UI 层界面的元数据，则获取实体对应的的显示名称，并进行翻译后返回。
        /// 否则，直接返回以下格式的字符串，方便替换：[实体类型名称]。（服务端一般都没有 UI 层元数据。）
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        public static string Display(Type entityType)
        {
            return RuleArgs.Display(entityType);
        }

        #region 内部接口

        void IValidationRule.Validate(ManagedPropertyObject entity, RuleArgs e)
        {
            this.Validate(entity as Entity, e);
        }

        bool IValidationRule.ConnectToDataSource
        {
            get { return this.ConnectToDataSource; }
        }

        #endregion
    }
}