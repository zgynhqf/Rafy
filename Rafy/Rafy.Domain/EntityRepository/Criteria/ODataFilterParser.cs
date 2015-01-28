/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20141217
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20141217 14:11
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Rafy.Domain.ORM.Query;
using Rafy.ManagedProperty;

namespace Rafy.Domain
{
    /// <summary>
    /// 用于解析类 OData 查询中的 filter 查询字符串。
    /// </summary>
    internal class ODataFilterParser
    {
        internal ManagedPropertyList _properties;

        private IQuery _query;
        private ITableSource _mainTable;
        private StringReader _reader;
        private QueryFactory _f;
        private IConstraint _current;
        private BinaryOperator? _concat;
        private IColumnNode _column;
        private string _comparison;
        private Stack<StackItem> _bracketStack;
        struct StackItem
        {
            public IConstraint Constraint;
            public BinaryOperator Concat;
        }

        public void Parse(string filter, IQuery query)
        {
            _query = query;
            _mainTable = query.MainTable;
            _column = null;
            _comparison = null;
            _concat = null;
            _current = null;
            _f = QueryFactory.Instance;
            _bracketStack = new Stack<StackItem>();

            using (_reader = new StringReader(filter))
            {
                while (true)
                {
                    var part = this.ReadPart();
                    if (part == null) break;
                    this.DealPart(part);
                }
            }

            query.Where = _current;
        }

        private void DealPart(string part)
        {
            if (part == "(" || part == ")")
            {
                this.DealBracket(part);
            }
            else if (part.EqualsIgnoreCase("and") || part.EqualsIgnoreCase("or"))
            {
                _concat = part.EqualsIgnoreCase("and") ? BinaryOperator.And : BinaryOperator.Or;
            }
            else
            {
                this.DealConstraintWord(part);
            }
        }

        private void DealBracket(string part)
        {
            if (part == "(")
            {
                if (_concat.HasValue && _current != null)
                {
                    _bracketStack.Push(new StackItem
                    {
                        Concat = _concat.Value,
                        Constraint = _current
                    });
                    _concat = null;
                    _current = null;
                }
            }
            else
            {
                if (_bracketStack.Count > 0)
                {
                    var outBracket = _bracketStack.Pop();
                    _current = _f.Binary(outBracket.Constraint, outBracket.Concat, _current);
                }
            }
        }

        private void DealConstraintWord(string part)
        {
            if (_column == null)
            {
                //可以使用了引用属性，例如表达式：User.Name eq 'huqf'
                var properties = part.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                if (properties.Length > 1)
                {
                    ITableSource lastTable = _mainTable;
                    for (int i = 0; i < properties.Length; i++)
                    {
                        var property = properties[i];
                        var mp = lastTable.EntityRepository.EntityMeta.ManagedProperties.GetCompiledProperties().Find(property);
                        if (mp == null) throw new InvalidProgramException(string.Format("查询条件解析出错，没有找到名称为 {0} 的属性。", property));

                        var refProperty = mp as IRefEntityProperty;
                        if (refProperty != null)
                        {
                            lastTable = _f.FindOrCreateJoinTable(_query, lastTable, refProperty);
                        }
                        else
                        {
                            _column = lastTable.Column(mp);
                        }
                    }
                    if (_column == null) { throw new InvalidProgramException(string.Format("{0} 查询条件出错：属性表达式不能以引用实体属性结尾。", part)); }
                }
                else
                {
                    var mp = _properties.Find(part, true);
                    if (mp == null) throw new InvalidProgramException(string.Format("查询条件解析出错，没有找到名称为 {0} 的属性。", part));
                    _column = _mainTable.Column(mp);
                }
            }
            else if (_comparison == null)
            {
                _comparison = part;
            }
            else
            {
                var propertyConstraint = CreateColumnConstraint(_comparison, part);
                if (_concat.HasValue && _current != null)
                {
                    _current = _f.Binary(_current, _concat.Value, propertyConstraint);
                    _concat = null;
                }
                else
                {
                    _current = propertyConstraint;
                }

                _column = null;
                _comparison = null;
            }
        }

        private IConstraint CreateColumnConstraint(string comparison, string value)
        {
            var op = PropertyOperator.Equal;
            switch (comparison.ToLower())
            {
                case "eq":
                    op = PropertyOperator.Equal;
                    break;
                case "ne":
                    op = PropertyOperator.NotEqual;
                    break;
                case "gt":
                    op = PropertyOperator.Greater;
                    break;
                case "ge":
                    op = PropertyOperator.GreaterEqual;
                    break;
                case "lt":
                    op = PropertyOperator.Less;
                    break;
                case "le":
                    op = PropertyOperator.LessEqual;
                    break;
                case "contains":
                    op = PropertyOperator.Contains;
                    break;
                case "startwith":
                    op = PropertyOperator.StartWith;
                    break;
                case "endwith":
                    op = PropertyOperator.EndWith;
                    break;
                default:
                    throw new NotSupportedException("不支持这个操作符：" + comparison + "。");
            }

            return _f.Constraint(_column, op, value);
        }

        #region ReadPart

        private StringBuilder _wordBuffer = new StringBuilder();

        /// <summary>
        /// Part 有以下类型：括号、单词。
        /// </summary>
        /// <returns></returns>
        private string ReadPart()
        {
            _wordBuffer.Clear();
            bool hasString = false;

            while (true)
            {
                var cValue = _reader.Read();
                if (cValue < 0) break;

                var c = (char)cValue;

                //括号直接返回。
                if (c == '(' || c == ')')
                {
                    return c.ToString();
                }

                //如果是字符串，则直接返回。
                if (c == '\'' || c == '"')
                {
                    //如果是结尾的字符串，则表示结束。
                    if (hasString) { break; }

                    hasString = true;
                    continue;
                }

                if (!char.IsWhiteSpace(c) || hasString)
                {
                    //非括号、非空白，或者正在处理包含在字符串中的字符时，需要添加到单词中。
                    _wordBuffer.Append(c);
                }
                else
                {
                    if (_wordBuffer.Length > 0)
                    {
                        //以空格结尾，表示单词获取完成。
                        break;
                    }
                    else
                    {
                        //起始的空格需要被忽略。
                    }
                }
            }

            return _wordBuffer.Length > 0 ? _wordBuffer.ToString() : null;
        }

        #endregion
    }
}