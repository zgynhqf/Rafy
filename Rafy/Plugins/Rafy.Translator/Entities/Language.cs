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
using Rafy.Domain.Validation;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;

namespace Rafy.MultiLanguages
{
    /// <summary>
    /// 语言
    /// </summary>
    [RootEntity, Serializable]
    public partial class Language : MLEntity
    {
        #region 引用属性

        #endregion

        #region 子属性

        public static readonly ListProperty<MappingInfoList> MappingInfoListProperty = P<Language>.RegisterList(e => e.MappingInfoList);
        public MappingInfoList MappingInfoList
        {
            get { return this.GetLazyList(MappingInfoListProperty); }
        }

        #endregion

        #region 一般属性

        public static readonly Property<string> CodeProperty = P<Language>.Register(e => e.Code);
        /// <summary>
        /// 只能使用文化代码，否则该语言将无法使用，详见：
        /// http://msdn.microsoft.com/en-us/goglobal/bb896001.aspx
        /// </summary>
        public string Code
        {
            get { return this.GetProperty(CodeProperty); }
            set { this.SetProperty(CodeProperty, value); }
        }

        public static readonly Property<string> NameProperty = P<Language>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        public static readonly Property<string> BingAPICodeProperty = P<Language>.Register(e => e.BingAPICode);
        /// <summary>
        /// Bing 翻译引擎对应的语言编码
        /// </summary>
        public string BingAPICode
        {
            get { return this.GetProperty(BingAPICodeProperty); }
            set { this.SetProperty(BingAPICodeProperty, value); }
        }

        public static readonly Property<bool> NeedCollectProperty = P<Language>.Register(e => e.NeedCollect);
        /// <summary>
        /// 是否在该语言下执行收集操作。
        /// 
        /// 例如在中文模块中，大量的字符串都是不需要翻译的，这时也不需要收集。
        /// 否则系统会认为都没有翻译，而把所有字符串收集起来。
        /// </summary>
        public bool NeedCollect
        {
            get { return this.GetProperty(NeedCollectProperty); }
            set { this.SetProperty(NeedCollectProperty, value); }
        }

        #endregion

        #region 只读属性

        #endregion

        //已经使用空格处理冲突，这里不需要强制验证了。
        //protected override void AddValidations()
        //{
        //    base.AddValidations();

        //    this.ValidationRules.AddRule((target, e) =>
        //    {
        //        var language = target as Language;
        //        var mappings = language.MappingInfoList.Cast<MappingInfo>();
        //        foreach (var m1 in mappings)
        //        {
        //            foreach (var m2 in mappings)
        //            {
        //                if (m1 != m2 && m1.TranslatedTextRO == m2.TranslatedTextRO)
        //                {
        //                    e.BrokenDescription = "存在两项使用同样的翻译值。";
        //                    return;
        //                }
        //            }
        //        }
        //    });
        //}
    }

    [Serializable]
    public partial class LanguageList : MLEntityList { }

    public partial class LanguageRepository : MLEntityRepository
    {
        protected LanguageRepository() { }

        public Language GetByCode(string code)
        {
            return this.CacheAll().Concrete().FirstOrDefault(e => e.Code == code);
        }
    }

    internal class LanguageConfig : EntityConfig<Language>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllProperties();

            Meta.EnableClientCache(ClientCacheScopeType.Table);
        }
    }
}