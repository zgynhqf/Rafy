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
using System.Windows.Shapes;
using Rafy.Domain;
using Rafy.RBAC.Audit;

namespace WPFClient.Shell
{
    /// <summary>
    /// 本类型用于测试 TreeGrid xaml 声明式创建。
    /// </summary>
    public partial class TreeGridTest : Window
    {
        public TreeGridTest()
        {
            InitializeComponent();

            var data = RF.Concrete<AuditItemRepository>()
                .GetAll(new Rafy.PagingInfo(1, 100));

            grid.RootItems = data;
        }
    }
}
