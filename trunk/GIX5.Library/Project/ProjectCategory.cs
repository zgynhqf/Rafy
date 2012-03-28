using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleCsla;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;

namespace GIX5.Library
{
    [Serializable]
    [RootEntity(Catalog = "指标分析", ModuleType = ModuleType.List)]
    [Label("项目分类")]
    public class ProjectCategory : GEntity
    {
        protected ProjectCategory() { }

        #region 支持树型操作

        public static readonly Property<string> TreeCodeProperty = P<ProjectCategory>.Register(e => e.TreeCode);
        public override string TreeCode
        {
            get { return GetProperty(TreeCodeProperty); }
            set { SetProperty(TreeCodeProperty, value); }
        }

        public static readonly Property<int?> TreePIdProperty = P<ProjectCategory>.Register(e => e.TreePId);
        public override int? TreePId
        {
            get { return this.GetProperty(TreePIdProperty); }
            set { this.SetProperty(TreePIdProperty, value); }
        }

        public override bool SupportTree { get { return true; } }

        #endregion

        public static readonly Property<string> NameProperty = P<ProjectCategory>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }
    }

    [Serializable]
    public class ProjectCategoryList : GEntityList
    {
        protected ProjectCategoryList() { }
    }

    public class ProjectCategoryRepository : EntityRepository
    {
        protected ProjectCategoryRepository() { }
    }

    internal class ProjectCategoryConfig : EntityConfig<ProjectCategory>
    {
        protected override void ConfigMeta()
        {
            base.ConfigMeta();

            Meta.MapTable().HasColumns(
                ProjectCategory.NameProperty
                );

            Meta.Property(ProjectCategory.NameProperty).MapColumn().HasColumnName("Caption");
            Meta.Property(ProjectCategory.TreeCodeProperty).MapColumn().HasColumnName("Code");
            Meta.Property(ProjectCategory.TreePIdProperty).MapColumn().HasColumnName("PId");
        }

        protected override void ConfigView()
        {
            base.ConfigView();

            View.HasTitle(ProjectCategory.NameProperty);

            View.Property(ProjectCategory.TreeCodeProperty).HasLabel("编码").ShowIn(ShowInWhere.All);
            View.Property(ProjectCategory.NameProperty).HasLabel("名称").ShowIn(ShowInWhere.All);
        }
    }
}