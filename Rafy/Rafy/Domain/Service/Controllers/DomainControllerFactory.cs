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
using Castle.DynamicProxy;
using Rafy.ComponentModel;
using Rafy.DataPortal;
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
    public class DomainControllerFactory : IDataPortalTargetFactory
    {
        internal const string DataPortalTargetFactoryName = "DCFty";

        private static DomainControllerFactory _default;
        public static DomainControllerFactory Default
        {
            get => _default;
            set
            {
                if(value == null) throw new ArgumentNullException(nameof(value));
                _default = value;
                DataPortalTargetFactoryRegistry.Register(value);
            }
        }

        /// <summary>
        /// 创建指定类型的控制器。
        /// </summary>
        /// <typeparam name="TController"></typeparam>
        /// <returns></returns>
        public static TController Create<TController>()
            where TController : class
        {
            return _default.Create(typeof(TController)) as TController;
        }

        /// <summary>
        /// 创建指定类型的控制器。
        /// </summary>
        /// <param name="controllerType"></param>
        /// <returns></returns>
        public DomainController Create(Type controllerType)
        {
            RafyEnvironment.LoadPlugin(controllerType.Assembly);

            this.InitializeIf();

            if (controllerType.IsInterface)
            {
                controllerType = this.GetImpl(controllerType);
            }
            else
            {
                if (!controllerType.IsSubclassOf(typeof(DomainController)))
                {
                    throw new InvalidOperationException($"无法为 {controllerType} 创建领域控制器实例，这是因为它没有从 {typeof(DomainController)} 上继承。");
                }
            }

            controllerType = this.GetOverride(controllerType);

            var instance = this.CreateInstanceProxy(controllerType);

            this.CreateDependee(instance, controllerType);

            var handler = ControllerCreated;
            if (handler != null) handler(null, new ControllerCreatedEventArgs(instance));

            return instance;
        }

        /// <summary>
        /// 控制器创建成功的事件。
        /// </summary>
        public static event EventHandler<ControllerCreatedEventArgs> ControllerCreated;

        #region DataPortal 相关、生成代理、方法拦截

        private ProxyGenerator _proxyGenerator = new ProxyGenerator();

        private DomainController CreateInstanceProxy(Type controllerType)
        {
            var options = new ProxyGenerationOptions(DataPortalCallMethodHook.Instance);

            //ProxyGenerator 内部对 Type 使用了 Cache。性能应该不是问题。
            var instance = _proxyGenerator.CreateClassProxy(controllerType, options, DataPortalCallInterceptor.Instance) as DomainController;

            return instance;
        }

        string IDataPortalTargetFactory.Name => DataPortalTargetFactoryName;

        IDataPortalTarget IDataPortalTargetFactory.GetTarget(DataPortalTargetFactoryInfo info)
        {
            return ControllerSerializer.Instance.Deserialize(info.TargetInfo, this.Create);
        }

        #endregion

        #region IntializeIf

        private bool _intialized;

        private IList<Type> _controllers = new List<Type>();
        private Dictionary<Type, Type> _interfaceToImpl = new Dictionary<Type, Type>();

        private Type GetImpl(Type interfaceType)
        {
            if (_interfaceToImpl.TryGetValue(interfaceType, out var value))
            {
                return value;
            }
            if (!typeof(IDomainControllerContract).IsAssignableFrom(interfaceType))
            {
                throw new InvalidOperationException($"接口类型 {interfaceType} 未继承 {typeof(IDomainControllerContract)}，无法创建其对应的领域控制器。");
            }
            throw new InvalidOperationException($"没有找到实现接口类型 {interfaceType} 的具体 DomainController 类型。如果开发者已经正确编写实现类型，那么可能是这个类型所对应的领域插件还没有正确加载");
        }

        private void ErrorIfIntialized()
        {
            if (_intialized) throw new InvalidProgramException();
        }

        private void InitializeIf()
        {
            if (!_intialized)
            {
                lock (this)
                {
                    if (!_intialized)
                    {
                        RafyEnvironment.HandleAllPlugins(this.Initialize);

                        _intialized = true;
                    }
                }
            }
        }

        private void Initialize(IPlugin plugin)
        {
            var assembly = plugin.Assembly;

            var types = assembly.GetTypes()
                .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(DomainController)))
                .ToArray();

            foreach (var controllerType in types)
            {
                this.Initialize(controllerType);
            }
        }

        private void Initialize(Type controllerType)
        {
            if (_controllers.Contains(controllerType)) return;
            _controllers.Add(controllerType);

            //先处理基类。
            var type = controllerType.BaseType;
            while (type != typeof(DomainController))
            {
                Initialize(type);
                type = type.BaseType;
            }

            //调用该类的静态构造函数。其中会进行建立控制器之间的依赖项。
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(controllerType.TypeHandle);

            //接下来，处理所有接口类型的领域控制器契约。
            var contractType = typeof(IDomainControllerContract);
            var interfaces = controllerType.GetInterfaces();
            foreach (var interfaceType in interfaces)
            {
                var isContract = contractType.IsAssignableFrom(interfaceType);
                if (isContract)
                {
                    if (_interfaceToImpl.TryGetValue(interfaceType, out var existType))
                    {
                        if (!contractType.IsSubclassOf(existType))
                        {
                            throw new InvalidOperationException($"{controllerType} 不可以实现契约 {interfaceType}。这是因为已经有注册了该契约的实现类型： {existType}，且目前只支持为契约注册唯一的实现。");
                        }
                        else
                        {
                            //如果是子类，则不做任何处理。由开发者调用 Override 将父类完全覆盖掉。
                        }
                    }
                    else
                    {
                        _interfaceToImpl.Add(interfaceType, controllerType);
                    }
                }
            }
        }

        #endregion

        #region Override

        /// <summary>
        /// key: parent,
        /// value: child
        /// </summary>
        private Dictionary<Type, Type> _overriedList = new Dictionary<Type, Type>();

        /// <summary>
        /// 使用子控制器来覆盖父控制器。
        /// </summary>
        /// <typeparam name="TParent"></typeparam>
        /// <typeparam name="TChild"></typeparam>
        public void Override<TParent, TChild>()
            where TParent : DomainController
            where TChild : TParent
        {
            this.ErrorIfIntialized();

            _overriedList[typeof(TParent)] = typeof(TChild);
        }

        private Type GetOverride(Type parent)
        {
            Type result = parent;

            while (_overriedList.TryGetValue(result, out var child))
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
        private Dictionary<Type, List<Type>> _dependency = new Dictionary<Type, List<Type>>();

        internal void CreateDependency(Type dependee, Type depended)
        {
            this.ErrorIfIntialized();

            lock (_dependency)
            {
                if (!_dependency.TryGetValue(depended, out var dependeeList))
                {
                    dependeeList = new List<Type>();
                    _dependency.Add(depended, dependeeList);
                }

                dependeeList.Add(dependee);
            }
        }

        private void CreateDependee(DomainController instance, Type controllerType)
        {
            var types = TypeHelper.GetHierarchy(controllerType, typeof(DomainController));
            foreach (var type in types)
            {
                if (_dependency.TryGetValue(type, out var dependeeList))
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

    /// <summary>
    /// <see cref="DomainControllerFactory"/> 的缩写。
    /// </summary>
    public abstract class DCF : DomainControllerFactory { }
}