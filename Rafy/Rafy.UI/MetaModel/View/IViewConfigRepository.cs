/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20210808
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20210808 19:12
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace Rafy.MetaModel
{
    public interface IViewConfigRepository
    {
        /// <summary>
        /// 获取某个实体视图的所有配置类实例
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="extendView">如果想获取扩展视图列表，则需要传入指定的扩展视图列表</param>
        /// <returns></returns>
        IEnumerable<ViewConfig> FindViewConfigurations(Type entityType, string extendView = null);
    }
}