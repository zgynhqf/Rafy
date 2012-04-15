using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;
using OEA.MetaModel.XmlConfig;

namespace OEA
{
    [ChildEntity, Serializable]
    public class ViewConfigurationProperty : Entity
    {
        public static readonly RefProperty<ViewConfigurationModel> ViewConfigurationModelRefProperty =
            P<ViewConfigurationProperty>.RegisterRef(e => e.ViewConfigurationModel, ReferenceType.Parent);
        public int ViewConfigurationModelId
        {
            get { return this.GetRefId(ViewConfigurationModelRefProperty); }
            set { this.SetRefId(ViewConfigurationModelRefProperty, value); }
        }
        public ViewConfigurationModel ViewConfigurationModel
        {
            get { return this.GetRefEntity(ViewConfigurationModelRefProperty); }
            set { this.SetRefEntity(ViewConfigurationModelRefProperty, value); }
        }

        public static readonly Property<string> NameProperty = P<ViewConfigurationProperty>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        public static readonly Property<string> LabelProperty = P<ViewConfigurationProperty>.Register(e => e.Label);
        public string Label
        {
            get { return this.GetProperty(LabelProperty); }
            set { this.SetProperty(LabelProperty, value); }
        }

        public static readonly Property<PropertyShowInWhere> ShowInWhereProperty = P<ViewConfigurationProperty>.Register(e => e.ShowInWhere);
        public PropertyShowInWhere ShowInWhere
        {
            get { return this.GetProperty(ShowInWhereProperty); }
            set { this.SetProperty(ShowInWhereProperty, value); }
        }

        public static readonly Property<double> OrderNoProperty = P<ViewConfigurationProperty>.Register(e => e.OrderNo);
        public double OrderNo
        {
            get { return this.GetProperty(OrderNoProperty); }
            set { this.SetProperty(OrderNoProperty, value); }
        }
    }

    [Flags]
    public enum PropertyShowInWhere
    {
        [Label("不显示")]
        Hide = ShowInWhere.Hide,
        [Label("下拉框")]
        DropDownList = ShowInWhere.DropDown,
        [Label("列表")]
        List = ShowInWhere.List,
        [Label("表单")]
        Detail = ShowInWhere.Detail,

        [Label("下拉框,列表")]
        ListAndDronDown = DropDownList | List,
        [Label("下拉框,表单")]
        DetailAndDronDown = DropDownList | Detail,
        [Label("列表,表单")]
        ListDetail = List | Detail,

        [Label("全显示")]
        All = DropDownList | List | Detail
    }

    [Serializable]
    public class ViewConfigurationPropertyList : EntityList
    {
        protected override void OnGetByParentId(int parentId)
        {
            var evm = ViewConfigurationModel.GetEVMByParentId(parentId);

            foreach (var p in evm.OrderedEntityProperties())
            {
                var m = this.AddNew().CastTo<ViewConfigurationProperty>();

                m.Id = OEAEnvironment.NewLocalId();
                m.ViewConfigurationModelId = parentId;
                m.Name = p.Name;
                m.Label = p.Label ?? p.Name;
                m.ShowInWhere = (PropertyShowInWhere)p.ShowInWhere;
                m.OrderNo = p.OrderNo;
            }
        }
    }

    public class ViewConfigurationPropertyRepository : EntityRepository
    {
        protected ViewConfigurationPropertyRepository() { }
    }

    internal class ViewConfigurationPropertyConfig : EntityConfig<ViewConfigurationProperty>
    {
        protected override void ConfigView()
        {
            base.ConfigView();

            View.HasTitle(ViewConfigurationProperty.LabelProperty);
            View.PageSize = 10000;

            View.Property(ViewConfigurationProperty.NameProperty).HasLabel("名称").ShowIn(ShowInWhere.All).Readonly();
            View.Property(ViewConfigurationProperty.LabelProperty).HasLabel("标题").ShowIn(ShowInWhere.All);
            View.Property(ViewConfigurationProperty.ShowInWhereProperty).HasLabel("显示信息").ShowIn(ShowInWhere.All);
            View.Property(ViewConfigurationProperty.OrderNoProperty).HasLabel("排序字段").ShowIn(ShowInWhere.All);

            View.WebCommands.Clear();
            View.WPFCommands.Clear();
            View.UseWebCommands(WebCommandNames.Edit).UseWPFCommands(WPFCommandNames.Edit);
        }
    }
}