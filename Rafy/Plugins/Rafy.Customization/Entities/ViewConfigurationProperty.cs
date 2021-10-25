using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.Domain;
using Rafy.Domain.ORM;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.MetaModel.XmlConfig;

namespace Rafy.Customization
{
    [ChildEntity, Serializable]
    public partial class ViewConfigurationProperty : IntEntity
    {
        public static readonly IRefIdProperty ViewConfigurationModelIdProperty =
            P<ViewConfigurationProperty>.RegisterRefId(e => e.ViewConfigurationModelId, ReferenceType.Parent);
        public int ViewConfigurationModelId
        {
            get { return (int)this.GetRefId(ViewConfigurationModelIdProperty); }
            set { this.SetRefId(ViewConfigurationModelIdProperty, value); }
        }
        public static readonly RefEntityProperty<ViewConfigurationModel> ViewConfigurationModelProperty =
            P<ViewConfigurationProperty>.RegisterRef(e => e.ViewConfigurationModel, ViewConfigurationModelIdProperty);
        public ViewConfigurationModel ViewConfigurationModel
        {
            get { return this.GetRefEntity(ViewConfigurationModelProperty); }
            set { this.SetRefEntity(ViewConfigurationModelProperty, value); }
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
    public partial class ViewConfigurationPropertyList : EntityList { }

    public partial class ViewConfigurationPropertyRepository : EntityRepository
    {
        protected ViewConfigurationPropertyRepository()
        {
            this.DataPortalLocation = DataPortalLocation.Local;
        }
    }

    [DataProviderFor(typeof(ViewConfigurationPropertyRepository))]
    public partial class ViewConfigurationPropertyDataProvider : RdbDataProvider
    {
        [RepositoryQuery]
        public override object GetByParentId(object pId, PagingInfo paging = null, LoadOptions loadOptions = null)
        {
            int parentId = (int)pId;

            var list = new ViewConfigurationPropertyList();

            var evm = ViewConfigurationModel.GetEVMByParentId(parentId);

            foreach (var p in evm.OrderedEntityProperties())
            {
                var m = new ViewConfigurationProperty();

                m.Id = RafyEnvironment.NewLocalId();
                m.ViewConfigurationModelId = parentId;
                m.Name = p.Name;
                m.Label = p.Label ?? p.Name;
                m.ShowInWhere = (PropertyShowInWhere)p.ShowInWhere;
                m.OrderNo = p.OrderNo;

                list.Add(m);
            }

            return list;
        }
    }
}