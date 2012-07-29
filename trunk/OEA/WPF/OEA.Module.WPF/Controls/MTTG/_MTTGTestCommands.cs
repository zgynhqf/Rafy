///*******************************************************
// * 
// * 作者：胡庆访
// * 创建时间：20111125
// * 说明：MultiTypesTreeGrid 测试代码
// * 运行环境：.NET 4.0
// * 版本号：1.0.0
// * 
// * 历史记录：
// * 创建文件 胡庆访 20111202
// * 
//*******************************************************/

//using System;
//using System.IO;
//using System.Linq;
//using System.Windows.Forms;
//
//
//using GIX4.Library;
//using OEA;
//using OEA.MetaModel;
//using OEA.MetaModel.View;
//using OEA.MetaModel.Attributes;
//using OEA.Module.WPF;
//using OEA.Module.WPF.Controls;
//
//using OEA.WPF.Command;
//using OEA.Library;
//using OEA.WPF.Command;
//using System.ComponentModel;
//using System.Windows.Data;
//using OEA.ManagedProperty;
//using System.Text;
//using OEA.Module.WPF.CommandAutoUI;
//using BQNorm.Library;
//using System.Collections;
//using System.Collections.Generic;

//namespace GIX4.Module.WPF
//{
//    public class SequenceTestCommand : ListViewCommand
//    {
//        private Action<ListObjectView>[] _actions;

//        private Random _ran = new Random();

//        protected bool IndexMode { get; set; }

//        private int _index = 0;

//        public SequenceTestCommand()
//        {
//            this.IndexMode = true;
//        }

//        protected void PrepareActions(params Action<ListObjectView>[] actions)
//        {
//            this._actions = actions;
//        }

//        public override void Execute(ListObjectView view)
//        {
//            if (this.IndexMode)
//            {
//                this._actions[this._index](view);

//                this._index++;
//                this._index %= this._actions.Length;
//            }
//            else
//            {
//                var r = this._ran.Next(this._actions.Length);
//                this._actions[r](view);
//            }
//        }
//    }

//    [Command("0744DAA4-2619-4FFA-BCC2-F4B0218CA850", typeof(PBS), Index = 100, Label = "测试 Filter")]
//    public class TestFilterCommand : SequenceTestCommand
//    {
//        public TestFilterCommand()
//        {
//            this.PrepareActions(
//                view => view.Filter = e => e.CastTo<PBS>().Code.Contains("1"),
//                view => view.Filter = e => e.CastTo<PBS>().Code.Contains("0") || e.CastTo<PBS>().Code.Contains("2"),
//                view => view.Filter = null
//                );
//        }
//    }

//    [Command("5692543B-8AFD-44C3-919E-355F2A259122", typeof(PBSProperty), typeof(PBS), typeof(UnitDictionary), Index = 100, Label = "测试 Group")]
//    public class TestGroupCommand : SequenceTestCommand
//    {
//        public TestGroupCommand()
//        {
//            this.PrepareActions(
//                view => view.RootGroupDescriptions = new string[] { 
//                    PBSProperty.CodeProperty.Name, PBSProperty.NameProperty.Name
//                },
//                view => view.RootGroupDescriptions = null
//                );
//        }
//    }

//    [Command("9DFC95F4-F0EA-4E37-B8A3-6AD1A05F2234", typeof(PBSProperty), typeof(PBS), typeof(UnitDictionary),
//        UIAlgorithm = typeof(GenericItemAlgorithm<TextBoxGenerator>),
//        Index = 100, Label = "AnyGrouping")]
//    public class TestGroupAnyCommand : ListViewCommand
//    {
//        public override void Execute(ListObjectView view)
//        {
//            var value = this.TryGetCustomParams<string>(CommandCustomParams.TextBox);

//            if (string.IsNullOrEmpty(value))
//            {
//                view.RootGroupDescriptions = null;
//            }
//            else
//            {
//                var properties = value.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
//                    .Select(label => view.EntityViewInfo.EntityProperties.FirstOrDefault(ep => ep.Label == label))
//                    .Where(p => p != null)
//                    .Select(p => p.Name)
//                    .ToArray();
//                view.RootGroupDescriptions = properties;
//            }
//        }

//        #region TextBoxGenerator

//        public class TextBoxGenerator : ItemGenerator
//        {
//            public TextBoxGenerator(CommandGroup group, CommandAutoUIContext context) : base(group, context) { }

//            protected override ItemControlResult CreateItemControl()
//            {
//                var commandInfo = this.CommandItem;
//                var cmd = CommandRepository.NewCommand(commandInfo);

//                var command = cmd.CoreCommand.CastTo<TestGroupAnyCommand>();

//                var textBox = CreateTextBox(command);

//                //TextChanged时，执行Command
//                textBox.KeyDown += (o, e) =>
//                {
//                    if (e.Key == System.Windows.Input.Key.Enter)
//                    {
//                        CommandRepository.TryExecuteCommand(command, this.Context.CommandArg);
//                    }
//                };

//                return new ItemControlResult(textBox, cmd);
//            }
//        }

//        #endregion
//    }

