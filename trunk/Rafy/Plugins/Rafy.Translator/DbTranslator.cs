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
using System.Text;
using Rafy.Domain;
using Rafy.Threading;

namespace Rafy.MultiLanguages
{
    internal class DbTranslator : Translator
    {
        public static readonly DbTranslator Instance = new DbTranslator();

        private DbTranslator() { }

        /// <summary>
        /// 收集到多少个时，发生自动保存。
        /// </summary>
        private const int AutoSaveOnCollectGate = 100;

        /// <summary>
        /// 正向翻译的高速集合。
        /// </summary>
        private Dictionary<string, string> _transDic;

        /// <summary>
        /// 逆向翻译的高速集合。
        /// </summary>
        private Dictionary<string, string> _transReverseDic;

        #region 自动更新保存

        /// <summary>
        /// 在收集时，如果大于一定的量，则自动保存。
        /// </summary>
        /// <param name="devLanguage"></param>
        protected override void OnCollected(string devLanguage)
        {
            base.OnCollected(devLanguage);

            //如果当前收集项大于 AutoSaveOnCollectGate，则触发自动收集
            if (base.CollectedList.Count >= AutoSaveOnCollectGate)
            {
                this.AutoSave();
            }
        }

        /// <summary>
        /// 自动保存新的开发语言及其它语言的相应项。
        /// </summary>
        public void AutoSave()
        {
            var collectedList = this.CollectedList;
            if (collectedList.Count > 0)
            {
                var svc = ServiceFactory.Create<AutoSaveLanguageService>();
                svc.CollectedList = collectedList.ToArray();
                svc.Invoke();

                collectedList.Clear();
                this.InvalidateItems();
            }
        }

        #endregion

        protected override void OnCurrentCultureChanged()
        {
            base.OnCurrentCultureChanged();

            //根据当前语言文件化设定，来重新设置 CollectionEnabled
            var value = RafyEnvironment.Configuration.Section.CollectDevLanguages.ToBoolean();
            var lang = RF.Concrete<LanguageRepository>().GetByCode(this.CurrentCulture);
            if (lang != null)
            {
                value &= lang.NeedCollect;
            }
            this.AutoCollect = value;

            //让所有缓存失效。
            this.InvalidateItems();
        }

        protected override bool TranslateCore(string devLanguage, out string result)
        {
            result = devLanguage;
            this.EnsureDicCreated();
            return this._transDic.TryGetValue(devLanguage, out result);
        }

        protected override string TranslateReverseCore(string currentLanguage)
        {
            this.EnsureDicCreated();
            string result = null;
            if (this._transReverseDic.TryGetValue(currentLanguage, out result))
            {
                return result;
            }

            throw new InvalidProgramException(string.Format("没有找到对应“ {0} ”的开发语言！", currentLanguage));
        }

        private void EnsureDicCreated()
        {
            if (this._transDic == null)
            {
                var lang = RF.Concrete<LanguageRepository>().GetByCode(this.CurrentCulture);

                //如果设置的语言已经在数据库中定义好了，则生成相应的两个字典，否则生成两个空字典。
                if (lang != null)
                {
                    this._transDic = new Dictionary<string, string>(lang.MappingInfoList.Count);
                    this._transReverseDic = new Dictionary<string, string>(lang.MappingInfoList.Count);

                    //把 MappingList 中的所有项都加入到两个 Dictionary 集合中。
                    var devItems = RF.Concrete<DevLanguageItemRepository>().CacheAll();
                    foreach (MappingInfo item in lang.MappingInfoList)
                    {
                        var devItem = devItems.Find(item.DevLanguageItemId) as DevLanguageItem;
                        var devContent = devItem.Content;

                        //如果 MappingInfoList 中有重复的值，则只添加一次即可。
                        if (!this._transDic.ContainsKey(devContent))
                        {
                            var translatedText = item.TranslatedText;
                            if (!string.IsNullOrEmpty(translatedText))
                            {
                                while (this._transReverseDic.ContainsKey(translatedText))
                                {
                                    translatedText += " ";//使用多余的空格翻译处理冲突
                                }

                                this._transDic.Add(devContent, translatedText);
                                this._transReverseDic.Add(translatedText, devContent);
                            }
                        }
                    }
                }
                else
                {
                    this._transDic = new Dictionary<string, string>();
                    this._transReverseDic = new Dictionary<string, string>();
                }
            }
        }

        public override IList<string> GetSupportCultures()
        {
            var items = RF.Concrete<LanguageRepository>().CacheAll();
            var codes = items.Cast<Language>().Select(i => i.Code).ToArray();
            return codes;
        }

        /// <summary>
        /// 通知所有数据不可用。
        /// 
        /// 此方法是线程安全的。
        /// </summary>
        internal void InvalidateItems()
        {
            this._transDic = null;
            this._transReverseDic = null;
        }
    }
}