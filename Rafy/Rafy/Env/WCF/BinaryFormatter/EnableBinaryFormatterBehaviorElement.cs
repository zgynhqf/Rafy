/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130609
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130609 14:23
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Configuration;
using System.Text;

namespace Rafy.WCF
{
    /// <summary>
    /// 启用旧的 BinaryFormatter 来对数据进行序列化。
    /// </summary>
    public class EnableBinaryFormatterBehaviorElement : BehaviorExtensionElement
    {
        public override Type BehaviorType
        {
            get { return typeof(EnableBinaryFormatterBehavior); }
        }

        protected override object CreateBehavior()
        {
            return new EnableBinaryFormatterBehavior();
        }
    }
}
