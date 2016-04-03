/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140719
 * 说明：见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140719 13:22
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
    /// 约束某属性不能为空值（null）。
    /// 字符串还应该不能为空字符串。
    /// </summary>
    public class RequiredRule : ValidationRule
    {
        /// <summary>
        /// 设置此属性可以自定义要显示的错误信息。
        /// </summary>
        public Func<Entity, string> MessageBuilder { get; set; }

        protected override void Validate(Entity entity, RuleArgs e)
        {
            var property = e.Property;

            bool isNull = false;

            if (property is IRefProperty)
            {
                var id = entity.GetRefNullableId((property as IRefProperty).RefIdProperty);
                isNull = id == null;
            }
            else
            {
                var value = entity.GetProperty(property);
                if (property.PropertyType == typeof(string))
                {
                    isNull = string.IsNullOrEmpty(value as string);
                }
                else
                {
                    isNull = value == null;
                }
            }

            if (isNull)
            {
                if (this.MessageBuilder != null)
                {
                    e.BrokenDescription = this.MessageBuilder(entity);
                }
                else
                {
                    e.BrokenDescription = string.Format("{0} 里没有输入值。".Translate(), e.DisplayProperty());
                }
            }
        }
    }
}