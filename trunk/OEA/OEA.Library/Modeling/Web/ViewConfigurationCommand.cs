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
    public class ViewConfigurationCommand : Entity
    {
        public static readonly RefProperty<ViewConfigurationModel> ViewConfigurationModelRefProperty =
            P<ViewConfigurationCommand>.RegisterRef(e => e.ViewConfigurationModel, ReferenceType.Parent);
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

        public static readonly Property<string> NameProperty = P<ViewConfigurationCommand>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        public static readonly Property<string> LabelProperty = P<ViewConfigurationCommand>.Register(e => e.Label);
        public string Label
        {
            get { return this.GetProperty(LabelProperty); }
            set { this.SetProperty(LabelProperty, value); }
        }

        public static readonly Property<bool> IsVisibleProperty = P<ViewConfigurationCommand>.Register(e => e.IsVisible);
        public bool IsVisible
        {
            get { return this.GetProperty(IsVisibleProperty); }
            set { this.SetProperty(IsVisibleProperty, value); }
        }
    }

    [Serializable]
    public class ViewConfigurationCommandList : EntityList
    {
        protected override void QueryByParentId(int parentId)
        {
            var evm = ViewConfigurationModel.GetEVMByParentId(parentId);

            if (OEAEnvironment.IsWeb)
            {
                this.Add(parentId, evm.WebCommands);
            }
            else
            {
                this.Add(parentId, evm.WPFCommands);
            }
        }

        private void Add(int parentId, IEnumerable<WebCommand> webCommands)
        {
            foreach (var jsCmd in webCommands)
            {
                if (jsCmd.Name != WebCommandNames.CustomizeUI)
                {
                    var m = this.AddNew().CastTo<ViewConfigurationCommand>();

                    m.Id = OEAEnvironment.NewLocalId();
                    m.ViewConfigurationModelId = parentId;
                    m.Name = jsCmd.Name;
                    m.Label = jsCmd.Label;
                    m.IsVisible = jsCmd.IsVisible;
                }
            }
        }

        private void Add(int parentId, IEnumerable<WPFCommand> wpfCommands)
        {
            foreach (var cmd in wpfCommands)
            {
                if (cmd.RuntimeType != WPFCommandNames.CustomizeUI)
                {
                    var m = this.AddNew().CastTo<ViewConfigurationCommand>();

                    m.Id = OEAEnvironment.NewLocalId();
                    m.ViewConfigurationModelId = parentId;
                    m.Name = cmd.Name;
                    m.Label = cmd.Label;
                    m.IsVisible = cmd.IsVisible;
                }
            }
        }
    }

    public class ViewConfigurationCommandRepository : EntityRepository
    {
        protected ViewConfigurationCommandRepository()
        {
            this.DataPortalLocation = OEA.DataPortalLocation.Local;
        }
    }

    internal class ViewConfigurationCommandConfig : EntityConfig<ViewConfigurationCommand>
    {
        protected override void ConfigView()
        {
            base.ConfigView();

            View.HasDelegate(ViewConfigurationCommand.LabelProperty);
            View.PageSize = 10000;

            View.Property(ViewConfigurationCommand.NameProperty).HasLabel("命令类型").ShowIn(ShowInWhere.All).Readonly();
            View.Property(ViewConfigurationCommand.LabelProperty).HasLabel("命令名称").ShowIn(ShowInWhere.All);
            View.Property(ViewConfigurationCommand.IsVisibleProperty).HasLabel("是否可见").ShowIn(ShowInWhere.All);

            View.WebCommands.Clear();
            View.WPFCommands.Clear();
            View.UseWebCommands(WebCommandNames.Edit).UseWPFCommands(WPFCommandNames.Edit);
        }
    }
}