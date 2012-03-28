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
using OEA.UIA.Utils;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UITesting;
using System.IO;
using System.Net.Mail;
using Microsoft.VisualStudio.TestTools.UITest.Extension;

namespace OEA.UIA
{
    public abstract class TestContext : APIContext
    {
        private static TestContext _current;

        public static TestContext Current
        {
            get { return _current; }
        }

        public TestContext()
        {
            _current = this;

            this.MainWindowTitle = "广联达工程造价数据管理系统PIMS2.0";
        }

        #region 主窗口

        private WpfWindow _mainWindow;

        public WpfWindow MainWindow
        {
            get
            {
                if (this._mainWindow == null)
                {
                    this._mainWindow = 窗口(TestContext.Current.MainWindowTitle);
                }

                return this._mainWindow;
            }
        }

        public WpfWindow CurrentWindow
        {
            get
            {
                var result = this.ControlContexts.Reverse().FirstOrDefault(w => w is WpfWindow) as WpfWindow;
                if (result != null) return result;

                return this.MainWindow;
            }
        }

        /// <summary>
        /// 测试对象链表，最后一个为当前测试对象，执行Object方法时会执行此对象的方法。
        /// 加上链表主要是为了处理弹出窗口又关闭后自动切换测试对象的情况
        /// </summary>
        internal Stack<WpfControl> ControlContexts = new Stack<WpfControl>();

        #endregion

        #region 属性

        public string MainWindowTitle { get; set; }

        public string[] MailRecievers { get; set; }

        /// <summary>
        /// 无人执守模式
        /// </summary>
        public bool NoAdministraotrMode { get; set; }

        internal Dictionary<Type, Exception> Errors = new Dictionary<Type, Exception>();

        #endregion

        #region RequestCancel

        private bool _requestCancel;

        internal bool NeedCancel
        {
            get { return this._requestCancel; }
        }

        public void RequestCancellation()
        {
            this._requestCancel = true;
        }

        #endregion

        private DateTime _startTime;

        public void Run()
        {
            try
            {
                this._startTime = DateTime.Now;

                this.Errors.Clear();

                //Playback.PlaybackSettings.SearchTimeout = 2000;
                //Playback.PlaybackSettings.WaitForReadyTimeout = 2000;
                //Playback.PlaybackSettings.DelayBetweenActions = 1000;
                Playback.Initialize();

                this.StartupApp();

                this.RunCases();
            }
            catch (StopUIAException)
            {
                LogLine("自动化已取消执行。");
            }
            finally
            {
                Playback.Cleanup();

                this.LogAllCompleted();

                this._requestCancel = false;
            }
        }

        /// <summary>
        /// 重写这个方法，调用 "运行测试用例" 来执行用例
        /// </summary>
        protected abstract void RunCases();

        protected abstract void StartupApp();

        protected void 运行测试用例<TTestCase>()
            where TTestCase : TestCase, new()
        {
            var testCase = new TTestCase();
            testCase.Run();

            _casesRunned.Add(typeof(TTestCase));
        }

        protected void 运行测试用例(Type testCaseType)
        {
            var testCase = Activator.CreateInstance(testCaseType) as TestCase;
            testCase.Run();

            _casesRunned.Add(testCaseType);
        }

        private List<Type> _casesRunned = new List<Type>();

        private void LogAllCompleted()
        {
            var sb = new StringBuilder();
            sb.AppendLine("本次自动化测试一共耗时：" + (DateTime.Now - this._startTime));
            if (this.Errors.Count > 0)
            {
                if (this.NoAdministraotrMode)
                {
                    foreach (var error in this.Errors)
                    {
                        sb.AppendFormat(@"用例“{0}”发生错误：
{1}
", error.Key.Name, TestCase.FormatException(error.Value));
                    }
                    var message = sb.ToString();

                    this.LogCore("测试失败，马上改掉吧", message, !this._requestCancel);
                }
            }
            else
            {
                if (!this._requestCancel)
                {
                    sb.AppendLine("*******************************************************");
                    sb.AppendLine("太棒了，已运行以下脚本，并通过自动化测试：");
                    foreach (var testCase in this._casesRunned)
                    {
                        sb.AppendLine(testCase.Name);
                    }
                    sb.AppendLine("*******************************************************");
                    var message = sb.ToString();
                    this.LogCore("测试通过", message, true);
                }
            }
        }

        private void LogCore(string title, string body, bool sendMail)
        {
            LogLine(title + Environment.NewLine + body + Environment.NewLine);

            if (sendMail && this.NoAdministraotrMode)
            {
                if (this.MailRecievers != null && this.MailRecievers.Length > 0)
                {
                    var smtp = new SmtpClient("server-ex2007.grandsoft.com.cn");

                    var mail = new MailMessage
                    {
                        From = new MailAddress("GIX4_AutoTest@grandsoft.com.cn"),
                        Subject = title,
                        SubjectEncoding = Encoding.UTF8,
                        Body = body,
                        BodyEncoding = Encoding.UTF8,
                    };

                    foreach (var reciever in this.MailRecievers)
                    {
                        mail.To.Add(reciever + "@grandsoft.com.cn");
                    }

                    smtp.Send(mail);
                }
            }
        }
    }
}