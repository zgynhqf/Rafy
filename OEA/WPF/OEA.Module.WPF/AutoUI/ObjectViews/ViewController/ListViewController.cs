/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110215
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100215
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SimpleCsla.Wpf;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using OEA.Module.WPF.Controls;
using Itenso.Windows.Input;
using SimpleCsla;
using SimpleCsla.Core;
using OEA.Module;
using OEA.MetaModel;
using OEA.MetaModel.View;

using System.Windows.Controls.Primitives;
using AvalonDock;
using OEA.WPF.Command;
using OEA.Library;

namespace OEA.Module.WPF.ViewControllers
{
    public class ListViewController : ViewDataLoaderBase
    {
        public ListViewController(WPFObjectView view) : base(view) { }

        protected override Type GetQueryType()
        {
            var entityType = base.View.EntityType;
            //return EntityConvention.ListType(entityType); 

            //从泛型类继承的不支持Repository模式
            if (entityType.IsGenericType || entityType.BaseType.IsGenericType)
            {
                return EntityConvention.ListType(entityType);
            }
            else
            {
                return typeof(RepositoryInvoker<>).MakeGenericType(entityType);
            }
        }

        protected override string FactoryMethod
        {
            get { return MethodConvention.GetList; }
        }
    }
}
