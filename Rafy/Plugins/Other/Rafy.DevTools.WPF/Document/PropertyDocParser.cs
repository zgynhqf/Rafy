/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120606 11:45
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120606 11:45
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Rafy.MetaModel.View;

namespace Rafy.DevTools.Document
{
    /// <summary>
    /// 为指定实体模型生成对应的属性文档对象列表
    /// </summary>
    class PropertyDocParser
    {
        public EntityViewMeta EntityViewMeta { get; set; }

        public string ClassContent { get; set; }

        private List<PropertyDoc> _result;

        public IList<PropertyDoc> Parse()
        {
            if (this.EntityViewMeta == null) throw new ArgumentNullException("this.EntityViewMeta");
            if (string.IsNullOrWhiteSpace(this.ClassContent)) throw new ArgumentNullException("this.ClassContent");

            this.ParseCommentsFromContent();

            this._result = new List<PropertyDoc>();

            this.ParseDoc();

            return this._result;
        }

        #region 解析注释

        private Dictionary<string, string> _propertyToComment;

        private void ParseCommentsFromContent()
        {
            this._propertyToComment = new Dictionary<string, string>();

            var matches = Regex.Matches(this.ClassContent,
@"public static readonly ((List)|(Ref))Property\<.+?/// \<summary\>(?<content>.+?)\</summary\>.+?public \S+ (?<name>\S+)\s",
                RegexOptions.Singleline);

            foreach (Match match in matches)
            {
                var name = match.Groups["name"].Value;
                var comment = match.Groups["content"].Value;
                comment = Regex.Replace(comment, @" +/// ", string.Empty).Trim();

                this._propertyToComment[name] = comment;
            }
        }

        private string GetComment(string property)
        {
            string value = null;
            if (!this._propertyToComment.TryGetValue(property, out value))
            {
                value = string.Empty;
            }

            return value;
        }

        #endregion

        private void ParseDoc()
        {
            foreach (var epvm in this.EntityViewMeta.EntityProperties)
            {
                var doc = new PropertyDoc();

                doc.PropertyName = epvm.Name;
                doc.Label = epvm.Label;
                doc.Comment = this.GetComment(doc.PropertyName);

                this._result.Add(doc);
            }
        }
    }
}