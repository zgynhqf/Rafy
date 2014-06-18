/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130412 09:21
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130412 09:21
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.DomainModeling.Controls;
using Rafy.DomainModeling;
using Rafy.DomainModeling.Models;

namespace Rafy.VSPackage.Modeling
{
    /// <summary>
    /// 编辑器的属性。
    /// </summary>
    internal class EditorProperties
    {
        private EditorPane _editor;
        private ModelingDesigner _designer;
        private ODMLDocument _document
        {
            get { return _designer.GetDocument(); }
        }

        public EditorProperties(EditorPane editor)
        {
            _editor = editor;
            _designer = editor.Designer;
        }

        [DisplayName("文件名")]
        public string FileName
        {
            get { return _editor.FileName; }
        }

        [DisplayName("显示隐藏连接")]
        public bool ShowHiddenConnections
        {
            get { return _designer.ShowHiddenRelations; }
            set { _designer.ShowHiddenRelations = value; }
        }

        [DisplayName("隐藏同名连接标题")]
        public bool HideNonsenceLabels
        {
            get { return _document.HideNonsenceLabels; }
            set { _document.HideNonsenceLabels = value; }
        }
    }
}