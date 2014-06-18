/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130109 11:43
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130109 11:43
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Domain;

namespace Rafy.MultiLanguages
{
    [Serializable]
    [Contract, ContractImpl]
    public class AutoSaveLanguageService : Service
    {
        public string[] CollectedList { get; set; }

        protected override void Execute()
        {
            AutoAddDevLanguageItems();
            AutoAddOtherLanguageItems();
        }

        /// <summary>
        /// 自动把收集到的开发语言添加到数据库中
        /// </summary>
        private void AutoAddDevLanguageItems()
        {
            var collectedList = this.CollectedList;
            if (collectedList.Length > 0)
            {
                var items = RF.Concrete<DevLanguageItemRepository>().CacheAll() as DevLanguageItemList;

                foreach (var item in collectedList)
                {
                    items.FindOrCreate(item);
                }

                RF.Save(items);
            }
        }

        /// <summary>
        /// 自动把开发语言中所有的项都加入到所有其它语言中。
        /// 
        /// 此方法是线程安全的。
        /// </summary>
        private void AutoAddOtherLanguageItems()
        {
            var devItems = RF.Concrete<DevLanguageItemRepository>().CacheAll();
            var languages = RF.Concrete<LanguageRepository>().CacheAll();

            foreach (Language lang in languages)
            {
                var mappingList = lang.MappingInfoList;
                foreach (DevLanguageItem devItem in devItems)
                {
                    mappingList.FindOrCreate(devItem);
                }
            }

            RF.Save(languages);
        }
    }
}