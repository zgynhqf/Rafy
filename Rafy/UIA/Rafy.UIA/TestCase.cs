/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111201
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111201
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;

namespace Rafy.UIA
{
    public abstract class TestCase : APIContext
    {
        internal void Run()
        {
            this.LogStarted();

            try
            {
                this.RunOverride();
                this.LogSuccessed();
            }
            catch (StopUIAException)
            {
                throw;
            }
            catch (Exception ex)
            {
                this.LogFailed(ex);
                TestContext.Current.Errors[this.GetType()] = ex;

                if (!TestContext.Current.NoAdministraotrMode) { throw; }
            }
        }

        protected abstract void RunOverride();

        private void LogStarted()
        {
            LogLine(string.Format("★★★★★开始执行测试用例：{0}★★★★★" + Environment.NewLine, this.GetType().Name));
        }

        private void LogSuccessed()
        {
            LogLine(string.Format("√√√√√测试用例：{0}执行成功。", this.GetType().Name));
        }

        private void LogFailed(Exception e)
        {
            LogLine(string.Format(@"！！！！！注意：测试用例：{0}执行失败：
{1}", this.GetType().Name, FormatException(e)));
        }

        internal static string FormatException(Exception e)
        {
            //var length = Math.Min(e.StackTrace.Length, 1000);
            //return e.Message + e.StackTrace.Substring(0, length);

            return e.Message + Environment.NewLine + e.StackTrace;
        }
    }
}
