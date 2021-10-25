/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121107 23:25
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121107 23:25
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.Domain;
using Rafy.Domain.ORM;
using Rafy.Domain.ORM.Query;
using Rafy.Domain.Validation;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;

namespace Rafy.MultiLanguages
{
    /// <summary>
    /// 开发语言项
    /// </summary>
    [RootEntity]
    public partial class DevLanguageItem : MLEntity
    {
        #region 引用属性

        #endregion

        #region 子属性

        #endregion

        #region 一般属性

        public static readonly Property<string> ContentProperty = P<DevLanguageItem>.Register(e => e.Content);
        public string Content
        {
            get { return this.GetProperty(ContentProperty); }
            set { this.SetProperty(ContentProperty, value); }
        }

        #endregion

        #region 只读属性

        #endregion
    }

    public partial class DevLanguageItemList : MLEntityList
    {
        public DevLanguageItem Find(string content)
        {
            content = content.Trim();
            var devItemFound = this.Cast<DevLanguageItem>().FirstOrDefault(i => i.Content == content);
            return devItemFound;
        }

        public DevLanguageItem FindOrCreate(string content)
        {
            content = content.Trim();

            var devItemFound = this.Cast<DevLanguageItem>().FirstOrDefault(i => i.Content == content);
            if (devItemFound == null)
            {
                devItemFound = new DevLanguageItem { Content = content };
                this.Add(devItemFound);
            }

            return devItemFound;
        }
    }

    public partial class DevLanguageItemRepository : MLEntityRepository
    {
        protected DevLanguageItemRepository() { }
    }

    [DataProviderFor(typeof(DevLanguageItemRepository))]
    public partial class DevLanguageItemDataProvider : RdbDataProvider
    {
        public DevLanguageItemDataProvider()
        {
            this.DataSaver = new DevLanguageItemSaver();
            this.DataQueryer = new DevLanguageItemQueryer();
        }

        private class DevLanguageItemQueryer : RdbDataQueryer
        {
            protected override void OnQuerying(ORMQueryArgs args)
            {
                var query = args.Query;
                query.OrderBy.Add(query.MainTable.Column(DevLanguageItem.ContentProperty));

                base.OnQuerying(args);
            }
        }

        private class DevLanguageItemSaver : RdbDataSaver
        {
            protected override void Submit(SubmitArgs e)
            {
                if (e.Action == SubmitAction.Delete)
                {
                    //在删除前需要删除所有语言中的映射项。
                    this.DeleteRef(e.Entity, MappingInfo.DevLanguageItemProperty);
                }

                base.Submit(e);
            }
        }
    }

    internal class DevLanguageItemConfig : EntityConfig<DevLanguageItem>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllProperties();

            Meta.EnableClientCache(5000);
        }
    }
}