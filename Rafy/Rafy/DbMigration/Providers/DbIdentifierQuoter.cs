/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20170920
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20170920 15:42
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.DbMigration
{
    /// <summary>
    /// 数据库标识符的处理器。
    /// </summary>
    public interface IDbIdentifierQuoter
    {
        /// <summary>
        /// 准备 SQL 中所使用到的任意一个关键标识（表、字段、外键、主键、约束名等）。
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        string Prepare(string identifier);

        /// <summary>
        /// 标识符被引用时的起始字符
        /// </summary>
        char QuoteStart { get; }

        /// <summary>
        /// 标识符被引用时的终止字符
        /// </summary>
        char QuoteEnd { get; }
    }

    /// <summary>
    /// 数据库标识符的处理器。
    /// </summary>
    public abstract class DbIdentifierQuoter : IDbIdentifierQuoter
    {
        /// <summary>
        /// 标识符被引用时的起始字符
        /// </summary>
        public abstract char QuoteStart { get; }

        /// <summary>
        /// 标识符被引用时的终止字符。
        /// 默认与 <see cref="QuoteStart"/> 相同。
        /// </summary>
        public virtual char QuoteEnd { get => this.QuoteStart; }

        /// <summary>
        /// 对于最终生成好的关键标识，在其外围添加相应的引用符。
        /// 它与 <see cref="IDbIdentifierQuoter.Prepare(string)" /> 方法的区别在于，后者可以对一个标识符中的某一部分使用。
        /// 例如：PK_Table_Id 中的 Table、Id 都需要调用 Prepare，而最终的 PK_Table_Id 则只需要调用 Quote。
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <returns></returns>
        public string Quote(string identifier)
        {
            identifier = this.Prepare(identifier);

            if (this.QuoteStart != char.MinValue)
            {
                return this.QuoteStart + identifier + this.QuoteEnd;
            }

            return identifier;
        }

        /// <summary>
        /// 准备 SQL 中所使用到的任意一个关键标识（表、字段、外键、主键、约束名等）。
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public virtual string Prepare(string identifier)
        {
            return identifier;
        }

        /// <summary>
        /// 某些数据库对标识符的长度会有限制，例如 Oracle 中不能超过 30 个字符。
        /// 这个方法可以把传入的字符串剪裁到 length 个字符，并尽量保持有用的信息。
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        internal static string LimitIdentifier(string identifier, int length = 30)
        {
            if (identifier.Length > length)
            {
                //var toCut = identifier.Length - length;

                //if (identifier.StartsWith("FK_"))
                //{
                //    identifier = identifier.Substring(3);
                //}

                ////保留 ID 字样
                //var newName = identifier.Replace("Id", "ID");

                ////从后面开始把多余的小写字母去除。
                //var list = newName.ToList();
                //for (int i = list.Count - 1; i >= 0 && toCut > 0; i--)
                //{
                //    var c = list[i];
                //    if (char.IsLower(c))
                //    {
                //        list.RemoveAt(i);
                //        toCut--;
                //    }
                //}
                ////如何还是太长，直接截取
                //for (int i = list.Count - 1; toCut > 0; i--)
                //{
                //    list.RemoveAt(i);
                //    toCut--;
                //}
                //newName = new string(list.ToArray());

                //return newName;

                //以上算法会导致一些缩写的名称重复。所以不如直接截取。这样，外层程序应该保证越在前面的字符，重要性越高。
                return identifier.Substring(0, length);
            }

            return identifier;
        }
    }
}