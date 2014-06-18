/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130414 20:15
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130414 20:15
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using Rafy.EntityObjectModel;
using Rafy.VSPackage.Modeling.CodeSync;

namespace Rafy.VSPackage.Modeling
{
    /// <summary>
    /// 当前项目的实体关系缓存。
    /// </summary>
    public static class ProjectEOM
    {
        private static Project _project;
        private static EOMGroup _eom;

        public static EOMGroup Get(Project project)
        {
            if (_project != project)
            {
                _project = project;
                _eom = null;
            }

            if (_eom == null)
            {
                if (_project != null)
                {
                    var visitor = new EOMReader();
                    visitor.ReadProject(project);
                    _eom = visitor.GetResult();
                }
                else
                {
                    _eom = new EOMGroup();
                }
            }

            return _eom;
        }

        public static void Reset()
        {
            _eom = null;
        }
    }
}
