/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20151022
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20151022 12:06
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.ManagedProperty;

namespace Rafy.Domain
{
    /// <summary>
    /// 此类会为所有的实体都添加一个 IsPhantom 的运行时属性。
    /// </summary>
    [CompiledPropertyDeclarer]
    public class EntityPhantomExtension
    {
        public static readonly Property<bool> IsPhantomProperty =
            P<Entity>.RegisterExtension<bool>("IsPhantom", typeof(EntityPhantomExtension));
        /// <summary>
        /// 获取当前实体是否为一个‘幽灵’（即已经删除不再使用的数据）。
        /// </summary>
        /// <param name="me"></param>
        /// <returns></returns>
        public static bool GetIsPhantom(Entity me)
        {
            return me.GetProperty(IsPhantomProperty);
        }
        /// <summary>
        /// 设置当前实体是否为一个‘幽灵’（即已经删除不再使用的数据）。
        /// </summary>
        /// <param name="me"></param>
        /// <returns></returns>
        public static void SetIsPhantom(Entity me, bool value)
        {
            me.SetProperty(IsPhantomProperty, value);
        }
    }
}