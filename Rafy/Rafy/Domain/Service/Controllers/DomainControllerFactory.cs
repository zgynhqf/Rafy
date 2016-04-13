/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20150801
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20150801 14:35
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Rafy.Reflection;

namespace Rafy.Domain
{
    /// <summary>
    /// Domain Controller Factory
    /// 
    /// 实现以下功能：
    /// * DomainController 的创建。
    /// * DomainController 的覆盖。
    /// * 管理控制器之间的依赖。（在创建 DomainController 时，为其建立监听程序。）
    /// </summary>
    public abstract class DomainControllerFactory
    {
        /// <summary>
        /// 创建指定类型的控制器。
        /// </summary>
        /// <typeparam name="TController"></typeparam>
        /// <returns></returns>
        public static TController Create<TController>()
            where TController : DomainController
        {
            return Create(typeof(TController)) as TController;
        }

        /// <summary>
        /// 创建指定类型的控制器。
        /// </summary>
        /// <param name="controllerType"></param>
        /// <returns></returns>
        public static DomainController Create(Type controllerType)
        {
            InitializeIf();

            controllerType = GetOverride(controllerType);

            var instance = Activator.CreateInstance(controllerType, true) as DomainController;

            CreateDependee(instance, controllerType);

            var handler = ControllerCreated;
            if (handler != null) handler(null, new ControllerCreatedEventArgs(instance));

            return instance;
        }

        /// <summary>
        /// 控制器创建成功的事件。
        /// </summary>
        public static event EventHandler<ControllerCreatedEventArgs> ControllerCreated;

        #region IntializeIf

        private static bool _intialized;

        private static void ErrorIfIntialized()
        {
            if (_intialized) throw new InvalidProgramException();
        }

        private static void InitializeIf()
        {
            if (!_intialized)
            {
                foreach (var plugin in RafyEnvironment.AllPlugins)
                {
                    var assembly = plugin.Assembly;

                    var types = assembly.GetTypes()
                        .Where(t => t.IsSubclassOf(typeof(DomainController)))
                        .ToArray();

                    foreach (var item in types)
                    {
                        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(item.TypeHandle);
                    }
                }

                _intialized = true;
            }
        }

        #endregion

        #region Override

        /// <summary>
        /// key: parent,
        /// value: child
        /// </summary>
        private static Dictionary<Type, Type> _overriedList = new Dictionary<Type, Type>();

        /// <summary>
        /// 使用子控制器来覆盖父控制器。
        /// </summary>
        /// <typeparam name="TParent"></typeparam>
        /// <typeparam name="TChild"></typeparam>
        public static void Override<TParent, TChild>()
            where TParent : DomainController
            where TChild : TParent
        {
            ErrorIfIntialized();

            _overriedList[typeof(TParent)] = typeof(TChild);
        }

        private static Type GetOverride(Type parent)
        {
            Type result = parent;

            Type child = null;
            while (_overriedList.TryGetValue(result, out child))
            {
                result = child;
            }

            return result;
        }

        #endregion

        #region Listen

        /// <summary>
        /// key: depended
        /// value: dependee list
        /// </summary>
        private static Dictionary<Type, List<Type>> _dependency = new Dictionary<Type, List<Type>>();

        internal static void CreateDependency(Type dependee, Type depended)
        {
            ErrorIfIntialized();

            List<Type> dependeeList = null;
            lock (_dependency)
            {
                if (!_dependency.TryGetValue(depended, out dependeeList))
                {
                    dependeeList = new List<Type>();
                    _dependency.Add(depended, dependeeList);
                }

                dependeeList.Add(dependee);
            }
        }

        private static void CreateDependee(DomainController instance, Type controllerType)
        {
            var types = TypeHelper.GetHierarchy(controllerType, typeof(DomainController));
            foreach (var type in types)
            {
                List<Type> dependeeList = null;
                if (_dependency.TryGetValue(type, out dependeeList))
                {
                    foreach (var dependeeType in dependeeList)
                    {
                        var dependee = Create(dependeeType);
                        dependee.InnerDependon(instance);
                    }
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// 控制器创建成功的事件参数。
    /// </summary>
    public class ControllerCreatedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ControllerCreatedEventArgs"/> class.
        /// </summary>
        /// <param name="controller">The controller.</param>
        public ControllerCreatedEventArgs(DomainController controller)
        {
            this.Controller = controller;
        }

        /// <summary>
        /// 被创建的控制器。
        /// </summary>
        public DomainController Controller { get; private set; }
    }
}