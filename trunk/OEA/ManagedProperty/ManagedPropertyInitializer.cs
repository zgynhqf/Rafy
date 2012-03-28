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
using System.Reflection;

namespace OEA.ManagedProperty
{
    /// <summary>
    /// 所有程序集中的托管属性初始化器
    /// OEA Used Only!
    /// </summary>
    public static class ManagedPropertyInitializer
    {
        /// <summary>
        /// 初始化操作
        /// </summary>
        /// <param name="assemblies"></param>
        public static void Initialize(IEnumerable<Assembly> assemblies)
        {
            var entityTypes = SearchAllDeclaringTypes(assemblies);
            foreach (var type in entityTypes)
            {
                //泛型类在绑定具体类型前，是无法初始化它的静态字段的，所以这里直接退出，而留待子类来进行初始化。
                if (type.ContainsGenericParameters)
                {
                    if (!type.IsAbstract)
                    {
                        throw new InvalidOperationException(string.Format(
                            "继承自 ManagedPropertyObject 的泛型类型 {0}，必须声明为 abstract，否则无法正常使用托管属性！",
                            type.FullName
                            ));
                    }
                    continue;
                }

                ForceInitStaticFieldsRecur(type);

                //有些实体类型没有任何一个托管属性，这会导致它没有对应的 Container。
                //所以添加下面一行以保证这些托管属性
                ManagedPropertyRepository.Instance.GetOrCreateTypeProperties(type);
            }

            ManagedPropertyRepository.Instance.NotifyCompilePropertiesCompleted();
        }

        /// <summary>
        /// 获取所有 Entity 的子类。
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<Type> SearchAllDeclaringTypes(IEnumerable<Assembly> assemblies)
        {
            var baseType = typeof(IManagedPropertyDeclarer);
            foreach (var assembly in assemblies)
            {
                var configTypes = assembly.GetTypes()
                    .Where(t => baseType.IsAssignableFrom(t));

                foreach (var configType in configTypes) { yield return configType; }
            }
        }

        private static void ForceInitStaticFields(Type type)
        {
            var attr =
                System.Reflection.BindingFlags.Static |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.DeclaredOnly |
                System.Reflection.BindingFlags.NonPublic;
            var t = type;
            var fields = t.GetFields(attr);
            if (fields.Length > 0)
            {
                //只需要获取一个，即可强制整个静态构造函数运行。
                fields[0].GetValue(null);
            }
        }

        private static void ForceInitStaticFieldsRecur(Type type)
        {
            var attr =
                System.Reflection.BindingFlags.Static |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.DeclaredOnly |
                System.Reflection.BindingFlags.NonPublic;
            var t = type;
            while (t != null)
            {
                var fields = t.GetFields(attr);
                if (fields.Length > 0)
                    fields[0].GetValue(null);
                t = t.BaseType;
            }
        }
    }
}
