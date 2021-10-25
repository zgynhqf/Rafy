/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120606 11:49
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120606 11:49
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy;
using Rafy.Domain;
using Rafy.Domain.Validation;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.ManagedProperty;
using System.Security.Permissions;
using System.Runtime.Serialization;
using Rafy.Domain.ORM;

namespace Rafy.DevTools
{
    [RootEntity, Serializable]
    public partial class TestDocument : IntEntity
    {
        #region 引用属性

        #endregion

        #region 子属性

        //public static readonly RefProperty<TestDocument> TestDocumentProperty =
        //    P<TestDocument>.RegisterRef(e => e.TestDocument, ReferenceType.Normal);
        //public int TestDocumentId
        //{
        //    get { return (int)this.GetRefId(TestDocumentProperty); }
        //    set { this.SetRefId(TestDocumentProperty, value); }
        //}
        //public TestDocument TestDocument
        //{
        //    get { return this.GetRefEntity(TestDocumentProperty); }
        //    set { this.SetRefEntity(TestDocumentProperty, value); }
        //}

        #endregion

        #region 一般属性

        public static readonly Property<string> NameProperty = P<TestDocument>.Register(e => e.Name);
        /// <summary>
        /// 名称
        /// 
        /// 名称名称名称名称名称名称名称名称名称名称名称名称
        /// 
        /// 名称名称名称
        /// 名称名称
        /// 名称名称名称名称
        /// </summary>
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        #endregion
    }

    [Serializable]
    public partial class TestDocumentList : EntityList { }

    public partial class TestDocumentRepository : EntityRepository
    {
        protected TestDocumentRepository() { }
    }

    [DataProviderFor(typeof(TestDocumentRepository))]
    public partial class TestDocumentDataProvider : RdbDataProvider
    {
        protected override string ConnectionStringSettingName
        {
            get { return string.Empty; }
        }
    }

    internal class TestDocumentConfig : EntityConfig<TestDocument>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllProperties();
        }
    }

    internal class TestDocumentWPFViewConfig : WPFViewConfig<TestDocument>
    {
        protected override void ConfigView()
        {
            View.DomainName("测试文档生成对象").HasDelegate(TestDocument.NameProperty);

            using (View.OrderProperties())
            {
                View.Property(TestDocument.NameProperty).HasLabel("名称").ShowIn(ShowInWhere.All);
            }
        }
    }
}