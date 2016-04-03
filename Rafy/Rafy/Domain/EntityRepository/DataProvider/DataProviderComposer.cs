/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140508
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140508 10:19
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Domain.ORM;
using Rafy.Reflection;

namespace Rafy.Domain
{
    static class DataProviderComposer
    {
        private static IDictionary<Type, Type> _repoToDP = new Dictionary<Type, Type>();

        internal static RepositoryDataProvider CreateDataProvider(Type repositoryType)
        {
            var list = TypeHelper.GetHierarchy(repositoryType, typeof(EntityRepository));
            foreach (var type in list)
            {
                Type dpType = null;
                if (_repoToDP.TryGetValue(type, out dpType))
                {
                    var dp = Activator.CreateInstance(dpType) as RepositoryDataProvider;
                    return dp;
                }
            }

            //如果子类都没有创建相应的 dp，则直接使用默认的 dp。
            return new RdbDataProvider();
        }

        //internal static void Compose()
        //{
        //    var plugins = RafyEnvironment.GetDomainPlugins();
        //    foreach (var plugin in plugins)
        //    {
        //        var types = plugin.Assembly.GetTypes();
        //        foreach (var type in types)
        //        {
        //            Compose(type);
        //        }
        //    }
        //}

        internal static void TryAddDataProvider(Type dataProviderType)
        {
            if (dataProviderType.IsAbstract) return;
            var attri = dataProviderType.GetSingleAttribute<DataProviderForAttribute>();
            if (attri == null) return;

            if (!dataProviderType.IsSubclassOf(typeof(RepositoryDataProvider)))
            {
                throw new InvalidProgramException("只有继承自 RepositoryDataProvider 的类型才能标记 DataProviderForAttribute。");
            }

            var repoType = attri.RepositoryType;
            if (!typeof(EntityRepository).IsAssignableFrom(repoType))
            {
                throw new InvalidProgramException(string.Format(
                    "没有找到为 {0} 数据提供器标记的仓库类型：{1}。",
                    dataProviderType, repoType
                    ));
            }

            _repoToDP[repoType] = dataProviderType;
        }
    }
}