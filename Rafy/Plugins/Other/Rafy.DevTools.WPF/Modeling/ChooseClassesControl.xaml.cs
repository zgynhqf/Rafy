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
using System.ComponentModel;
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
using Rafy.EntityObjectModel;
using Rafy.MetaModel;

namespace Rafy.DevTools.Modeling
{
    public partial class ChooseClassesControl : UserControl
    {
        private EOMGroup _eom;

        public ChooseClassesControl(EOMGroup eom)
        {
            _eom = eom;

            InitializeComponent();

            this.Loaded += ChooseClassesControl_Loaded;
        }

        /// <summary>
        /// 已经选择的实体类。
        /// </summary>
        public IList<EntityType> SelectedEntities
        {
            get { return lbClasses.SelectedItems.Cast<Model>().Select(m => m.EOM).ToArray(); }
        }

        void ChooseClassesControl_Loaded(object sender, RoutedEventArgs e)
        {
            var models = _eom.EntityTypes.Select(m => new Model
            {
                Name = m.FullName,
                EOM = m
            });
            lbClasses.ItemsSource = models;
            lbClasses.Items.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
        }

        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            lbClasses.Items.Filter = o =>
            {
                return (o as Model).Name.ToLower().Contains(tbSearch.Text.ToLower());
            };
        }

        private class Model
        {
            public string Name { get; set; }

            public EntityType EOM;

            public override string ToString()
            {
                return this.Name;
            }
        }
    }
}