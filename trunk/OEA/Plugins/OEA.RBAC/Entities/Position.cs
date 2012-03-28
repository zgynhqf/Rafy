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
using OEA.Library;
using OEA.ManagedProperty;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;
using OEA.ORM;
using SimpleCsla;

namespace OEA.RBAC
{
    [RootEntity, Serializable]
    public partial class Position : Entity
    {
        protected Position() { }

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

        #region  Data Access

        protected override void OnInsert()
        {
            this.CheckUniqueCode();
            base.OnInsert();
        }

        protected override void OnUpdate()
        {
            this.CheckUniqueCode();
            base.OnUpdate();
        }

        private void CheckUniqueCode()
        {
            using (var db = this.CreateDb())
            {
                var count = db.Select(db.Query(typeof(Position))
                    .Constrain(Position.CodeProperty).Equal(this.Code)
                    .And().Constrain(Position.IdProperty).NotEqual(this.Id)
                    ).Count;
                if (count > 0) { throw new FriendlyMessageException("已经有这个编码的岗位。"); }
            }
        }

        #endregion
    }

    [Serializable]
    public class PositionList : EntityList { }

    public class PositionRepository : EntityRepository
    {
        protected PositionRepository() { }
    }

    internal class PositionConfig : EntityConfig<Position>
    {
        protected override void ConfigMeta()
        {
            base.ConfigMeta();

            this.Meta.MapTable().HasColumns(
                Position.CodeProperty,
                Position.NameProperty
                );
        }

        protected override void ConfigView()
        {
            base.ConfigView();

            View.HasTitle(Position.NameProperty).HasLabel("岗位");

            View.Property(Position.CodeProperty).HasLabel("编码").ShowIn(ShowInWhere.All);
            View.Property(Position.NameProperty).HasLabel("名称").ShowIn(ShowInWhere.All);
        }
    }
}