/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130412 13:50
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130412 13:50
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Rafy.DomainModeling.Commands;
using Rafy.DomainModeling.Controls;
using Rafy.DomainModeling.Models;

namespace Rafy.DomainModeling
{
    public static class ModelingDesignerExtension
    {
        /// <summary>
        /// 让设计器加载指定的文档对象
        /// </summary>
        /// <param name="designer"></param>
        /// <param name="document"></param>
        public static void BindDocument(this ModelingDesigner designer, ODMLDocument document)
        {
            DocumentBinder.BindDocument(designer, document);

            //绑定新的文档时，需要清空文件路径。
            designer.ClearValue(CurrentOdmlFileProperty);
        }

        ///// <summary>
        ///// 让设计器加载指定的文档对象
        ///// </summary>
        ///// <param name="designer"></param>
        ///// <param name="document"></param>
        //public static void LoadDocument(this ModelingDesigner designer, ODMLDocument document)
        //{
        //    DocumentHelper.LoadDocument(designer, document);
        //    designer.ClearValue(CurrentOdmlFileProperty);
        //}

        /// <summary>
        /// 获取目前绑定在设计器中的文档对象
        /// </summary>
        /// <param name="designer"></param>
        /// <returns></returns>
        public static ODMLDocument GetDocument(this ModelingDesigner designer)
        {
            var doc = designer.DataContext as ODMLDocument;
            if (doc == null)
            {
                doc = new ODMLDocument();
                BindDocument(designer, doc);
            }
            return doc;

            //return DocumentHelper.GetDocument(designer);
        }

        /// <summary>
        /// 加载某个 xml 文件。
        /// </summary>
        /// <param name="odmlFile"></param>
        public static void LoadDocument(this ModelingDesigner designer, string odmlFile)
        {
            var xml = File.ReadAllText(odmlFile);
            var doc = string.IsNullOrEmpty(xml) ? new ODMLDocument() : ODMLDocument.Parse(xml);

            DocumentBinder.BindDocument(designer, doc);

            SetCurrentOdmlFile(designer, odmlFile);
        }

        /// <summary>
        /// 把当前模型的内容，保存到某个指定的 xml 文件。
        /// </summary>
        /// <param name="odmlFile"></param>
        public static void SaveDocument(this ModelingDesigner designer, string odmlFile)
        {
            var doc = designer.GetDocument();
            var xml = doc.ToXml();
            File.WriteAllText(odmlFile, xml);
        }

        #region CurrentOdmlFile AttachedDependencyProperty

        private static readonly DependencyProperty CurrentOdmlFileProperty = DependencyProperty.RegisterAttached(
            "CurrentOdmlFile", typeof(string), typeof(ModelingDesignerExtension)
            );

        internal static string GetCurrentOdmlFile(ModelingDesigner element)
        {
            return (string)element.GetValue(CurrentOdmlFileProperty);
        }

        private static void SetCurrentOdmlFile(ModelingDesigner element, string value)
        {
            element.SetValue(CurrentOdmlFileProperty, value);
        }

        #endregion
    }
}