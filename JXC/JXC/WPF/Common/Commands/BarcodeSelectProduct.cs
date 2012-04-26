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
using System.Text;
using JXC.WPF.Templates;
using OEA;
using OEA.Library;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;
using OEA.Module.WPF;
using OEA.Module.WPF.CommandAutoUI;
using OEA.Module.WPF.Controls;
using OEA.WPF.Command;
using Itenso.Windows.Input;
using System.Windows.Input;
using System.Windows;

namespace JXC.Commands
{
    [Command(Label = "条码录入", ToolTip = "手工输入后请按回车键确认", GroupType = CommandGroupType.Business,
        UIAlgorithm = typeof(GenericItemAlgorithm<BarcodeTextBoxGenerator>))]
    public class BarcodeSelectProduct : ListViewCommand
    {
        public override void Execute(ListObjectView view)
        {
            var barcode = this.TryGetCustomParams<string>(CommandCustomParams.TextBox);

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
                    var product = RF.Concreate<ProductRepository>().GetByBarcode(barcode);
                    if (product == null)
                    {
                        App.MessageBox.Show(string.Format("没有找到对应 {0} 的商品", barcode), MessageBoxImage.Error);
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
            protected override ItemControlResult CreateItemControl()
            {
                var cmd = this.CreateItemCommand();

                var textBox = this.CreateTextBox(cmd);

                //在textBox上按下回车时，执行命令。
                textBox.KeyDown += (o, e) =>
                {
                    if (e.Key == Key.Enter)
                    {
                        this.TryExecuteCommand(cmd);

                        textBox.Text = string.Empty;
                    }
                };

                return new ItemControlResult(textBox, cmd);
            }
        }
    }
}