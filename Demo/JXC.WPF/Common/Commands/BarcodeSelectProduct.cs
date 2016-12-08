/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120419
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120419
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using JXC.WPF.Templates;
using Rafy;
using Rafy.Domain;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.WPF;
using Rafy.WPF.Command.UI;
using Rafy.WPF.Controls;
using Rafy.WPF.Command;
using System.Windows.Input;
using System.Windows;

namespace JXC.Commands
{
    [Command(Label = "条码录入", ToolTip = "手工输入后请按回车键确认", GroupType = CommandGroupType.Business,
        UIAlgorithm = typeof(GenericItemAlgorithm<BarcodeTextBoxGenerator>))]
    public class BarcodeSelectProduct : ListViewCommand
    {
        public override void Execute(ListLogicalView view)
        {
            var barcode = BarcodeTextBoxGenerator.GetTextBoxParameter(this);

            if (!string.IsNullOrEmpty(barcode))
            {
                var list = view.Data;
                var item = list.Cast<ProductRefItem>().FirstOrDefault(i => i.Product.Barcode == barcode);
                if (item != null)
                {
                    item.Amount++;
                }
                else
                {
                    var product = RF.ResolveInstance<ProductRepository>().GetByBarcode(barcode);
                    if (product == null)
                    {
                        App.MessageBox.Show(string.Format("没有找到对应 {0} 的商品".Translate(), barcode), MessageBoxImage.Error);
                        return;
                    }

                    item = view.CreateNewItem().CastTo<ProductRefItem>();
                    item.Product = product;
                    item.Amount = 1;
                    list.Add(item);
                    view.RefreshControl();
                }

                view.Current = item;
            }
        }

        public class BarcodeTextBoxGenerator : ItemGenerator
        {
            protected override FrameworkElement CreateCommandUI(ClientCommand cmd)
            {
                var textBox = CreateTextBox(cmd);

                //在textBox上按下回车时，执行命令。
                textBox.KeyDown += (o, e) =>
                {
                    if (e.Key == Key.Enter)
                    {
                        cmd.TryExecute(textBox);

                        textBox.Text = string.Empty;
                    }
                };

                return textBox;
            }
        }
    }
}