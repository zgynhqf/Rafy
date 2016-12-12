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
using Rafy;
using Rafy.Domain.ORM;
using System.Linq;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.ManagedProperty;
using Rafy.Domain;
using System.Security.Permissions;
using System.Runtime.Serialization;
using Rafy.Domain.Validation;

namespace Rafy.RBAC.Old
{
    /// <summary>
    /// 部门
    /// </summary>
    [RootEntity, Serializable]
    public partial class Org : IntEntity
    {
        #region 构造函数

        public Org() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected Org(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        public static readonly Property<string> NameProperty = P<Org>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        public static readonly ListProperty<OrgPositionList> OrgPositionListProperty = P<Org>.RegisterList(e => e.OrgPositionList);
        public OrgPositionList OrgPositionList
        {
            get { return this.GetLazyList(OrgPositionListProperty); }
        }

        //由于使用自动编码，所以此块的功能暂时去除。
        //#region  Data Access

        //protected override void OnInsert()
        //{
        //    this.CheckUniqueCode();
        //    base.OnInsert();
        //}

        //protected override void OnUpdate()
        //{
        //    this.CheckUniqueCode();
        //    base.OnUpdate();
        //}

        //private void CheckUniqueCode()
        //{
        //    using (var db = this.CreateDb())
        //    {
        //        var count = db.Select(db.Query(typeof(Org))
        //            .Constrain(Org.TreeCodeProperty).Equal(this.TreeCode)
        //            .And().Constrain(Org.IdProperty).NotEqual(this.Id)
        //            ).Count;
        //        if (count > 0) { throw new FriendlyMessageException("已经有这个编码的组织了。"); }
        //    }
        //}

        //#endregion
    }

    [Serializable]
    public partial class OrgList : EntityList { }

    public partial class OrgRepository : EntityRepository
    {
        protected OrgRepository() { }
    }

    internal class OrgConfig : EntityConfig<Org>
    {
        protected override void AddValidations(IValidationDeclarer rules)
        {
            rules.AddRule(Org.NameProperty, new RequiredRule());
        }

        protected override void ConfigMeta()
        {
            Meta.SupportTree();

            Meta.MapTable().MapProperties(
                Org.NameProperty
                );
        }
    }
}