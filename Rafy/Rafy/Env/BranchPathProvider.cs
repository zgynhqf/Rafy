/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20100908
 * 说明：客户化路径的计算方案提供器
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100908
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Rafy.Utils;

namespace Rafy.MetaModel
{
    /// <summary>
    /// 管理所有分支版本的路径及优先级。
    /// </summary>
    public class BranchPathProvider
    {
        /// <summary>
        /// 各分支版本存放的文件夹名称。
        /// </summary>
        public const string BranchFilesRootDir = "BranchFiles";

        /// <summary>
        /// 主干版本的文件夹名。
        /// </summary>
        public const string CommonAppName = "Common";

        /// <summary>
        /// 所有的应用列表。
        /// 第一个是Common
        /// </summary>
        private List<string> _appNames;

        /// <summary>
        /// 所有客户文件夹所在的父文件夹路径
        /// </summary>
        private string _rootPath;

        internal BranchPathProvider()
        {
            this._appNames = new List<string>();
            this._appNames.Add(CommonAppName);
            _rootPath = RafyEnvironment.MapAbsolutePath(BranchFilesRootDir);
        }

        /// <summary>
        /// 返回当前是否已经有分支版本进行了客户化。
        /// </summary>
        public bool HasBranch
        {
            get { return this._appNames.Count > 1; }
        }

        /// <summary>
        /// 按照优先级调用此方法添加分支版本的路径。
        /// </summary>
        /// <param name="branchAppName"></param>
        internal void AddBranch(string branchAppName)
        {
            if (string.IsNullOrWhiteSpace(branchAppName)) throw new ArgumentNullException("branchAppName");
            if (branchAppName == CommonAppName) throw new ArgumentException("branchAppName 不能是" + CommonAppName);

            if (!this._appNames.Contains(branchAppName))
            {
                //所有新加的文件都比Common的优先级高。全部插入到 Common 之前
                this._appNames.Insert(this._appNames.Count - 1, branchAppName);
            }
        }

        /// <summary>
        /// 根据提供的版本文件（夹）路径，按优先级返回所有版本对应的文件名。
        /// </summary>
        /// <param name="versionPath"></param>
        /// <param name="toAbsolute"></param>
        /// <returns></returns>
        public string[] MapAllPathes(string versionPath, bool toAbsolute)
        {
            if (string.IsNullOrEmpty(versionPath)) throw new ArgumentNullException("relativePath");

            versionPath = versionPath.Replace('/', '\\');

            var result = this._appNames.Select(n => Path.Combine(_rootPath, n, versionPath));

            if (!toAbsolute) { result = result.Select(p => RafyEnvironment.MapRelativePath(p)); }

            return result.ToArray();
        }

        /// <summary>
        /// 找到客户化相对路径所对应的文件，按照版本的优先级寻找。
        /// 
        /// 从一个相对于版本目录的地址，获取到确切的文件相对地址，如：
        /// Images/1.jpg
        /// =>
        /// Files/DongFang/Images/1.jpg
        /// 其中，分支版本名是不确定的。
        /// </summary>
        /// <param name="versionPath">与分支版本无关的文件路径。</param>
        /// <returns></returns>
        public string GetCustomerFile(string versionPath)
        {
            var res = FindCustomerFile(versionPath);

            if (res == null) { throw new ArgumentException("不存在这个文件或文件夹：" + versionPath); }

            return res;
        }

        /// <summary>
        /// 找到客户化相对路径所对应的文件，按照版本的优先级寻找。
        /// 
        /// 从一个相对于版本目录的地址，获取到确切的文件相对地址，如：
        /// Images/1.jpg
        /// =>
        /// Files/DongFang/Images/1.jpg
        /// 其中，分支版本名是不确定的。
        /// </summary>
        /// <param name="versionPath">与分支版本无关的文件路径。</param>
        /// <returns></returns>
        public string FindCustomerFile(string versionPath)
        {
            if (string.IsNullOrEmpty(versionPath)) throw new ArgumentNullException("relativePath");

            versionPath = versionPath.Replace('/', '\\');

            for (int i = 0, c = this._appNames.Count; i < c; i++)
            {
                var app = this._appNames[i];
                var path = Path.Combine(_rootPath, app, versionPath);
                if (File.Exists(path)) return path;
            }

            for (int i = 0, c = this._appNames.Count; i < c; i++)
            {
                var app = this._appNames[i];
                var path = Path.Combine(_rootPath, app, versionPath);
                if (Directory.Exists(path)) return path;
            }

            return null;
        }
    }
}