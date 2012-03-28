using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace OEA.Library
{
    /// <summary>
    /// 获取服务端时间的类
    /// </summary>
    public static class ServerTime
    {
        public static DateTime Get()
        {
            var cmd = new ServerTimeCommand();
            cmd = SimpleCsla.DataPortal.Execute<ServerTimeCommand>(cmd);
            return cmd.Result;
        }
    }

    /// <summary>
    /// 获取服务端时间的命令
    /// </summary>
    [Serializable]
    public class ServerTimeCommand : SimpleCsla.ServiceBase
    {
        public DateTime Result { get; set; }

        protected override void DataPortal_Execute()
        {
            this.Result = DateTime.Now;
        }
    }
}