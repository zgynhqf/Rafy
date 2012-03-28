/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120327
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120327
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
using OEA.Module.WPF;
using OEA.Library;
using OEA.MetaModel.View;
using OEA.Module.WPF.ViewControllers;
using OEA.RBAC;

namespace RBAC
{
    public class OperationSelectionView : CustomObjectView
    {
        public OperationSelectionView(EntityViewMeta meta)
            : base(meta)
        {
            this.SetControl(new OperationSelection());
        }

        public new OperationSelection Control
        {
            get { return base.Control as OperationSelection; }
        }

        protected override void OnDataChanged()
        {
            base.OnDataChanged();

            this.Control.BindData(this.Data as OrgPositionOperationDenyList);
        }
    }
}