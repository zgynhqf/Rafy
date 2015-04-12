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
using Rafy;
using Rafy.Domain.ORM;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.Utils;
using Rafy.ManagedProperty;
using Rafy.MetaModel.View;
using Rafy.Web;
using Rafy.Domain;
using System.Security.Permissions;
using System.Runtime.Serialization;
using Rafy.Data;

namespace Rafy.RBAC
{
    /// <summary>
    /// 部门岗位
    /// </summary>
    [ChildEntity, Serializable]
    public partial class OrgPosition : IntEntity
    {
        #region 构造函数

        public OrgPosition() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected OrgPosition(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        public static readonly IRefIdProperty OrgIdProperty =
            P<OrgPosition>.RegisterRefId(e => e.OrgId, ReferenceType.Parent);
        public int OrgId
        {
            get { return (int)this.GetRefId(OrgIdProperty); }
            set { this.SetRefId(OrgIdProperty, value); }
        }
        public static readonly RefEntityProperty<Org> OrgProperty =
            P<OrgPosition>.RegisterRef(e => e.Org, OrgIdProperty);
        public Org Org
        {
            get { return this.GetRefEntity(OrgProperty); }
            set { this.SetRefEntity(OrgProperty, value); }
        }

        public static readonly IRefIdProperty PositionIdProperty =
            P<OrgPosition>.RegisterRefId(e => e.PositionId, ReferenceType.Normal);
        public int PositionId
        {
            get { return (int)this.GetRefId(PositionIdProperty); }
            set { this.SetRefId(PositionIdProperty, value); }
        }
        public static readonly RefEntityProperty<Position> PositionProperty =
            P<OrgPosition>.RegisterRef(e => e.Position, PositionIdProperty);
        public Position Position
        {
            get { return this.GetRefEntity(PositionProperty); }
            set { this.SetRefEntity(PositionProperty, value); }
        }

        #region 视图属性

        public static readonly Property<string> View_CodeProperty = P<OrgPosition>.RegisterReadOnly(
            e => e.View_Code, e => (e as OrgPosition).GetView_Code(), PositionProperty);
        public string View_Code
        {
            get { return this.GetProperty(View_CodeProperty); }
        }
        private string GetView_Code()
        {
            return this.Position.Code;
        }

        public static readonly Property<string> View_NameProperty = P<OrgPosition>.RegisterReadOnly(
            e => e.View_Name, e => (e as OrgPosition).GetView_Name(), PositionProperty);
        public string View_Name
        {
            get { return this.GetProperty(View_NameProperty); }
        }
        private string GetView_Name()
        {
            return this.Position.Name;
        }

        #endregion

        public static readonly ListProperty<OrgPositionUserList> OrgPositionUserListProperty = P<OrgPosition>.RegisterList(e => e.OrgPositionUserList);
        public OrgPositionUserList OrgPositionUserList
        {
            get { return this.GetLazyList(OrgPositionUserListProperty); }
        }

        /// <summary>
        /// 注意，这个列表中存储的是“不可用”的命令列表。
        /// </summary>
        public static readonly ListProperty<OrgPositionOperationDenyList> OrgPositionOperationDenyListProperty = P<OrgPosition>.RegisterList(e => e.OrgPositionOperationDenyList);
        public OrgPositionOperationDenyList OrgPositionOperationDenyList
        {
            get { return this.GetLazyList(OrgPositionOperationDenyListProperty); }
        }
    }

    [Serializable]
    public partial class OrgPositionList : EntityList
    {
    }

    public partial class OrgPositionRepository : EntityRepository
    {
        protected OrgPositionRepository() { }

        public OrgPositionList GetList(int userId)
        {
            return this.FetchList(userId);
        }

        protected EntityList FetchBy(int userId)
        {
            FormattedSql sql = @"
SELECT *
FROM OrgPosition
left outer join OrgPositionUser on OrgPosition.Id = OrgPositionUser.OrgPositionId
where OrgPositionUser.UserId = {0}
";
            sql.Parameters.Add(userId);

            return (this.DataQueryer as RdbDataQueryer).QueryList(sql);
        }
    }

    internal class OrgPositionConfig : EntityConfig<OrgPosition>
    {
        protected override void ConfigMeta()
        {
            base.ConfigMeta();

            Meta.MapTable().MapProperties(
                OrgPosition.PositionIdProperty,
                OrgPosition.OrgIdProperty
                );
        }
    }
}