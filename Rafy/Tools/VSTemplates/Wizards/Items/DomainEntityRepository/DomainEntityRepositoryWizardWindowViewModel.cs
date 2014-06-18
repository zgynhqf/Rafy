/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140507
 * 说明：见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140507 23:43
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using EnvDTE;
using RafySDK;
using RafySDK.Templates.Wizards.Items;

namespace VSTemplates.Wizards
{
    class DomainEntityRepositoryWizardWindowViewModel : ViewModel
    {
        private string _DomainNamespace;
        public string DomainNamespace
        {
            get { return this._DomainNamespace; }
            set
            {
                if (_DomainNamespace != value)
                {
                    _DomainNamespace = value;
                    this.OnPropertyChanged("DomainNamespace");
                }
            }
        }

        private string _EntityTypeName;
        public string EntityTypeName
        {
            get { return this._EntityTypeName; }
            set
            {
                if (_EntityTypeName != value)
                {
                    _EntityTypeName = value;
                    this.OnPropertyChanged("EntityTypeName");

                    this.EntityRepositoryTypeName = value + Consts.RepositorySuffix;
                }
            }
        }

        private string _BaseTypeName;
        public string BaseTypeName
        {
            get { return this._BaseTypeName; }
            set
            {
                if (_BaseTypeName != value)
                {
                    _BaseTypeName = value;
                    this.OnPropertyChanged("BaseTypeName");
                }
            }
        }

        private string _EntityRepositoryTypeName;
        public string EntityRepositoryTypeName
        {
            get { return this._EntityRepositoryTypeName; }
            set
            {
                if (_EntityRepositoryTypeName != value)
                {
                    _EntityRepositoryTypeName = value;
                    this.OnPropertyChanged("EntityRepositoryTypeName");
                }
            }
        }

        public DTE DTE { get; set; }
    }
}
