/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20211115
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20211115 07:41
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace Rafy.DataPortal
{
    /// <summary>
    /// 需要被数据门户来管理调用的目标对象，都需要实现这个接口。
    /// </summary>
    public interface IDataPortalTarget
    {
        /// <summary>
        /// 调用目标对象，可以根据自己的状态，来确定当前是否需要在本地进行调用。
        /// </summary>
        DataPortalLocation DataPortalLocation { get; }

        /// <summary>
        /// 如果目标对象在远端反序列化时，需要使用工厂对象，则需要实现此方法。
        /// </summary>
        /// <returns></returns>
        DataPortalTargetFactoryInfo TryUseFactory();

        /// <summary>
        /// 在门户调用进入时触发。
        /// </summary>
        /// <param name="e"></param>
        void OnPortalCalling(DataPortalCallContext e);

        /// <summary>
        /// 在门户调用完成后触发。
        /// </summary>
        /// <param name="e"></param>
        void OnPortalCalled(DataPortalCallContext e);
    }

    /// <summary>
    /// 门户的目标对象对应的工厂及构造信息
    /// </summary>
    [Serializable]
    public class DataPortalTargetFactoryInfo
    {
        /// <summary>
        /// 工厂在 <see cref="DataPortalTargetFactoryRegistry"/> 中注册的名称。
        /// </summary>
        public string FactoryName { get; set; }

        /// <summary>
        /// 通过该工厂构造本目标对象时，需要使用到的所有信息。（序列化后）
        /// </summary>
        public string TargetInfo { get; set; }
    }
}