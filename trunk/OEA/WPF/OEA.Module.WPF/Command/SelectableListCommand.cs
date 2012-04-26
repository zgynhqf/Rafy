/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110311
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100311
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using OEA.WPF.Command;
using OEA.Module.WPF.CommandAutoUI;
using Itenso.Windows.Input;
using System.Windows.Controls;

namespace OEA.WPF.Command
{
    /// <summary>
    /// 提供菜单下拉数据源的命令基类
    /// 
    /// 继承此类，并设置UIAlgorithm：
    /// <![CDATA[
    /// UIAlgorithm = typeof(GenericItemAlgorithm<DataSelectorMenuGenerator>),
    /// ]]>
    /// </summary>
    /// <typeparam name="TParamater"></typeparam>
    public abstract class DataSelectorCommand<TParamater> : ClientCommand<TParamater>, IDataSelectorCommand
        where TParamater : class
    {
        private ObservableCollection<string> _listSource = new ObservableCollection<string>();

        private string _value;

        /// <summary>
        /// 子类使用此属性定义需要选择的数据
        /// </summary>
        protected ObservableCollection<string> Data
        {
            get
            {
                return _listSource;
            }
        }

        /// <summary>
        /// 子类使用此属性来获取被选择的数据项。
        /// </summary>
        protected string SelectedValue
        {
            get
            {
                return this._value;
            }
        }

        void IDataSelectorCommand.SetSelectedItem(string value)
        {
            this._value = value;
        }

        ObservableCollection<string> IDataSelectorCommand.DataSource
        {
            get { return this._listSource; }
        }
    }

    internal interface IDataSelectorCommand
    {
        ObservableCollection<string> DataSource { get; }

        void SetSelectedItem(string value);
    }

    /// <summary>
    /// 为 DataSelectorCommand 生成菜单按钮。
    /// </summary>
    public class DataSelectorMenuGenerator : ItemGenerator
    {
        protected override ItemControlResult CreateItemControl()
        {
            var cmd = CommandRepository.NewCommand(this.CommandItem);
            var listSource = cmd.CoreCommand as IDataSelectorCommand;
            if (listSource == null) throw new InvalidProgramException("命令必须继承 DataSelectorCommand<T> 类。");

            var item = this.CreateAMenuItem(cmd);
            item.Header = this.CommandItem.Label;

            item.ItemsSource = listSource.DataSource;
            item.Click += (o, e) =>
            {
                //当点击的是 ItemsSource 绑定生成的 MenuItem 时，开始执行事件。
                if (e.OriginalSource != e.Source)
                {
                    var itemSelected = (e.OriginalSource as MenuItem).Header.ToString();
                    listSource.SetSelectedItem(itemSelected);
                    cmd.CoreCommand.TryExecute(item.CommandParameter);
                }
            };

            return new ItemControlResult(item, cmd);
        }
    }

    //使用方法：
    //[Command("B711E059-10C4-4E59-8545-7C3BA6AACDC6", typeof(OrgPosition),
    //    CommandType = OEA.Types.CommandType.Menu,
    //    //Group = "一级",
    //    UIAlgorithm = typeof(GenericItemAlgorithm<DataSelectorGenerator>),
    //    Group = "一级",
    //    Label = "测试专用按钮", Index = -1)]
    //public class xxxxxxxxxxxxxxxxxxxxxxxxx : DataSelectorCommand<ListObjectView>
    //{
    //    public xxxxxxxxxxxxxxxxxxxxxxxxx()
    //    {
    //        var list = this.DropDownListSource;
    //        for (int i = 0; i < 2; i++) list.Add(i.ToString());
    //    }

    //    public override void Execute(ListObjectView view)
    //    {
    //        for (int i = 0; i < Convert.ToInt32(this.SelectedValue); i++)
    //        {
    //            this.DropDownListSource.Add(this.DropDownListSource.Count.ToString());
    //        }
    //    }
    //}
}