/*******************************************************
 * 调用EXE
 * 作者：李智
 * 创建时间：20100101
 * 说明：文件描述
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 李智 20100101
 * 
*******************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace OEA.Utils
{
    public static class InvokeEXE
    {
        /// <summary>
        /// 启动其他的应用程序
        /// </summary>
        /// <param name="pathAndName">其他应用程序名称，包含路劲</param>
        public static void Invoke(string pathAndName)
        {
            string exe_path = Path.GetDirectoryName(pathAndName);
            string exeName = Path.GetFileName(pathAndName);

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = exeName;
            process.StartInfo.WorkingDirectory = exe_path;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            //if (process.HasExisted)//判断是否运行结束       
            //    process.kill();  
        }
        ///   <启动其他的应用程序>   
        ///   <param   name="path">应用程序名称,包含路径</param>          
        ///   <param   name="param">命令行参数</param>          
        public static void Invoke(string path, string param)
        {
            string exe_path = Path.GetDirectoryName(path);
            string exeName = Path.GetFileName(path);

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = exeName;
            process.StartInfo.WorkingDirectory = exe_path;
            process.StartInfo.Arguments = param;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            //if (process.HasExisted)//判断是否运行结束       
            //    process.kill();  
        }
        /// <同步调用外面的EXE>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="param"></param>
        public static void InvokeSynchronization(string path, string param)
        {
            string exe_path = Path.GetDirectoryName(path);
            string exeName = Path.GetFileName(path);

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = exeName;
            process.StartInfo.WorkingDirectory = exe_path;
            process.StartInfo.Arguments = param;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            //'等待程序装载完成
            process.WaitForInputIdle();
            //'等待进行程退出
            process.WaitForExit();
                        
            //if (process.HasExisted)//判断是否运行结束       
            //    process.kill();  
        }
    }
}