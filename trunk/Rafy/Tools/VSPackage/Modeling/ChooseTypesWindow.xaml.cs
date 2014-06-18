/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130409 19:17
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130409 19:17
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Rafy.EntityObjectModel;

namespace Rafy.VSPackage.Modeling
{
    public partial class ChooseTypesWindow : Window
    {
        private EOMGroup _eom;

        internal ChooseTypesWindow(EOMGroup eom)
        {
            InitializeComponent();

            _eom = eom;

            this.Loaded += ChooseClassesControl_Loaded;
        }

        void ChooseClassesControl_Loaded(object sender, RoutedEventArgs e)
        {
            var models = _eom.EntityTypes.Select(m => new Model
            {
                Name = m.FullName,
                EntityType = m
            });

            lbClasses.ItemsSource = models;
            lbClasses.Items.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
        }

        /// <summary>
        /// 已经选择的实体类。
        /// </summary>
        public IList<EntityType> SelectedEntities
        {
            get { return lbClasses.SelectedItems.Cast<Model>().Select(m => m.EntityType).ToArray(); }
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
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

            public EntityType EntityType;

            public override string ToString()
            {
                return this.Name;
            }
        }
    }
}
