/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140313
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140313 11:00
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.Domain.Validation;

namespace UT
{
    [QueryEntity, Serializable]
    public class TestUserQueryCriteria : Criteria
    {
        public static readonly Property<string> CodeProperty = P<TestUserQueryCriteria>.Register(e => e.Code);
        public string Code
        {
            get { return this.GetProperty(CodeProperty); }
            set { this.SetProperty(CodeProperty, value); }
        }

        public static readonly Property<string> NameProperty = P<TestUserQueryCriteria>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }
    }

    internal class TestUserQueryCriteriaConfig : EntityConfig<TestUserQueryCriteria>
    {
        protected override void AddValidations(IValidationDeclarer rules)
        {
            rules.AddRule(TestUserQueryCriteria.NameProperty, new RequiredRule());

            //测试规则的清空。
            rules.AddRule(TestUserQueryCriteria.CodeProperty, new RequiredRule());
            rules.ClearRules(TestUserQueryCriteria.CodeProperty);
        }
    }
}