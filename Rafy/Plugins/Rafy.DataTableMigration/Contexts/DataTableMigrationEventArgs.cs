/*******************************************************
 * 
 * 作者：吴中坡
 * 创建日期：20170315
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 吴中坡 20170315 13:48
 * 
*******************************************************/

using System;

namespace Rafy.DataTableMigration.Contexts
{
    /// <summary>
    /// 为数据迁移状态的变化提供数据。
    /// </summary>
    public class DataTableMigrationEventArgs : EventArgs
    {
        public DataTableMigrationEventArgs(string message, decimal currentProcess = 0M)
        {
            if (currentProcess < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(currentProcess));
            }

            this.Message = message;
            this.CurrentProcess = currentProcess;
        }

        /// <summary>
        /// 获取一段文本描述信息。
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// 当前数据迁移进度，值区间：[1, 100]。
        /// </summary>
        public decimal CurrentProcess { get; }

        /// <summary>
        /// 是否增加换行
        /// </summary>
        public bool NeedNewLine { get; set; }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            var message = $"{this.Message}";

            if (this.CurrentProcess > 0)
            {
                message += $", CurrentProcess: {this.CurrentProcess} %";
            }

            if (this.NeedNewLine)
            {
                message += Environment.NewLine;
            }

            return message;
        }
    }
}