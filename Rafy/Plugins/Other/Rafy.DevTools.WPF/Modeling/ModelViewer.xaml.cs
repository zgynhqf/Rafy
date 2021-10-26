/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130403 23:34
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130403 23:34
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Rafy.EntityObjectModel;
using Rafy.MetaModel;
using Rafy.DomainModeling;
using Rafy.DomainModeling.Models;
using Rafy.WPF;
using Rafy.DomainModeling.Commands;

namespace Rafy.DevTools.Modeling
{
    public partial class ModelViewer : UserControl
    {
        public ModelViewer()
        {
            InitializeComponent();
        }

        private EOMGroup _eom;

        private void btnAddClasses_Click(object sender, RoutedEventArgs e)
        {
            if (_eom == null)
            {
                //注意：从 CommonModel 中获取的实体类，是没有继承关系的。
                CommonModel.Entities.EnsureAllLoaded();
                var converter = new MetaToEOM(CommonModel.Entities.Where(m => m.EntityCategory != EntityCategory.QueryObject));
                _eom = converter.Read();
            }

            var content = new ChooseClassesControl(_eom);

            var res = App.Windows.ShowDialog(content, w =>
            {
                w.Title = "添加实体类".Translate();
                w.Width = 400;
            });

            if (res == WindowButton.Yes)
            {
                var typeList = content.SelectedEntities;
                if (typeList.Count > 0)
                {
                    var document = modelViewer.GetDocument();
                    ODMLDocumentHelper.AddToDocument(new AddToDocumentArgs
                    {
                        Docment = document,
                        TypeList = typeList,
                        AllTypes = _eom.EntityTypes
                    });
                    modelViewer.BindDocument(document);
                }
            }
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            var document = modelViewer.GetDocument();
            foreach (var type in document.Connections)
            {
                type.Hidden = true;
            }
        }
    }
}
