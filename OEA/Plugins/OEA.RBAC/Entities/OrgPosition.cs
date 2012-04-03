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
using SimpleCsla;
using OEA.ORM;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.Utils;
using OEA.ManagedProperty;
using OEA.MetaModel.View;
using OEA.Web;
using OEA.Library;

namespace OEA.RBAC
{
    [ChildEntity, Serializable]
    public partial class OrgPosition : Entity
    {
        public static readonly RefProperty<Org> OrgRefProperty =
            P<OrgPosition>.RegisterRef(e => e.Org, ReferenceType.Parent);
        public int OrgId
        {
            get { return this.GetRefId(OrgRefProperty); }
            set { this.SetRefId(OrgRefProperty, value); }
        }
        public Org Org
        {
            get { return this.GetRefEntity(OrgRefProperty); }
            set { this.SetRefEntity(OrgRefProperty, value); }
        }

        public static readonly RefProperty<Position> PositionRefProperty =
            P<OrgPosition>.RegisterRef(e => e.Position, ReferenceType.Normal);
        public int PositionId
        {
            get { return this.GetRefId(PositionRefProperty); }
            set { this.SetRefId(PositionRefProperty, value); }
        }
        public Position Position
        {
            get { return this.GetRefEntity(PositionRefProperty); }
            set
            {
                this.SetRefEntity(PositionRefProperty, value);
                this.OnPropertyChanged("Position");
                this.OnPropertyChanged("View_Code");
                this.OnPropertyChanged("View_Name");
                this.OnPropertyChanged("View_UseTime");
                this.OnPropertyChanged("View_CycleType");
                this.OnPropertyChanged("View_LoginTotalCount");
            }
        }

        #region 视图属性

        public static readonly Property<string> View_CodeProperty = P<OrgPosition>.RegisterReadOnly(e => e.View_Code, e => (e as OrgPosition).GetView_Code(), null);
        public string View_Code
        {
            get { return this.GetProperty(View_CodeProperty); }
        }
        private string GetView_Code()
        {
            return this.Position.Code;
        }

        public static readonly Property<string> View_NameProperty = P<OrgPosition>.RegisterReadOnly(e => e.View_Name, e => (e as OrgPosition).GetView_Name(), null);
        public string View_Name
        {
            get { return this.GetProperty(View_NameProperty); }
        }
        private string GetView_Name()
        {
            return this.Position.Name;
        }

        #endregion

        public static readonly Property<OrgPositionUserList> OrgPositionUserListProperty = P<OrgPosition>.Register(e => e.OrgPositionUserList);
        [Association]
        public OrgPositionUserList OrgPositionUserList
        {
            get { return this.GetLazyChildren(OrgPositionUserListProperty); }
        }

        /// <summary>
        /// 注意，这个列表中存储的是“不可用”的命令列表。
        /// </summary>
        public static readonly Property<OrgPositionOperationDenyList> OrgPositionOperationDenyListProperty = P<OrgPosition>.Register(e => e.OrgPositionOperationDenyList);
        [Association]
        public OrgPositionOperationDenyList OrgPositionOperationDenyList
        {
            get { return this.GetLazyChildren(OrgPositionOperationDenyListProperty); }
        }
    }

    [Serializable]
    public class OrgPositionList : EntityList
    {
        protected void QueryBy(int userId)
        {
            using (var db = this.CreateDb())
            {
                var qUser = db.Query(typeof(OrgPositionUser))
                    .Constrain(OrgPositionUser.UserRefProperty).Equal(userId);
                var positionIds = db.Select<OrgPositionUser>(qUser)
                    .Select(e => e.OrgPositionId).ToList();

                this.QueryDb(q => q.Constrain(OrgPosition.IdProperty).In(positionIds));
            }
        }
    }

    public class OrgPositionRepository : EntityRepository
    {
        protected OrgPositionRepository() { }

        public OrgPositionList GetList(int userId)
        {
            return this.FetchListCast<OrgPositionList>(userId);
        }
    }

    internal class OrgPositionConfig : EntityConfig<OrgPosition>
    {
        protected override void ConfigMeta()
        {
            base.ConfigMeta();

            Meta.MapTable().HasColumns(
                OrgPosition.PositionRefProperty,
                OrgPosition.OrgRefProperty
                );
        }

        protected override void ConfigView()
        {
            base.ConfigView();

            View.HasLabel("岗位").NotAllowEdit();

            if (OEAEnvironment.IsWeb)
            {
                View.RemoveWebCommands(WebCommandNames.Add);

                View.UseWebCommand("LookupSelectAddOrgPosition")
                    .HasLabel("选择岗位")
                    .SetCustomParams("targetClass", ClientEntities.GetClientName(typeof(Position)));
            }
            else
            {
                View.UseWPFCommands("RBAC.ChoosePositionCommand")
                    .RemoveWPFCommands(WPFCommandNames.Add, WPFCommandNames.Edit);
            }



            View.Property(OrgPosition.PositionRefProperty).HasLabel("岗位");
            View.Property(OrgPosition.View_CodeProperty).ShowIn(ShowInWhere.List).HasLabel("编码");
            View.Property(OrgPosition.View_NameProperty).ShowIn(ShowInWhere.List).HasLabel("名称");
        }
    }
}