//    [Command("2C766B56-D7D8-4C40-A93E-EB9584177E8F", typeof(PBS), Index = 100, Label = "测试 Sort")]
//    public class TestSortCommand : SequenceTestCommand
//    {
//        public TestSortCommand()
//        {
//            this.PrepareActions(
//                view => view.SortDescriptions = new SortDescription[]{
//                        new SortDescription(PBS.CodeProperty.Name, ListSortDirection.Descending)
//                    },
//                view => view.SortDescriptions = null
//                );
//        }
//    }

//    [Command("17656D75-8120-4A31-82F7-3AD6224B933B", typeof(PBS), Index = 100, Label = "测试 CheckMode")]
//    public class TestCheckModeCommand : SequenceTestCommand
//    {
//        public TestCheckModeCommand()
//        {
//            this.PrepareActions(
//                view =>
//                {
//                    App.MessageBox.Show("CheckingRowCascade.None");
//                    view.CheckingRowCascade = CheckingRowCascade.None;
//                    view.CheckingMode = CheckingMode.CheckingRow;
//                },
//                view =>
//                {
//                    App.MessageBox.Show("CheckingRowCascade.CascadeParent");
//                    view.CheckingRowCascade = CheckingRowCascade.CascadeParent;
//                    view.CheckingMode = CheckingMode.CheckingRow;
//                },
//                view =>
//                {
//                    App.MessageBox.Show("CheckingRowCascade.CascadeChildren");
//                    view.CheckingRowCascade = CheckingRowCascade.CascadeChildren;
//                    view.CheckingMode = CheckingMode.CheckingRow;
//                },
//                view =>
//                {
//                    App.MessageBox.Show("CheckingRowCascade.CascadeParent | CheckingRowCascade.CascadeChildren");
//                    view.CheckingRowCascade = CheckingRowCascade.CascadeParent | CheckingRowCascade.CascadeChildren;
//                    view.CheckingMode = CheckingMode.CheckingRow;
//                },
//                //view => view.CheckingMode = CheckingMode.CheckingViewModel,
//                view => view.CheckingMode = CheckingMode.None
//                );
//        }
//    }

//    [Command("052838B9-27E5-4E22-A39C-4F5ABBB0B292", typeof(PBS), Index = 100, Label = "测试 Count")]
//    public class GetCountCommand : ListViewCommand
//    {
//        public override void Execute(ListObjectView view)
//        {
//            var dataCount = "null";
//            var selectedObjectsCount = "null";

//            if (view.Data != null) dataCount = view.Data.Count.ToString();
//            if (view.SelectedObjects != null) selectedObjectsCount = view.SelectedObjects.Count.ToString();

//            var msg = string.Format("Data：{0}；\n\rSelectedObjects：{1}。", dataCount, selectedObjectsCount);

//            App.MessageBox.Show(msg);
//        }
//    }

//    [Command("42A0782E-A0DC-474A-9301-C2E8A98E913A", typeof(UnitDictionary), Index = 100, Label = "测试 RowHeader")]
//    public class TestRowHeaderCommand : ListViewCommand
//    {
//        public override void Execute(ListObjectView view)
//        {
//            var type = RF.Create<PBSType>().GetAll()
//                .Cast<PBSType>().First(t => t.Code.Contains("G002"));

//            var control = AutoUI.GenerateAggtControl(typeof(PBS), (l, d) =>
//            {
//                l.Children.Clear();
//            });
//            control.MainView.Data = type.PBSs;
//            (control.MainView as ListObjectView).IsReadOnly = true;

//            App.Windows.ShowWindow(control.Control);
//        }
//    }

//    [Command("E2D32D1C-7417-43FC-AEA4-B65D5D625528", typeof(UnitDictionary), Label = "选择定额")]
//    public class TestPerformaceCommand : ListViewCommand
//    {
//        //private static Type testType = typeof(BQItem);
//        ////private static Type testType = typeof(PBS);

//        //private static Entity[] All;

//        //public override void Execute(ListObjectView view)
//        //{
//        //    if (All == null)
//        //    {
//        //        All = RF.Create(testType).GetAll().Cast<Entity>().ToArray();
//        //        MessageBox.Show("数据加载完成，条数：" + All.Length);
//        //    }

//        //    var controlResult = AutoUI.ViewFactory.CreateListObjectView(testType);

//        //    controlResult.Data = All;

//        //    App.Windows.ShowDialog(controlResult.Control);
//        //}

//        public override void Execute(ListObjectView view)
//        {
//            var controlResult = AutoUI.ViewFactory.CreateListObjectView(typeof(NormItem));

//            var c = new NormItemNavigateCriteria
//            {
//                NormInfoId = new Guid("c9a2ac8d-feb2-44bb-ba33-d97813f266c6"),
//                NormSectionId = new Guid("f7575daa-79e2-49a2-93c4-ffe63fb00f87"),
//                NormTradeId = new Guid("cbaf29d7-bc22-4754-b9a7-1964e77a36fb"),
//            };

//            var data = RF.Concreate<NormItemRepository>().GetList(c);

//            //public NormItemList GetList(NormItemNavigateCriteria n)
//            //{
//            //    return this.FetchListCast<NormItemList>(n);
//            //}

//            controlResult.Data = data;

//            App.Windows.ShowDialog(controlResult.Control, w =>
//            {
//                w.AddCommand("5692543B-8AFD-44C3-919E-355F2A259122", controlResult);
//            });
//        }
//    }
//}