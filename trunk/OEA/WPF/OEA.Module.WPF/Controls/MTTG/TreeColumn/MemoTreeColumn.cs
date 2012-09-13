/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110217
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100217
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;


namespace OEA.Module.WPF.Controls
{
    public class MemoTreeColumn : TreeColumn
    {
        protected MemoTreeColumn() { }

        /// <summary>
        /// 强制进入编辑状态，生成编辑状态下的按钮用于弹出窗口
        /// </summary>
        protected override bool ForceEditing
        {
            get { return true; }
        }
    }
}