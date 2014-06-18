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
using System.Text;
using System.Text.RegularExpressions;
using Rafy;

namespace Rafy.MultiLanguages
{
    public class Baidu : TranslationEngine
    {
        protected override IList<string> Request(IList<MappingInfo> batch, string language)
        {
            if (!language.EqualsIgnoreCase("en") && !language.EqualsIgnoreCase("jp"))
            {
                throw new InvalidProgramException("百度翻译引擎目前只支持英文、日文。");
            }

            var words = new StringBuilder();
            for (int i = 0, c = batch.Count; i < c; i++)
            {
                var mapping = batch[i];
                if (i > 0) words.Append(Environment.NewLine);
                var word = mapping.DevLanguageRD.Replace("#", string.Empty).Replace(Environment.NewLine, string.Empty);
                words.Append(word);
            }

            string url = @"http://fanyi.baidu.com/transcontent";

            string data = string.Format(
@"ie=utf-8&source=txt&query={0}&t=1352446403520&token=65edeb8dd1450f32667ac77d619501a48&from=zh&to={1}"
, words, language);

            this.Client.Headers["Content-Type"] = "application/x-www-form-urlencoded";
            var res = this.Client.UploadString(url, data);

            return this.ParseResultInJson(res);
        }

        private IList<string> ParseResultInJson(string json)
        {
            var matches = Regex.Matches(json, @"dst"":""(?<dst>[^""]+)""");

            var result = new List<string>(matches.Count);

            foreach (Match match in matches)
            {
                var translatedText = match.Groups["dst"].Value;
                result.Add(translatedText);
            }

            return result;
        }
    }
}