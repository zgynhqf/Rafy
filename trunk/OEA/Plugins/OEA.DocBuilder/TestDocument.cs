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
using OEA;
using OEA.Library;
using OEA.Library.Validation;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;
using OEA.ManagedProperty;

namespace OEA.DocBuilder
{
    [RootEntity, Serializable]
    public class TestDocument : Entity
    {
        protected override string ConnectionStringSettingName
        {
            get { return string.Empty; }
        }

        #region 引用属性

        #endregion

        #region 子属性

        //public static readonly RefProperty<TestDocument> TestDocumentRefProperty =
        //    P<TestDocument>.RegisterRef(e => e.TestDocument, ReferenceType.Normal);
        //public int TestDocumentId
        //{
        //    get { return this.GetRefId(TestDocumentRefProperty); }
        //    set { this.SetRefId(TestDocumentRefProperty, value); }
        //}
        //public TestDocument TestDocument
        //{
        //    get { return this.GetRefEntity(TestDocumentRefProperty); }
        //    set { this.SetRefEntity(TestDocumentRefProperty, value); }
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
    public class TestDocumentList : EntityList { }

    public class TestDocumentRepository : EntityRepository
    {
        protected TestDocumentRepository() { }
    }

    internal class TestDocumentConfig : EntityConfig<TestDocument>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllPropertiesToTable();
        }

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