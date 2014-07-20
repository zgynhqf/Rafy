/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120607 14:14
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120607 14:14
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using Rafy.Domain;
using Rafy.Domain.Validation;
using Rafy.ManagedProperty;

namespace Rafy.WPF
{
    /// <summary>
    /// 托管属性的验证规则
    /// 
    /// 使用本规则，需要在 Binding.Path 中添加 ManagedProperty 作为参数。
    /// </summary>
    public class ManagedProeprtyValidationRule : System.Windows.Controls.ValidationRule
    {
        public static readonly ManagedProeprtyValidationRule Instance = new ManagedProeprtyValidationRule();

        private ManagedProeprtyValidationRule()
            : base(ValidationStep.UpdatedValue, true)
        {
            this.ValidatorActions = ValidatorActions.IgnoreDataSourceValidations;
        }

        /// <summary>
        /// 默认值为 ValidatorActions.IgnoreDataSourceValidations。
        /// </summary>
        public ValidatorActions ValidatorActions { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var bindingExp = value as BindingExpression;
            if (bindingExp != null)
            {
                var entity = bindingExp.DataItem as Entity;
                if (entity != null)
                {
                    var mp = bindingExp.ParentBinding.Path.PathParameters
                        .FirstOrDefault(p => p is IManagedProperty) as IManagedProperty;
                    if (mp != null)
                    {
                        var broken = entity.Validate(mp, this.ValidatorActions);
                        if (broken.Count > 0)
                        {
                            return new ValidationResult(false, broken.ToString());
                        }
                    }
                }
            }

            return ValidationResult.ValidResult;
        }
    }
}