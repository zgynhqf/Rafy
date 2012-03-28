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
    [Label("项目")]
    public class Project : GEntity
    {
        protected Project() { }

        public static readonly Property<string> FileNameProperty = P<Project>.Register(e => e.FileName);
        public string FileName
        {
            get { return this.GetProperty(FileNameProperty); }
            set { this.SetProperty(FileNameProperty, value); }
        }

        public static readonly Property<string> ProjectNameProperty = P<Project>.Register(e => e.ProjectName);
        public string ProjectName
        {
            get { return this.GetProperty(ProjectNameProperty); }
            set { this.SetProperty(ProjectNameProperty, value); }
        }

        public static readonly RefProperty<ProjectCategory> ProjectCategoryRefProperty =
            P<Project>.RegisterRef(e => e.ProjectCategory, ReferenceType.Normal);
        public int? ProjectCategoryId
        {
            get { return this.GetRefNullableId(ProjectCategoryRefProperty); }
            set { this.SetRefNullableId(ProjectCategoryRefProperty, value); }
        }
        public ProjectCategory ProjectCategory
        {
            get { return this.GetRefEntity(ProjectCategoryRefProperty); }
            set { this.SetRefEntity(ProjectCategoryRefProperty, value); }
        }

        public static readonly Property<int> MaskProperty = P<Project>.Register(e => e.Mask);
        public int Mask
        {
            get { return this.GetProperty(MaskProperty); }
            set { this.SetProperty(MaskProperty, value); }
        }

        public static readonly Property<DateTime> SaveTimeProperty = P<Project>.Register(e => e.SaveTime);
        public DateTime SaveTime
        {
            get { return this.GetProperty(SaveTimeProperty); }
            set { this.SetProperty(SaveTimeProperty, value); }
        }

        public static readonly RefProperty<User> OpenerRefProperty =
            P<Project>.RegisterRef(e => e.Opener, ReferenceType.Normal);
        public int? OpenerId
        {
            get { return this.GetRefNullableId(OpenerRefProperty); }
            set { this.SetRefNullableId(OpenerRefProperty, value); }
        }
        public User Opener
        {
            get { return this.GetRefEntity(OpenerRefProperty); }
            set { this.SetRefEntity(OpenerRefProperty, value); }
        }
    }

    //public enum MaskType
    //{
    //    [Label("标记一")]
    //    Mask1 = 0,
    //    [Label("标记二")]
    //    Mask2 = 1,
    //    [Label("标记三")]
    //    Mask3 = 2
    //}

    [Serializable]
    public class ProjectList : GEntityList
    {
        protected ProjectList() { }
    }

    public class ProjectRepository : EntityRepository
    {
        protected ProjectRepository() { }
    }

    internal class ProjectConfig : EntityConfig<Project>
    {
        protected override void ConfigMeta()
        {
            base.ConfigMeta();

            Meta.MapTable().HasColumns(
                Project.FileNameProperty,
                Project.ProjectNameProperty,
                Project.ProjectCategoryRefProperty,
                Project.MaskProperty,
                Project.SaveTimeProperty,
                Project.OpenerRefProperty
                );
        }

        protected override void ConfigView()
        {
            base.ConfigView();

            View.HasTitle(Project.ProjectNameProperty)
                //.HasLockProperty(Project.OpenerRefProperty)
                .GroupBy(Project.ProjectCategoryRefProperty);

            View.Property(Project.FileNameProperty).HasLabel("文件名称").ShowIn(ShowInWhere.All);
            View.Property(Project.ProjectNameProperty).HasLabel("项目名称").ShowIn(ShowInWhere.All);
            View.Property(Project.ProjectCategoryRefProperty).HasLabel("项目分类").ShowIn(ShowInWhere.All);
            View.Property(Project.MaskProperty).HasLabel("标记").ShowIn(ShowInWhere.All).Readonly(true);
            View.Property(Project.SaveTimeProperty).HasLabel("保存时间").ShowIn(ShowInWhere.All);
            View.Property(Project.OpenerRefProperty).HasLabel("打开者").ShowIn(ShowInWhere.All).Readonly(true);
        }
    }
}