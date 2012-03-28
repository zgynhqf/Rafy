using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Text.RegularExpressions;
using System.Web;

namespace hxy
{
    public static class TextFormatter
    {
        #region AggregateString

        private static readonly char _charSplit = ',';
        /// <summary>
        /// Aggregate a array to a string which splitted by comma.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <returns></returns>
        public static string AggregateString(IEnumerable<string> array)
        {
            return AggregateString(array, new StringBuilder());
        }
        /// <summary>
        /// Aggregate a array to a string which splitted by comma.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="buffer">share this variation</param>
        /// <returns></returns>
        public static string AggregateString(IEnumerable<string> array, StringBuilder buffer)
        {
            if (array == null)
            {
                return string.Empty;
            }

            var _array = array as string[];
            if (_array != null)
            {
                if (_array.Length <= 0)
                {
                    return string.Empty;
                }
                if (_array.Length == 1)
                {
                    return _array[0];
                }
            }
            var list = array as IList<string>;
            if (list != null)
            {
                if (list.Count <= 0)
                {
                    return string.Empty;
                }
                if (list.Count == 1)
                {
                    return list[0];
                }
            }

            //prepare buffer
            if (buffer == null)
            {
                buffer = new StringBuilder();
            }
            else
            {
                buffer.Remove(0, buffer.Length);
            }

            foreach (var a in array)
            {
                if (buffer.Length > 0)
                {
                    buffer.Append(_charSplit);
                }
                buffer.Append(a);
            }
            //array.Aggregate(0, (s, a) =>
            //{
            //    if (buffer.Length > 0)
            //    {
            //        buffer.Append(_charSplit);
            //    }
            //    buffer.Append(a);
            //    return 0;
            //});
            return buffer.ToString();
        }
        /// <summary>
        /// Aggregate a array to a string which splitted by comma.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <returns></returns>
        public static string AggregateString<T>(IEnumerable<T> array, Func<T, string> toString)
        {
            return AggregateString(array, toString, new StringBuilder());
        }
        /// <summary>
        /// Aggregate a array to a string which splitted by comma.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="buffer">share this variation</param>
        /// <returns></returns>
        public static string AggregateString<T>(IEnumerable<T> array, Func<T, string> toString, StringBuilder buffer)
        {
            if (array == null)
            {
                return string.Empty;
            }
            if (toString == null)
            {
                throw new ArgumentNullException("toString");
            }
            var _array = array as T[];
            if (_array != null)
            {
                if (_array.Length <= 0)
                {
                    return string.Empty;
                }
                if (_array.Length == 1)
                {
                    return toString(_array[0]);
                }
            }
            var list = array as IList<T>;
            if (list != null)
            {
                if (list.Count <= 0)
                {
                    return string.Empty;
                }
                if (list.Count == 1)
                {
                    return toString(list[0]);
                }
            }

            //prepare buffer
            if (buffer == null)
            {
                buffer = new StringBuilder();
            }
            else
            {
                buffer.Remove(0, buffer.Length);
            }

            foreach (var a in array)
            {
                if (buffer.Length > 0)
                {
                    buffer.Append(_charSplit);
                }
                buffer.Append(toString(a));
            }
            //array.Aggregate(0, (s, a) =>
            //{
            //    if (buffer.Length > 0)
            //    {
            //        buffer.Append(_charSplit);
            //    }
            //    buffer.Append(toString(a));
            //    return 0;
            //});
            return buffer.ToString();
        }
        /// <summary>
        /// Aggregate a array to a string which splitted by comma.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="buffer">share this variation</param>
        /// <returns></returns>
        public static string AggregateString<T>(IEnumerable<T> array, Func<T, string> toString, string split, StringBuilder buffer)
        {
            if (array == null)
            {
                return string.Empty;
            }
            if (toString == null)
            {
                throw new ArgumentNullException("toString");
            }
            var _array = array as T[];
            if (_array != null)
            {
                if (_array.Length <= 0)
                {
                    return string.Empty;
                }
                if (_array.Length == 1)
                {
                    return toString(_array[0]);
                }
            }
            var list = array as IList<T>;
            if (list != null)
            {
                if (list.Count <= 0)
                {
                    return string.Empty;
                }
                if (list.Count == 1)
                {
                    return toString(list[0]);
                }
            }

            //prepare buffer
            if (buffer == null)
            {
                buffer = new StringBuilder();
            }
            else
            {
                buffer.Remove(0, buffer.Length);
            }

            foreach (var a in array)
            {
                if (buffer.Length > 0)
                {
                    buffer.Append(split);
                }
                buffer.Append(toString(a));
            }
            //array.Aggregate(0, (s, a) =>
            //{
            //    if (buffer.Length > 0)
            //    {
            //        buffer.Append(_charSplit);
            //    }
            //    buffer.Append(toString(a));
            //    return 0;
            //});
            return buffer.ToString();
        }

        #endregion

        #region SqlInjection

