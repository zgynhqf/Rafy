/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130403 22:33
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130403 22:33
 * 
*******************************************************/

using System;
using System.Collections.Generic;
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
using Rafy.DomainModeling.Controls;
using Rafy.DomainModeling.Models;

namespace Rafy.DomainModeling
{
    /// <summary>
    /// 一个用于测试的控件
    /// </summary>
    public partial class RafyModelDesigner : UserControl
    {
        public RafyModelDesigner()
        {
            InitializeComponent();

            //使用双向绑定，不再需要手动调用文档加载。
            //this.IntializeByDocument();
        }

        /// <summary>
        /// 测试使用 Document 来进行初始化。
        /// </summary>
        private void IntializeByDocument()
        {
            var document = new ODMLDocument
            {
                EntityTypes =
                {
                    new EntityTypeElement("Item"){ Left = 100, Top = 100 },
                    new EntityTypeElement("Role")
                    {
                        IsAggtRoot = true,
                        Left = 300, Top = 100,
                        Properties = 
                        {
                            new PropertyElement("Id"){PropertyType = "int"},
                            new PropertyElement("Name"){PropertyType = "string", Name = "名称"},
                        }
                    },
                    new EntityTypeElement("Warehouse"){ Left = 300, Top = 300 },
                },
                EnumTypes = 
                {
                    new EnumElement("CategoryType")
                    { 
                        Left = 100, Top = 300,
                        Items = 
                        {
                            new EnumItemElement("NONE") { Label = "未登记" },
                            new EnumItemElement("REJECTED") { Label = "保留" },
                            new EnumItemElement("IDLE") { Label = "预定" },
                            new EnumItemElement("ACCEPTED") { Label = "已受理" },
                            new EnumItemElement("INSTRUCTING") { Label = "指示中" },
                            new EnumItemElement("INSTRUCTED") { Label = "已指示" },
                            new EnumItemElement("RECEIVING") { Label = "进货中" },
                            new EnumItemElement("RECEIVED") { Label = "已进货" },
                            new EnumItemElement("COMPLETED") { Label = "已认可" },
                        }
                    },
                }
            };

            designer.BindDocument(document);
        }
    }
}
