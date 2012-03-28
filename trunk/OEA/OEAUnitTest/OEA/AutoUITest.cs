///*******************************************************
// * 
// * 作者：胡庆访
// * 创建时间：20110927
// * 说明：此文件只包含一个类，具体内容见类型注释。
// * 运行环境：.NET 4.0
// * 版本号：1.0.0
// * 
// * 历史记录：
// * 创建文件 胡庆访 20110927
// * 
//*******************************************************/

//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using OEA.Library;
//using OEA.MetaModel;
//using System.Linq;
//using OEA;
//using OEA.Module.WPF.Layout;
//using OEA.Module.WPF;
//
//using OEA.MetaModel.WPF.Layout;

//namespace OEAUnitTest
//{
//    [TestClass]
//    public class AutoUITest : TestBase
//    {
//        [ClassInitialize]
//        public static void AutoUITest_ClassInitialize(TestContext context)
//        {
//            ClassInitialize(context, true);
//        }

//        //[TestMethod]
//        //public void AutoUITest_CreateUIBlock()
//        //{
//        //    var r = RF.Create<IndicatorType2222222>();
//        //    var e = r.New() as IndicatorType2222222;

//        //    //检测条件
//        //    var denpendentObject = e as IDenpendentObject;
//        //    if (denpendentObject != null)
//        //    {
//        //        denpendentObject.CheckRules();
//        //        if (denpendentObject.BrokenRulesCollection.Count > 0)
//        //        {
//        //            App.Current.MessageBox.Show(denpendentObject.BrokenRulesCollection[0].Description, "保存出错");
//        //            return;
//        //        }
//        //    }
//        //    //object l = e.QuantityIndicators;
//        //    //l = e.ResourceIndicators;
//        //    //l = e.RatioIndicators;

//        //    var array = Serializer.Serialize(e);
//        //    var e2 = Serializer.Deserialize(array) as IndicatorType2222222;
//        //    Assert.AreEqual(e.Id, e2.Id);

//        //    DiffSaveCommand.Execute(e);
//        //}

//        [TestMethod]
//        public void AutoUITest_CreateUIBlock()
//        {
//            var entityType = typeof(Org);
//            var entityInfo = AppModel.DefaultViews.Get(entityType);

//            var block = new UIModelDefiner().DefineBlock(entityType);

//            //Name
//            Assert.AreEqual(block.TypeName, entityType.Name);

//            //Commands
//            var commands = AppModel.Commands.GetAvailableCommands(entityInfo);
//            Assert.AreEqual(block.Commands.Count, commands.Count);

//            //Assert.IsNull(block.ControlFactoryType);

//            Assert.AreEqual(block.GenerateType, GenerateType.List);

//            Assert.AreEqual(block.EntityViewInfo, entityInfo);
//        }

//        [TestMethod]
//        public void AutoUITest_CreateTypeBlocks()
//        {
//            var entityType = typeof(Org);

//            var blocks = new UIModelDefiner().DefineAggregate(entityType);

//            Assert.AreEqual(blocks.TypeName, entityType.Name);

//            Assert.AreEqual(blocks.MainBlock.TypeName, entityType.Name);

//            //var entityInfo = AppModel.Entities.FindViewMeta(entityType);
//            //var commands = BoInfoOperationList.GetList(entityInfo).AsEnumerable()
//            //    .Cast<BoInfoOperation>().Select(o => o.Operation.Command).ToList();
//            //Assert.AreEqual(blocks.MainBlock.Commands.Count, commands.Count);

//            Assert.AreEqual(blocks.Surrounders.Count, 0);

//            Assert.AreEqual(blocks.Children.Count, 1);

//            Assert.IsNull(blocks.LayoutType);
//        }

//        //[TestMethod]
//        //public void AutoUITest_UIInfoRepository()
//        //{
//        //    var entityType = typeof(PBS);
//        //    var blocks = new UIModelDefiner().DefineAggregate(entityType);

//        //    blocks.LayoutType = typeof(TraditionalLayoutMethod<NaviListDetailLayout>);

//        //    var repo = UIInfoRepository.Instance;

//        //    //save
//        //    var key = "temp";
//        //    repo.Save(key, blocks);

//        //    //get
//        //    blocks = repo.FindUIInfo(typeof(PBS), key);
//        //    Assert.IsNotNull(blocks);
//        //    Assert.AreEqual(blocks.LayoutType, typeof(TraditionalLayoutMethod<NaviListDetailLayout>));

//        //    //delete
//        //    repo.Save(key, null);
//        //    blocks = repo.FindUIInfo(typeof(PBS), key);
//        //    Assert.IsNull(blocks);
//        //}

//        ///// <summary>
//        ///// 此方法需要把环境配置为客户端环境，才能运行成功。
//        ///// </summary>
//        //[TestMethod]
//        //public void AutoUITest_CreateUI_Single()
//        //{
//        //    var entityType = typeof(Project);
//        //    var entityInfo = AppModel.Entities.FindViewMeta(entityType);

//        //    var view = AutoUI.ViewFactory.CreateListObjectView(entityInfo);

//        //    var grid = view.Control;

//        //    Assert.IsNotNull(grid);
//        //}
//    }
//}