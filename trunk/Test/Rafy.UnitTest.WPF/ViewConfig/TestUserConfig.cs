/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130930
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130930 14:58
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.Web;
using UT;

namespace Rafy.UnitTest.WPF.ViewConfig
{
    internal class TestUserConfig : WPFViewConfig<TestUser>
    {
        protected override void ConfigView()
        {
            base.ConfigView();

            View.Property(TestUser.NameProperty).HasLabel("姓名").ShowIn(ShowInWhere.All);
            View.Property(TestUser.AgeProperty).HasLabel("年龄").ShowIn(ShowInWhere.List | ShowInWhere.Detail);
            View.Property(TestUser.NotEmptyCodeProperty).HasLabel("编码").ShowIn(ShowInWhere.List | ShowInWhere.Detail);
            View.Property(TestUser.TasksTimeProperty).HasLabel("任务时间").ShowIn(ShowInWhere.All);
        }
    }
}