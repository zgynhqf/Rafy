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
using OEA;
using OEA.ORM;
using System.Linq;
using OEA.MetaModel.Attributes;
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.ManagedProperty;
using OEA.Library;

namespace OEA.RBAC
{
    [RootEntity, Serializable]
    public partial class Org : Entity
    {
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
    public class OrgList : EntityList { }

    public class OrgRepository : EntityRepository
    {
        protected OrgRepository() { }
    }

    internal class OrgConfig : EntityConfig<Org>
    {
        protected override void ConfigMeta()
        {
            Meta.SupportTree();

            Meta.MapTable().HasColumns(
                Org.NameProperty
                );
        }

        protected override void ConfigView()
        {
            base.ConfigView();

            View.HasLabel("部门").HasTitle(Org.NameProperty);

            View.Property(Org.NameProperty).HasLabel("名称").ShowIn(ShowInWhere.All);
        }
    }
}