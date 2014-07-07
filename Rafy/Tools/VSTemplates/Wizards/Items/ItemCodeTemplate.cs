/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140504
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140504 23:33
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Rafy.VSPackage;

namespace VSTemplates.Wizards
{
    public class ItemCodeTemplate
    {
        private static string DomainEntityAutoCodeTemplate = 
            Helper.GetResourceContent("RafySDK.Templates.Items.DomainEntity.DomainEntity.g.cs");

        private static string DomainEntityRepositoryAutoCodeTemplate = 
            Helper.GetResourceContent("RafySDK.Templates.Items.DomainEntityRepository.DomainEntityRepository.g.cs");

        private static string DomainEntityRepositoryAutoCodeTemplateCore = 
            Helper.GetResourceContent("RafySDK.Templates.Items.DomainEntityRepository.DomainEntityRepositoryTemplate.cs");

        /// <summary>
        /// Gets the domain entity automatic code.
        /// </summary>
        /// <param name="domainNamespace">The domain namespace.</param>
        /// <param name="concreteNew">The concrete new.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="renderRepository">
        /// 如果实体类文件中还包含了仓库的文件，则需要同时在自动代码中加入仓库的自动代码。
        /// </param>
        /// <returns></returns>
        public static string GetEntityFileCode(
            string domainNamespace, string concreteNew, string entity,
            bool renderRepository
            )
        {
            var repositoryAutoCode = string.Empty;
            if (renderRepository)
            {
                repositoryAutoCode = GetRepositoryCoreCode(entity);
            }

            return DomainEntityAutoCodeTemplate
                .Replace("$domainNamespace$", domainNamespace)
                .Replace("$concreteNew$", concreteNew)
                .Replace("$domainEntityName$", entity)
                .Replace("$repositoryAutoCode$", repositoryAutoCode);

            //_template = _template.Replace("$time$", DateTime.Now.ToString());
        }

        public static string GetRepositoryFileCode(
            string domainNamespace, string rootnamespace, string entity
            )
        {
            var repositoryAutoCode = GetRepositoryCoreCode(entity);

            return DomainEntityRepositoryAutoCodeTemplate
                .Replace("$domainNamespace$", domainNamespace)
                .Replace("$rootnamespace$", rootnamespace)
                .Replace("$repositoryAutoCode$", repositoryAutoCode);
        }

        public static string GetRepositoryCoreCode(string entity)
        {
            return DomainEntityRepositoryAutoCodeTemplateCore.Replace("$domainEntityName$", entity);
        }
    }
}
