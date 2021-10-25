///*******************************************************
// * 
// * 作者：胡庆访
// * 创建时间：20120413
// * 说明：此文件只包含一个类，具体内容见类型注释。
// * 运行环境：.NET 4.0
// * 版本号：1.0.0
// * 
// * 历史记录：
// * 创建文件 胡庆访 20120413
// * 
//*******************************************************/

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Rafy.Library;
//using Rafy.MetaModel;
//using Rafy.MetaModel.Attributes;
//using Rafy.MetaModel.View;

//namespace JXC
//{
//    [RootEntity]
//    public class Unit : JXCEntity
//    {
//        public static readonly Property<string> CodeProperty = P<Unit>.Register(e => e.Code);
//        public string Code
//        {
//            get { return this.GetProperty(CodeProperty); }
//            set { this.SetProperty(CodeProperty, value); }
//        }

//        public static readonly Property<string> NameProperty = P<Unit>.Register(e => e.Name);
//        public string Name
//        {
//            get { return this.GetProperty(NameProperty); }
//            set { this.SetProperty(NameProperty, value); }
//        }

//        public static readonly Property<string> DescriptionProperty = P<Unit>.Register(e => e.Description);
//        public string Description
//        {
//            get { return this.GetProperty(DescriptionProperty); }
//            set { this.SetProperty(DescriptionProperty, value); }
//        }
//    }

//    [Serializable]
//    public class UnitList : JXCEntityList { }

//    public class UnitRepository : JXCEntityRepository
//    {
//        protected UnitRepository() { }
//    }

//    internal class UnitConfig : EntityConfig<Unit>
//    {
//        protected override void ConfigMeta()
//        {
//            base.ConfigMeta();

//            Meta.MapTable().HasColumns(
//                Unit.CodeProperty,
//                Unit.NameProperty,
//                Unit.DescriptionProperty
//                );
//        }

//        protected override void ConfigView()
//        {
//            base.ConfigView();

//            View.DomainName("计量单位").HasDelegate(Unit.NameProperty);

//            View.Property(Unit.CodeProperty).HasLabel("编码").ShowIn(ShowInWhere.All);
//            View.Property(Unit.NameProperty).HasLabel("名称").ShowIn(ShowInWhere.All);
//            View.Property(Unit.DescriptionProperty).HasLabel("单位说明").ShowIn(ShowInWhere.All);
//        }
//    }
//}