using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.MetaModel.View
{
    /// <summary>
    /// 一些内置的 web 命令
    /// </summary>
    public static class WebCommandNames
    {
        /// <summary>
        /// 用于界面配置的命令。
        /// 如果没有添加客户化配置插件，
        /// </summary>
        public static string CustomizeUI;

        public static readonly string Add = "Rafy.cmd.Add";
        public static readonly string Save = "Rafy.cmd.Save";
        public static readonly string Refresh = "Rafy.cmd.Refresh";
        public static readonly string Delete = "Rafy.cmd.Delete";
        public static readonly string Edit = "Rafy.cmd.Edit";

        public static readonly string Insert = "Rafy.cmd.Insert";
        public static readonly string ExpandAll = "Rafy.cmd.ExpandAll";
        public static readonly string CollaseAll = "Rafy.cmd.CollaseAll";

        public static readonly string LookupSelectAdd = "Rafy.cmd.LookupSelectAdd";


        /*********************** 代码块解释 *********************************
         * 
         * 可以这两个集合中的数据以达到修改整个应用程序的效果。
         * 
        **********************************************************************/
        public static readonly List<string> SysCommands = new List<string>();
        public static readonly List<string> SysQueryCommands = new List<string>();
        public static readonly List<string> CommonCommands = new List<string>{
            Add, Edit, Delete, Save, Refresh
        };
        public static readonly List<string> TreeCommands = new List<string>{
            ExpandAll, CollaseAll, Add, Insert, Edit, Delete, Save, Refresh
        };
    }
}