        public static string SafeSqlLiteral(string inputSQL)
        {
            return inputSQL.Replace("'", "''");
        }
        public static string SafeSqlInLikeClause(string inputSQL)
        {
            inputSQL = inputSQL.Replace("[", "[[]");
            inputSQL = inputSQL.Replace("%", "[%]");
            inputSQL = inputSQL.Replace("_", "[_]");
            inputSQL = inputSQL.Replace("'", "''");
            return inputSQL;
        }
        private static readonly string[] _arrBadChars = new string[] { 
            "+", "'", "%", "^", "&", "?", "(", ")", "<", ">", "[", "]", "{", "}", "/", "\"", 
            ";", ":", "Chr(34)", "Chr(0)", "--"
        };
        public static string IgnoreBadChar(string strchar)
        {
            if (string.IsNullOrEmpty(strchar))
            {
                return string.Empty;
            }
            StringBuilder builder = new StringBuilder(strchar);
            for (int i = 0, l = _arrBadChars.Length; i < l; i++)
            {
                builder.Replace(_arrBadChars[i], string.Empty);
            }
            string result = builder.ToString();
            result = Regex.Replace(result, "@+", "@");
            return result;
        }

        #endregion

        #region Regular Expressions

        /// <summary>
        /// All chars are chinese words.
        /// </summary>
        public static readonly Regex ReAllChinese = new Regex("^[\u4e00-\u9fa5]+$", RegexOptions.Compiled);
        /// <summary>
        /// URL
        /// </summary>
        public static readonly Regex ReUrl = new Regex(@"^[a-zA-Z]+://(\w+(-\w+)*)(\.(\w+(-\w+)*))*(\?\S*)?$", RegexOptions.Compiled);
        /// <summary>
        /// Telephone
        /// </summary>
        public static readonly Regex RePhone = new Regex("^\\(0\\d{2}\\)[- ]?\\d{8}&|^0\\d{2}[- ]?\\d{8}&|^\\(0\\d{3}\\)[- ]?\\d{7}&|^0\\d{3}[- ]?\\d{7}$", RegexOptions.Compiled);
        /// <summary>
        /// Mobile phone
        /// </summary>
        public static readonly Regex ReMobilePhone = new Regex("^1[3,5,8]\\d{9}$", RegexOptions.Compiled);
        /// <summary>
        /// All numbers, could be a negative number.(start with a '-')
        /// </summary>
        public static readonly Regex ReNumber = new Regex("^-?\\d+$|^(-?\\d+)(\\.\\d+)?$", RegexOptions.Compiled);
        /// <summary>
        /// Positive number or zero.
        /// </summary>
        public static readonly Regex ReNotNagtiveNumber = new Regex(@"^\d+$", RegexOptions.Compiled);
        /// <summary>
        /// Positive integer
        /// </summary>
        public static readonly Regex ReUint = new Regex("^[0-9]*[1-9][0-9]*$", RegexOptions.Compiled);
        /// <summary>
        /// All letters
        /// </summary>
        public static readonly Regex ReLetter = new Regex("^[A-Za-z]+$", RegexOptions.Compiled);
        /// <summary>
        /// Indicates a IPV4 address
        /// </summary>
        public static readonly Regex ReIPAddress = new Regex(@"^(((\d{1,2})|(1\d{2})|(2[0-4]\d)|(25[0-5]))\.){3}((\d{1,2})|(1\d{2})|(2[0-4]\d)|(25[0-5]))$", RegexOptions.Compiled);

        /// <summary>
        /// Is the string all number?
        /// </summary>
        public static readonly Regex ReAllNumber = new Regex(@"^\d+$");
        /// <summary>
        /// It the string all letters?
        /// </summary>
        public static readonly Regex ReAllLetter = new Regex(@"^[a-zA-Z]+$");
        /// <summary>
        /// Are the chars of the string all letters or numbers?
        /// </summary>
        public static readonly Regex ReAllLetterOrNumber = new Regex(@"^[a-zA-Z\d]+$");
        /// <summary>
        /// is the string match the rule of C# programming.
        /// </summary>
        public static readonly Regex ReCSharpProgramable = new Regex(@"^[A-Za-z_]\w*$", RegexOptions.Compiled);

        /// <summary>
        /// script tag and its inner code.
        /// </summary>
        public static readonly Regex ReScripts = new Regex(@"<script[^<>]*>[^<>]*</script>", RegexOptions.IgnoreCase);
        /// <summary>
        /// E-Mail address
        /// </summary>
        public static readonly Regex ReEMail = new Regex(@"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
        /// <summary>
        /// new line
        /// </summary>
        public static readonly Regex ReNewLine = new Regex("\r\n|\r|\n", RegexOptions.Compiled);
        /// <summary>
        /// 2 or more white spaces
        /// </summary>
        public static readonly Regex ReWhiteSpaces = new Regex("\\s{2,}", RegexOptions.Compiled);
        /// <summary>
        /// 2 or more spaces
        /// </summary>
        public static readonly Regex ReBlanks = new Regex(" {2,}", RegexOptions.Compiled);
        /// <summary>
        /// XML tags
        /// </summary>
        public static readonly Regex ReTags = new Regex(@"<[^<>]+>", RegexOptions.Compiled);
        /// <summary>
        /// XML tags and "&...;"
        /// eg. &nbsp; &bt;
        /// </summary>
        public static readonly Regex ReHTML = new Regex(@"(<[^<>]+>)|(\&\w+;)", RegexOptions.Compiled);

        #endregion
    }
}