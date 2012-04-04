///*******************************************************
// * 
// * 作者：胡庆访
// * 创建时间：20120404
// * 说明：此文件只包含一个类，具体内容见类型注释。
// * 运行环境：.NET 4.0
// * 版本号：1.0.0
// * 
// * 历史记录：
// * 创建文件 胡庆访 20120404
// * 
//*******************************************************/

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using OEA.Library;
//using OEA.MetaModel;
//using OEA.MetaModel.Attributes;
//using OEA.MetaModel.View;

//namespace Demo
//{
//    [RootEntity, Serializable]
//    public class BookAdministrator : DemoEntity
//    {
//        public static readonly Property<string> UserNameProperty = P<BookAdministrator>.Register(e => e.UserName);
//        public string UserName
//        {
//            get { return this.GetProperty(UserNameProperty); }
//            set { this.SetProperty(UserNameProperty, value); }
//        }
//    }

//    [Serializable]
//    public class BookAdministratorList : DemoEntityList { }

//    public class BookAdministratorRepository : EntityRepository
//    {
//        protected BookAdministratorRepository() { }
//    }

//    internal class BookAdministratorConfig : EntityConfig<BookAdministrator>
//    {
//        protected override void ConfigMeta()
//        {
//            base.ConfigMeta();

//            Meta.MapTable().HasColumns(
//                BookAdministrator.UserNameProperty
//                );
//        }

//        protected override void ConfigView()
//        {
//            base.ConfigView();

//            View.HasLabel("管理员").HasTitle(BookAdministrator.UserNameProperty);

//            View.Property(BookAdministrator.UserNameProperty).HasLabel("姓名").ShowIn(ShowInWhere.All);
//        }
//    }
//}