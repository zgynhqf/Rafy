using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;

namespace Rafy.VSPackage
{
    /// <summary>
    /// 查找某一个只写了 Repository 代码的文件。
    /// </summary>
    class RepoFileFinder : ClassFinder
    {
        protected override void VisitClassCore(CodeClass codeClass)
        {
            if (Helper.IsEntity(codeClass))
            {
                StopVisiting = true;
            }
            else if (Helper.IsRepository(codeClass))
            {
                this.Files.Add(codeClass);
                StopVisiting = true;
            }
        }
    }
}
