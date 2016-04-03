using System;
using System.Reflection;

namespace Rafy.Reflection
{
    internal class DynamicMethodHandle
    {
        /// <summary>
        /// 对应的方法
        /// </summary>
        public MethodInfo Method;

        /// <summary>
        /// 动态生成的代理方法
        /// </summary>
        public DynamicMethodDelegate DynamicMethod;

        /// <summary>
        /// 最后一个参数是否为一个标记了 param 标记的数组对象。
        /// </summary>
        public bool HasFinalArrayParam;

        /// <summary>
        /// 方法参数的长度
        /// </summary>
        public int MethodParamsLength;

        /// <summary>
        /// 如果 HasFinalArrayParam 为 true，那么此属性表示最后一个数组中的元素的类型。
        /// </summary>
        public Type FinalArrayElementType;

        public DynamicMethodHandle(MethodInfo method)
        {
            if (method == null) throw new ArgumentNullException("info");

            this.Method = method;
            var parameters = method.GetParameters();
            this.MethodParamsLength = parameters.Length;

            if (MethodCaller.IsLastArray(parameters))
            {
                this.HasFinalArrayParam = true;
                this.FinalArrayElementType = parameters[parameters.Length - 1].ParameterType.GetElementType();
            }
            this.DynamicMethod = DynamicMethodHandlerFactory.CreateMethod(method);
        }
    }
}
