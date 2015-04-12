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
        private static string DomainEntityCodeTemplate = 
            Helper.GetResourceContent("RafySDK.Templates.Items.DomainEntity.DomainEntity.cs");

        private static string DomainEntityAutoCodeTemplate = 
            Helper.GetResourceContent("RafySDK.Templates.Items.DomainEntity.DomainEntity.g.cs");

        private static string DomainEntityRepositoryCodeTemplate = 
            Helper.GetResourceContent("RafySDK.Templates.Items.DomainEntityRepository.DomainEntityRepository.cs");

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
        public static string GetEntityFileCode(EntityFileCodeParamters p)
        {
            if (p.renderRepository)
            {
                p.repositoryCode = GetRepositoryCodeInEntityFile(
                    p.domainEntityName, p.domainEntityLabel, p.domainBaseEntityName
                    );
            }

            p.entityAttributes = p.isRootEntity ? "[RootEntity, Serializable]" : "[ChildEntity, Serializable]";

            var result = DomainEntityCodeTemplate;
            foreach (var property in typeof(EntityFileCodeReplacements).GetFields())
            {
                var search = '$' + property.Name + '$';
                var replace = property.GetValue(p) as string;
                result = result.Replace(search, replace);
            }
            return result;
        }

        public abstract class EntityFileCodeReplacements
        {
            public string entityAttributes;
            public string domainNamespace;
            public string domainEntityLabel;
            public string domainEntityName;
            public string domainBaseEntityName;
            public string refProperties = string.Empty;
            public string normalProperties = string.Empty;
            public string tableConfig = string.Empty;
            public string columnConfig = string.Empty;
            public string viewConfiguration = string.Empty;
            public string repositoryCode = string.Empty;
        }

        public class EntityFileCodeParamters : EntityFileCodeReplacements
        {
            public bool isRootEntity = true;
            public bool renderRepository = false;
        }

        public static string GetNormalPropertyCode(
            string domainEntityName, string propertyType, string propertyName
            )
        {
            string codeSnippet = @"
        public static readonly Property<$PropertyType$> $PropertyName$Property = P<$ClassName$>.Register(e => e.$PropertyName$);
        public $PropertyType$ $PropertyName$
        {
            get { return this.GetProperty($PropertyName$Property); }
            set { this.SetProperty($PropertyName$Property, value); }
        }
";

            var propertyCode = codeSnippet.Replace("$ClassName$", domainEntityName)
                .Replace("$PropertyName$", propertyName)
                .Replace("$PropertyType$", propertyType);

            return propertyCode;
        }

        public static string GetRefPropertyCode(
            string domainEntityName, string refEntityName, string refPropertyName,
            string keyType = "int",
            bool isRequired = true, bool isParent = false
            )
        {
            if (!isRequired && keyType == "string") { isRequired = true; }

            string template = null;
            if (isRequired)
            {
                template = @"
        public static readonly IRefIdProperty $RefPropertyName$IdProperty =
            P<$ClassName$>.RegisterRefId(e => e.$RefPropertyName$Id, ReferenceType.$ReferenceType$);
        public $Key$ $RefPropertyName$Id
        {
            get { return ($Key$)this.GetRefId($RefPropertyName$IdProperty); }
            set { this.SetRefId($RefPropertyName$IdProperty, value); }
        }
        public static readonly RefEntityProperty<$RefEntityType$> $RefPropertyName$Property =
            P<$ClassName$>.RegisterRef(e => e.$RefPropertyName$, $RefPropertyName$IdProperty);
        public $RefEntityType$ $RefPropertyName$
        {
            get { return this.GetRefEntity($RefPropertyName$Property); }
            set { this.SetRefEntity($RefPropertyName$Property, value); }
        }
";
            }
            else
            {
                template = @"
        public static readonly IRefIdProperty $RefPropertyName$IdProperty =
            P<$ClassName$>.RegisterRefId(e => e.$RefPropertyName$Id, ReferenceType.$ReferenceType$);
        public $Key$? $RefPropertyName$Id
        {
            get { return ($Key$?)this.GetRefNullableId($RefPropertyName$IdProperty); }
            set { this.SetRefNullableId($RefPropertyName$IdProperty, value); }
        }
        public static readonly RefEntityProperty<$RefEntityType$> $RefPropertyName$Property =
            P<$ClassName$>.RegisterRef(e => e.$RefPropertyName$, $RefPropertyName$IdProperty);
        public $RefEntityType$ $RefPropertyName$
        {
            get { return this.GetRefEntity($RefPropertyName$Property); }
            set { this.SetRefEntity($RefPropertyName$Property, value); }
        }
";
            }

            var propertyCode = template.Replace("$ClassName$", domainEntityName)
                .Replace("$RefEntityType$", refEntityName)
                .Replace("$RefPropertyName$", refPropertyName)
                .Replace("$Key$", keyType)
                .Replace("$ReferenceType$", isParent ? "Parent" : "Normal");

            return propertyCode;
        }

        public static string GetChildrenPropertyCode(
            string domainEntityName, string childEntity
            )
        {
            var template = @"

        public static readonly ListProperty<$domainEntityName$List> $domainEntityName$ListProperty = P<$parentEntityName$>.RegisterList(e => e.$domainEntityName$List);
        public $domainEntityName$List $domainEntityName$List
        {
            get { return this.GetLazyList($domainEntityName$ListProperty); }
        }";

            var childrenPropertyCode = template.Replace("$domainEntityName$", childEntity)
                .Replace("$parentEntityName$", domainEntityName);

            return childrenPropertyCode;
        }

        public static string GetRepositoryCodeInEntityFile(string domainEntityName, string domainEntityLabel, string domainBaseEntityName)
        {
            return @"

    /// <summary>
    /// $domainEntityLabel$ 仓库类。
    /// 负责 $domainEntityLabel$ 类的查询、保存。
    /// </summary>
    public partial class $domainEntityName$Repository : $domainBaseEntityName$Repository
    {
        /// <summary>
        /// 单例模式，外界不可以直接构造本对象。
        /// </summary>
        protected $domainEntityName$Repository() { }
    }"
                .Replace("$domainEntityLabel$", domainEntityLabel)
                .Replace("$domainEntityName$", domainEntityName)
                .Replace("$domainBaseEntityName$", domainBaseEntityName);
        }

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
        public static string GetEntityFileAutoCode(
            string domainNamespace, string concreteNew, string entity,
            bool renderRepository
            )
        {
            var repositoryAutoCode = string.Empty;
            if (renderRepository)
            {
                repositoryAutoCode = GetRepositoryFileCoreAutoCode(entity);
            }

            return DomainEntityAutoCodeTemplate
                .Replace("$domainNamespace$", domainNamespace)
                .Replace("$concreteNew$", concreteNew)
                .Replace("$domainEntityName$", entity)
                .Replace("$repositoryAutoCode$", repositoryAutoCode);

            //_template = _template.Replace("$time$", DateTime.Now.ToString());
        }

        public static string GetRepositoryFileCode(
            string domainNamespace, string repositoryNamespace, string domainEntityName, string baseEntity
            )
        {
            return DomainEntityRepositoryCodeTemplate
                .Replace("$domainNamespace$", domainNamespace)
                .Replace("$rootnamespace$", repositoryNamespace)
                .Replace("$domainEntityName$", domainEntityName)
                .Replace("$baseRepositoryName$", baseEntity + "Repository");
        }

        public static string GetRepositoryFileAutoCode(
            string domainNamespace, string rootnamespace, string entity
            )
        {
            var repositoryAutoCode = GetRepositoryFileCoreAutoCode(entity);

            return DomainEntityRepositoryAutoCodeTemplate
                .Replace("$domainNamespace$", domainNamespace)
                .Replace("$rootnamespace$", rootnamespace)
                .Replace("$repositoryAutoCode$", repositoryAutoCode);
        }

        /// <summary>
        /// 获取仓库自动代码文件中的核心代码。
        /// 该代码同样可以插入到实体文件的自动代码文件中。
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static string GetRepositoryFileCoreAutoCode(string entity)
        {
            return DomainEntityRepositoryAutoCodeTemplateCore.Replace("$domainEntityName$", entity);
        }

        public static string GetViewConfigurationCode(string domainEntityName, string domainEntityLabel)
        {
            var template = @"

        protected override void ConfigView()
        {
            View.DomainName(""$domainEntityLabel$"").HasDelegate($domainEntityName$.NameProperty);

            using (View.OrderProperties())
            {
                //View.Property($domainEntityName$.NameProperty).HasLabel(""名称"").ShowIn(ShowInWhere.All);
            }
        }";
            var viewConfiguration = template
                .Replace("$domainEntityLabel$", domainEntityLabel)
                .Replace("$domainEntityName$", domainEntityName);

            return viewConfiguration;
        }
    }
}
