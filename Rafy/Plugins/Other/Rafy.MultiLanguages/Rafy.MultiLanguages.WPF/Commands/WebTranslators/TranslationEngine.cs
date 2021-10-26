/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121109 17:11
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121109 17:11
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Rafy.MultiLanguages
{
    public abstract class TranslationEngine
    {
        private const int MaxWordsValve = 1000;

        private WebClient _client;

        protected WebClient Client
        {
            get
            {
                if (this._client == null)
                {
                    this._client = new WebClient
                    {
                        Encoding = Encoding.UTF8
                    };
                }

                return this._client;
            }
        }

        public void Translate(Language language, List<MappingInfo> toDoList)
        {
            this.OnTranslating();

            var batch = new List<MappingInfo>(100);
            int length = 0;
            for (int i = 0, c = toDoList.Count; i < c; i++)
            {
                var mapping = toDoList[i];

                length += mapping.DevLanguageRD.Length;
                batch.Add(mapping);

                if (length > MaxWordsValve || i == c - 1)
                {
                    var translatedWords = this.Request(batch, language.BingAPICode);

                    //如果返回的个数不一致，说明出现异常，直接返回。
                    if (batch.Count != translatedWords.Count) { return; }

                    //设置自动翻译的值。
                    for (int j = 0; j < batch.Count; j++)
                    {
                        batch[j].TranslatedText = translatedWords[j];
                    }

                    length = 0;
                    batch.Clear();
                }
            }
        }

        protected virtual void OnTranslating() { }

        protected abstract IList<string> Request(IList<MappingInfo> batch, string language);
    }
}
