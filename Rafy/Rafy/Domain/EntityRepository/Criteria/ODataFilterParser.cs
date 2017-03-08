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
using Rafy.Reflection;
using Rafy.Utils;

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

            _filter = filter;
            _filterIndex = 0;
            _filterLength = _filter.Length;
            while (true)
            {
                var part = this.ReadPart();
                if (part == string.Empty) break;
                this.DealPart(part);
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
            //part 表示列名
            if (_column == null)
            {
                //可能使用了引用属性，例如表达式：User.Name eq 'huqf'
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
            //part 表示操作符
            else if (_comparison == null)
            {
                _comparison = part;
            }
            //part 表示值
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
            #region 转换操作符

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
                case "startswith":
                    op = PropertyOperator.StartsWith;
                    break;
                case "endswith":
                    op = PropertyOperator.EndsWith;
                    break;
                case "notcontains":
                    op = PropertyOperator.NotContains;
                    break;
                case "notstartswith":
                    op = PropertyOperator.NotStartsWith;
                    break;
                case "notendswith":
                    op = PropertyOperator.NotEndsWith;
                    break;
                default:
                    throw new NotSupportedException("不支持这个操作符：" + comparison + "。");
            }

            #endregion

            #region 把表达式中的值转换为列的类型对应的值。（同时，兼容处理枚举的 Label 值。）

            object columnValue = null;
            var propertyType = _column.Property.PropertyType;
            var innerType = TypeHelper.IgnoreNullable(propertyType);
            if (innerType.IsEnum)
            {
                columnValue = EnumViewModel.Parse(value, innerType);
            }
            else
            {
                columnValue = TypeHelper.CoerceValue(_column.Property.PropertyType, value);
            }

            #endregion

            return _f.Constraint(_column, op, columnValue);
        }

        #region ReadPart

        //过滤字符串长度
        private int _filterLength;

        private string _filter;
        private int _filterIndex;

        private StringBuilder _wordBuffer = new StringBuilder();

        /// <summary>
        /// Part 有以下类型：括号、单词。
        /// </summary>
        /// <returns>
        /// 空字符串：表示没有字符，无法读取。
        /// null：表示读取到空值。
        /// </returns>
        private string ReadPart()
        {
            _wordBuffer.Clear();
            bool stringStarted = false;

            while (true)
            {
                if (_filterIndex >= _filterLength) break;

                var c = _filter[_filterIndex++];

                //括号直接返回。
                if (!stringStarted && (c == '(' || c == ')'))
                {
                    return c.ToString();
                }

                //处理特殊字符‘  “  \  前面需要加上转义字符 \ 如 : filter = "Name contains 'aa\'bb'" ;
                if (c == '\\')
                {
                    //直接添加下一个字符
                    var next = _filter[_filterIndex++];
                    _wordBuffer.Append(next);
                    continue;
                }
                //如果是字符串，则直接返回。
                if (c == '\'' || c == '"')
                {
                    //如果是结尾的字符串，则表示结束。
                    if (stringStarted) { break; }

                    stringStarted = true;
                    continue;
                }

                if (!char.IsWhiteSpace(c) || stringStarted)
                {
                    //非括号、非空白，或者正在处理包含在字符串中的空白字符时，需要添加到单词中。
                    _wordBuffer.Append(c);

                    //如果单词后面紧跟括号，那么单词也必须结束。
                    if (_filterIndex < _filterLength)
                    {
                        var next = _filter[_filterIndex];
                        if (!stringStarted && (next == '(' || next == ')')) { break; }
                    }
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

            var res = _wordBuffer.ToString();

            if (!stringStarted && res.EqualsIgnoreCase("null"))
            {
                res = null;
            }

            return res;
        }

        #endregion
    }
}