/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20151209
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20151209 13:08
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.Domain
{
    /// <summary>
    /// 领域层业务逻辑控制器。
    /// 工作在 DDD 经典分层中的领域层中。
    /// 在具体的子类中编写具体业务的控制逻辑。
    /// </summary>
    public abstract class DomainController
    {
        #region 依赖管理

        internal void InnerDependon(DomainController controller)
        {
            this.OnAlwaysDependon(controller);
        }

        /// <summary>
        /// 如果使用了 Depend().On() 方法创建了监听关系，则需要重写此方法来建立确切的事件监听程序。
        /// </summary>
        /// <param name="controller"></param>
        protected virtual void OnAlwaysDependon(DomainController controller)
        {
            throw new NotImplementedException("使用了 Dependency 方法创建了监听关系，则需要重写此方法来建立确切的事件监听程序。");
        }

        /// <summary>
        /// 在子类的静态构造函数中，使用此方法来建立其它控制器的事件监听程序。
        /// <![CDATA[
        /// 使用方法：
        /// public class StockController : DomainController
        /// {
        ///     public event EventHandler StockChanged;
        /// 
        ///     protected virtual void OnStockChanged()
        ///     {
        ///         var handler = this.StockChanged;
        ///         if (handler != null) handler(this, EventArgs.Empty);
        ///     }
        /// }
        /// 
        /// public class RecieveController : DomainController
        /// {
        ///     static RecieveController()
        ///     {
        ///         Depend<RecieveController>().On<StockController>();
        ///     }
        /// 
        ///     protected override void OnAlwaysDependon(DomainController controller)
        ///     {
        ///         var sc = controller as StockController;
        ///         if (sc != null)
        ///         {
        ///             sc.StockChanged += OnStockChanged;
        ///         }
        ///     }
        /// 
        ///     private void OnStockChanged(object sender, EventArgs e)
        ///     {
        ///         //根据库存变化信息，来实现特定功能
        ///     }
        /// }
        /// ]]>
        /// </summary>
        /// <typeparam name="TController">子类的类型。</typeparam>
        /// <returns></returns>
        public static ControllerDependency Depend<TController>()
        {
            return new ControllerDependency { ControllerType = typeof(TController) };
        }

        public class ControllerDependency
        {
            internal Type ControllerType;

            internal ControllerDependency() { }

            /// <summary>
            /// 始终需要监听指定的控制器类型。
            /// </summary>
            /// <typeparam name="TListendController"></typeparam>
            public void On<TListendController>()
                where TListendController : DomainController
            {
                DomainControllerFactory.CreateDependency(ControllerType, typeof(TListendController));
            }

            /// <summary>
            /// 始终需要监听指定的控制器类型。
            /// </summary>
            /// <param name="controllerTypes"></param>
            public void On(params Type[] controllerTypes)
            {
                foreach (var item in controllerTypes)
                {
                    DomainControllerFactory.CreateDependency(ControllerType, item);
                }
            }
        }

        #endregion
    }
}
