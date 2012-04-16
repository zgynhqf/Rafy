/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120416
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120416
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Module.WPF;
using OEA;
using OEA.MetaModel;
using JXC.WPF.Templates;

namespace JXC.WPF
{
    internal class JXCModule : IModule
    {
        public ReuseLevel ReuseLevel
        {
            get { return ReuseLevel.Main; }
        }

        public void Initialize(IClientApp app)
        {
            app.ModuleOperations += (o, e) =>
            {
                CommonModel.Modules["商品管理"].UseCustomModule<ProductModule>();
                CommonModel.Modules["采购订单"].UseCustomModule<ConditionQueryModule>();
            };
        }
    }
}