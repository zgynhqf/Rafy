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
        #region 构造函数

        public TestUserQueryCriteria() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected TestUserQueryCriteria(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

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
            rules.AddRule(TestUserQueryCriteria.NameProperty, RequiredRule.Instance);

            //测试规则的清空。
            rules.AddRule(TestUserQueryCriteria.CodeProperty, RequiredRule.Instance);
            rules.ClearRules(TestUserQueryCriteria.CodeProperty);
        }
    }
}