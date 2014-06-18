using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy.MetaModel.View;
using Rafy.WPF;
using Rafy.Domain;
using Rafy.MetaModel;
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
                Layout = new LayoutMeta(typeof(FinanceInputLayout)),
                MainBlock = new Block(this.EntityType)
                {
                    BlockType = BlockType.Detail,
                    KeyLabel = "经费输入",
                    ExtendView = typeof(FinanceLogInputDetailWPFViewConfig)
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
            var defaultPersons = RF.Find<Person>().GetAll().Cast<Person>()
                .Where(t => t.IsDefault).Select(t => t.Name).ToArray();
            var defaultTags = RF.Find<Tag>().GetAll().Cast<Tag>()
                .Where(t => !t.NotUsed && t.IsDefault).Select(t => t.Name).ToArray();
            var log = new FinanceLog
            {
                Users = string.Join(",", defaultPersons),
                Tags = string.Join(",", defaultTags)
            };

            var main = ui.MainView as DetailLogicalView;
            main.Current = log;
            main.Control.Loaded += (s, e) => (main.PropertyEditors[0].Control as FrameworkElement).Focus();

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

                    main.Commands[typeof(ContinueAddFinanceLog)].TryExecute();
                }
            };

            var listView = main.Relations["list"] as ListLogicalView;
            listView.DataLoader.LoadDataAsync();
            listView.IsReadOnly = ReadOnlyStatus.ReadOnly;

            base.OnUIGenerated(ui);
        }
    }

    internal class FinanceLogInputDetailWPFViewConfig : WPFViewConfig<FinanceLog>
    {
        /// <summary>
        /// 子类重写此方法，并完成对 Meta 属性的配置。
        /// 注意：
        /// * 为了给当前类的子类也运行同样的配置，这个方法可能会被调用多次。
        /// </summary>
        protected override void ConfigView()
        {
            View.HasDetailLabelSize(50).HasDetailColumnsCount(1);

            View.ClearCommands(false)
                .UseCommands(
                typeof(ContinueAddFinanceLog)
                );
        }
    }
}
