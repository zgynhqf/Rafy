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
    /// 某一语言下的映射信息
    /// </summary>
    [ChildEntity, Serializable]
    public partial class MappingInfo : MLEntity
    {
        #region 构造函数

        public MappingInfo() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected MappingInfo(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        #region 引用属性

        public static readonly IRefIdProperty LanguageIdProperty =
            P<MappingInfo>.RegisterRefId(e => e.LanguageId, ReferenceType.Parent);
        public int LanguageId
        {
            get { return (int)this.GetRefId(LanguageIdProperty); }
            set { this.SetRefId(LanguageIdProperty, value); }
        }
        public static readonly RefEntityProperty<Language> LanguageProperty =
            P<MappingInfo>.RegisterRef(e => e.Language, LanguageIdProperty);
        public Language Language
        {
            get { return this.GetRefEntity(LanguageProperty); }
            set { this.SetRefEntity(LanguageProperty, value); }
        }

        public static readonly IRefIdProperty DevLanguageItemIdProperty =
            P<MappingInfo>.RegisterRefId(e => e.DevLanguageItemId, ReferenceType.Normal);
        public int DevLanguageItemId
        {
            get { return (int)this.GetRefId(DevLanguageItemIdProperty); }
            set { this.SetRefId(DevLanguageItemIdProperty, value); }
        }
        public static readonly RefEntityProperty<DevLanguageItem> DevLanguageItemProperty =
            P<MappingInfo>.RegisterRef(e => e.DevLanguageItem, DevLanguageItemIdProperty);
        /// <summary>
        /// 对应开发语言的字符串
        /// </summary>
        public DevLanguageItem DevLanguageItem
        {
            get { return this.GetRefEntity(DevLanguageItemProperty); }
            set { this.SetRefEntity(DevLanguageItemProperty, value); }
        }

        #endregion

        #region 子属性

        #endregion

        #region 一般属性

        public static readonly Property<string> TranslatedTextProperty = P<MappingInfo>.Register(e => e.TranslatedText);
        /// <summary>
        /// 翻译的文本
        /// 本语言中对应开发语言的字符串
        /// </summary>
        public string TranslatedText
        {
            get { return this.GetProperty(TranslatedTextProperty); }
            set { this.SetProperty(TranslatedTextProperty, value); }
        }

        #endregion

        #region 只读属性

        public static readonly Property<string> DevLanguageRDProperty = P<MappingInfo>.RegisterRedundancy(e => e.DevLanguageRD,
            new RedundantPath(DevLanguageItemProperty, DevLanguageItem.ContentProperty));
        /// <summary>
        /// 对应的开发语言
        /// 冗余属性
        /// </summary>
        public string DevLanguageRD
        {
            get { return this.GetProperty(DevLanguageRDProperty); }
        }

        #endregion
    }

    [Serializable]
    public partial class MappingInfoList : MLEntityList
    {
        public MappingInfo FindOrCreate(DevLanguageItem devLanguageItem)
        {
            var item = this.Cast<MappingInfo>().FirstOrDefault(mi => mi.DevLanguageItemId == devLanguageItem.Id);

            if (item == null)
            {
                item = new MappingInfo
                {
                    DevLanguageItem = devLanguageItem
                };
                this.Add(item);
            }

            return item;
        }
    }

    public partial class MappingInfoRepository : MLEntityRepository
    {
        protected MappingInfoRepository() { }
    }

    [DataProviderFor(typeof(MappingInfoRepository))]
    public partial class MappingInfoRepositoryDataProvider : RdbDataProvider
    {
        public MappingInfoRepositoryDataProvider()
        {
            this.DataQueryer = new MappingInfoQueryer();
        }

        private class MappingInfoQueryer : RdbDataQueryer
        {
            protected override void OnQuerying(EntityQueryArgs args)
            {
                var query = args.Query;
                query.OrderBy.Add(query.MainTable.Column(MappingInfo.DevLanguageRDProperty));

                base.OnQuerying(args);
            }
        }
    }

    internal class MappingInfoConfig : EntityConfig<MappingInfo>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable("LanguageMappingInfo").MapAllProperties();

            Meta.EnableClientCache(ClientCacheScopeType.ScopedByRoot);
        }
    }
}