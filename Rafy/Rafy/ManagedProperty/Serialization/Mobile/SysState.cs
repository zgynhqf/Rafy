/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111110
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111110
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Serialization.Mobile;

namespace Rafy
{
    /// <summary>
    /// 系统中 PrimitiveType 在使用 Mobile 序列化时，需要使用这个对象来进行封装。
    /// </summary>
    internal class SysState : MobileObject
    {
        public object Value;
        public string TypeName;

        protected override void OnMobileSerializeState(ISerializationContext context)
        {
            base.OnMobileSerializeState(context);

            context.AddState("v", this.Value);
            context.AddState("t", this.TypeName);
        }

        protected override void OnMobileDeserializeState(ISerializationContext context)
        {
            this.Value = context.GetState<object>("v");
            this.TypeName = context.GetState<string>("t");

            base.OnMobileDeserializeState(context);
        }
    }
}
