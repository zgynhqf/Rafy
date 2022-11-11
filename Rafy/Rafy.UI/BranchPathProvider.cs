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
        public const string CommonBranchName = "Common";

        /// <summary>
        /// 所有的分支列表。
        /// 第一个是Common
        /// </summary>
        private List<string> _brancNames;

        /// <summary>
        /// 所有应用对应的根目录。
        /// </summary>
        private Dictionary<string, string> _branchDirs;

        /// <summary>
        /// 各版本文件夹所在的根文件夹路径
        /// </summary>
        private string _defaultRootPath;

        internal BranchPathProvider()
        {
            _brancNames = new List<string> { CommonBranchName };
            _branchDirs = new Dictionary<string, string>();
            _defaultRootPath = RafyEnvironment.MapAbsolutePath(BranchFilesRootDir);
        }

        /// <summary>
        /// 当前正在使用的分支名称。
        /// </summary>
        public string ActiveBranch { get; set; } = CommonBranchName;

        /// <summary>
        /// 按照优先级调用此方法添加分支版本的路径。
        /// </summary>
        /// <param name="branchAppName"></param>
        /// <param name="branchRootDir"></param>
        public void AddBranch(string branchAppName, string branchRootDir = null)
        {
            if (string.IsNullOrWhiteSpace(branchAppName)) throw new ArgumentNullException("branchAppName");
            if (branchAppName == CommonBranchName) throw new ArgumentException("branchAppName 不能是" + CommonBranchName);

            if (!_brancNames.Contains(branchAppName))
            {
                _brancNames.Add(branchAppName);

                if (branchRootDir != null)
                {
                    _branchDirs[branchAppName] = branchRootDir;
                }
            }
        }

        /// <summary>
        /// 根据提供的版本文件（夹）路径，按版本优先级返回所有版本对应的文件名。
        /// </summary>
        /// <param name="versionPath"></param>
        /// <param name="toAbsolute"></param>
        /// <param name="destination">需要获取到哪一个部分的分支列表。</param>
        /// <returns></returns>
        public string[] MapAllBranchPathes(string versionPath, bool toAbsolute, BranchDestination destination = BranchDestination.ActiveBranch)
        {
            if (string.IsNullOrEmpty(versionPath)) throw new ArgumentNullException("relativePath");

            versionPath = versionPath.Replace('/', '\\');

            var result = this.GetBranches(destination).Select(app => CombineAppPath(app, versionPath));

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

            //先找文件
            foreach (var app in this.GetBranches())
            {
                var path = CombineAppPath(app, versionPath);
                if (File.Exists(path)) return path;
            }

            //再找文件夹
            foreach (var app in this.GetBranches())
            {
                var path = CombineAppPath(app, versionPath);
                if (Directory.Exists(path)) return path;
            }

            return null;
        }

        /// <summary>
        /// 获取正在使用的分支列表名称。
        /// </summary>
        /// <param name="destination">需要获取到哪一个部分的分支列表。</param>
        /// <returns></returns>
        public IEnumerable<string> GetBranches(BranchDestination destination = BranchDestination.ActiveBranch)
        {
            if (destination != BranchDestination.Empty)
            {
                for (int i = 0, c = _brancNames.Count; i < c; i++)
                {
                    var app = _brancNames[i];

                    //如果已经激活到需要的版本，则后续的版本都不再使用。
                    if (app == this.ActiveBranch)
                    {
                        if (destination == BranchDestination.BeforeActiveBranch) break;

                        yield return app;

                        if (destination == BranchDestination.ActiveBranch) break;
                    }
                    else
                    {
                        yield return app;
                    }
                }
            }
        }

        private string CombineAppPath(string branchName, string versionPath)
        {
            _branchDirs.TryGetValue(branchName, out var appDir);
            if (appDir == null) { appDir = _defaultRootPath; }

            var result = Path.Combine(appDir, branchName, versionPath);

            return result;
        }
    }

    /// <summary>
    /// 视图模型在进行块配置时，需要配置到哪一步的分支版本。
    /// </summary>
    public enum BranchDestination
    {
        /// <summary>
        /// 不需要任何分支配置。（获取到的只有代码配置）
        /// </summary>
        Empty,

        /// <summary>
        /// 配置到当前激活版本的前一个版本。
        /// </summary>
        BeforeActiveBranch,

        /// <summary>
        /// 配置到到当前激活版本。
        /// </summary>
        ActiveBranch
    }
}