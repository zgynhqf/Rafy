/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120606 11:49
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120606 11:49
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.DocBuilder.Property;
using OEA.MetaModel;

namespace OEA.DocBuilder
{
    class DocBuilderPlugin : LibraryPlugin
    {
        public override ReuseLevel ReuseLevel
        {
            get { return ReuseLevel._System; }
        }

        public override void Initialize(IApp app)
        {
            var module = CommonModel.Modules.AddRoot(new ModuleMeta
            {
                Label = "OEA文档生成",
                Children =
                {
                    new ModuleMeta{ Label = "生成属性文档", CustomUI = typeof(PropertyDocBuilderUI).AssemblyQualifiedName},
                }
            });
        }
    }
}