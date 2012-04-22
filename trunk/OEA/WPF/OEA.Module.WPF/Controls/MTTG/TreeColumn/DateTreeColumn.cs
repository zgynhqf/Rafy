/*******************************************************
 * 
 * 作者：李智
 * 创建时间：20100101
 * 说明：文件描述
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 李智 20100101
 * 
*******************************************************/

using System.Windows.Data;
using OEA.Module.WPF.Editors;

namespace OEA.Module.WPF.Controls
{
    public class DateTreeColumn : TreeColumn
    {
        protected DateTreeColumn() { }

        protected override Binding GenerateBindingFormat(string name, string stringformat)
        {
            var binding = base.GenerateBindingFormat(name, stringformat);
            if (string.IsNullOrEmpty(stringformat))
            {
                binding.StringFormat = "d";
            }
            return binding;
        }
    }
}