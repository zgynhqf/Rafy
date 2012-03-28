using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA.MetaModel.View
{
    /// <summary>
    /// 一些内置的 web 命令
    /// </summary>
    public static class WebCommandNames
    {
        public static readonly string CustomizeUI = "Oea.cmd.CustomizeUI";

        public static readonly string Add = "Oea.cmd.Add";
        public static readonly string Save = "Oea.cmd.Save";
        public static readonly string Refresh = "Oea.cmd.Refresh";
        public static readonly string Delete = "Oea.cmd.Delete";
        public static readonly string Edit = "Oea.cmd.Edit";

        public static readonly string Insert = "Oea.cmd.Insert";
        public static readonly string ExpandAll = "Oea.cmd.ExpandAll";
        public static readonly string CollaseAll = "Oea.cmd.CollaseAll";

        public static readonly string LookupSelectAdd = "Oea.cmd.LookupSelectAdd";


        /*********************** 代码块解释 *********************************
         * 
         * 可以这两个集合中的数据以达到修改整个应用程序的效果。
         * 
        **********************************************************************/
        public static readonly List<string> CommonCommands = new List<string>{
            Add, Edit, Delete, Save, Refresh
        };
        public static readonly List<string> TreeCommands = new List<string>{
            ExpandAll, CollaseAll, Add, Insert, Edit, Delete, Save, Refresh
        };
    }
}