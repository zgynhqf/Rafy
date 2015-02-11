/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110414
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110414
 * 
*******************************************************/

using System;
using System.Linq;
using System.Runtime.Serialization;
using Rafy.Domain;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.Domain.ORM;
using Rafy;
using System.Security.Permissions;
using Rafy.Domain.Validation;

namespace Rafy.RBAC
{
    /// <summary>
    /// 岗位
    /// </summary>
    [RootEntity, Serializable]
    public partial class Position : IntEntity
    {
        #region 构造函数

        public Position() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected Position(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        public static readonly Property<string> CodeProperty = P<Position>.Register(e => e.Code);
        public string Code
        {
            get { return this.GetProperty(CodeProperty); }
            set { this.SetProperty(CodeProperty, value); }
        }

        public static readonly Property<string> NameProperty = P<Position>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }
    }

    [Serializable]
    public partial class PositionList : EntityList { }

    public partial class PositionRepository : EntityRepository
    {
        protected PositionRepository() { }

        public Position GetByCode(string code)
        {
            return this.FetchFirst(new CommonQueryCriteria
            {
                new PropertyMatch(Position.CodeProperty, PropertyOperator.Contains, code)
            });
        }
    }

    internal class PositionConfig : EntityConfig<Position>
    {
        protected override void AddValidations(IValidationDeclarer rules)
        {
            rules.AddRule(Position.CodeProperty, new RequiredRule());
            rules.AddRule(Position.NameProperty, new RequiredRule());
        }

        protected override void ConfigMeta()
        {
            base.ConfigMeta();

            Meta.MapTable().MapProperties(
                Position.CodeProperty,
                Position.NameProperty
                );

            Meta.EnableClientCache(50);

            Meta.SetSaveListServiceType(typeof(SavePositionService));
        }
    }
}