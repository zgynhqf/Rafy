using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.MetaModel.View;
using OEA.Module.WPF;
using OEA.Library;
using OEA.MetaModel;
using FM.Commands;
using System.Windows.Input;
using System.Windows;

namespace FM.UI
{
    class FinanceLogInputModule : ModuleBase
    {
        protected override AggtBlocks DefineBlocks()
        {
            AggtBlocks blocks = new AggtBlocks
            {
                Layout = new LayoutMeta(typeof(TraditionalLayoutMethod<FinanceInputLayout>)),
                MainBlock = new Block(this.EntityType)
                {
                    BlockType = BlockType.Detail,
                    ExtendView = "FinanceLog 输入视图"
                },
                Surrounders =
                {
                    new SurrounderBlock(this.EntityType, "list")
                }
            };

            return blocks;
        }

        //protected override void OnItemCreated(Entity entity)
        //{
        //    base.OnItemCreated(entity);

        //    var log = entity as FinanceLog;
        //    log.Date = DateTime.Today;
        //}

        protected override void OnUIGenerated(ControlResult ui)
        {
            var defaultPersons = RF.Create<Person>().GetAll().Cast<Person>()
                .Where(t => t.IsDefault).Select(t => t.Name).ToArray();
            var defaultTags = RF.Create<Tag>().GetAll().Cast<Tag>()
                .Where(t => t.IsDefault).Select(t => t.Name).ToArray();
            var log = new FinanceLog
            {
                Users = string.Join(",", defaultPersons),
                Tags = string.Join(",", defaultTags)
            };

            var main = ui.MainView as DetailObjectView;
            main.Current = log;

            //暂时通过以下方式为按钮添加键盘事件。
            main.Control.KeyDown += (s, e) =>
            {
                if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Enter)
                {
                    //移除焦点，提交更改
                    var element = Keyboard.FocusedElement as FrameworkElement;
                    if (element != null)
                    {
                        element.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    }

                    main.Commands[typeof(ContinueAddFinanceLog)].TryExecute(main);
                }
            };

            var listView = main.TryFindRelation("list") as ListObjectView;
            listView.DataLoader.LoadDataAsync();
            listView.IsReadOnly = true;

            base.OnUIGenerated(ui);
        }
    }

    internal class FinanceLogInputDetailViewConfig : EntityConfig<FinanceLog>
    {
        protected override string ExtendView
        {
            get { return "FinanceLog 输入视图"; }
        }

        protected override void ConfigView()
        {
            View.ColumnsCountShowInDetail = 1;
            View.DetailLabelWidth = 50;

            View.ClearWPFCommands(false)
                .UseWPFCommands(
                typeof(ContinueAddFinanceLog)
                );
        }
    }
}
