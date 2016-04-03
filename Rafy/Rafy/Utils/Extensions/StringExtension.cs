/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：2008
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 2008
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy
{
    public static class StringExtension
    {
        /// <summary>
        /// if this string's length is more than size,
        /// cut the excessive part and append another string.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="size">whether longer than this size</param>
        /// <param name="appendMe">if longer, append this string</param>
        /// <returns></returns>
        public static string Cut(this string str, int size, string appendMe)
        {
            if (str.IsAllWhite())
            {
                return string.Empty;
            }
            if (str.Length > size)
            {
                return str.Substring(0, size) + appendMe;
            }
            else
            {
                return str;
            }
        }

        /// <summary>
        /// if this string's length is more than size,
        /// cut the excessive part and append another string.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="size">whether longer than this size</param>
        /// <returns></returns>
        public static string Cut(this string str, int size)
        {
            return Cut(str, size, string.Empty);
        }

        ///// <summary>
        ///// Cut a string by a english word size.
        ///// CutChinese("abcdefg",3,"...") => "abc..."
        ///// CutChinese("a胡庆访",3,"...") => "a胡..."
        ///// CutChinese("胡庆访",3,"...")  => "胡..."
        ///// </summary>
        ///// <param name="str"></param>
        ///// <param name="size">the size of Chinese chars.</param>
        ///// <param name="appendMe">if longer, append this string. This value could be null.</param>
        ///// <returns></returns>
        //public static string CutChinese(this string str, int size, string appendMe)
        //{
        //    //input check
        //    if (str == null)
        //    {
        //        throw new ArgumentNullException("str");
        //    }
        //    //convert to the size of english words.
        //    size *= 2;
        //    int strLen = str.Length;
        //    if (size < 0)
        //    {
        //        throw new ArgumentOutOfRangeException("size");
        //    }
        //    if (size > strLen)
        //    {
        //        return str;
        //    }

        //    for (int i = 0; i < size; ++i)
        //    {
        //        //if current char is a chinese char, cut size should be substracted.
        //        if (((int)(str[i]) & 0xFF00) != 0)
        //        {
        //            size--;
        //        }
        //    }

        //    //ensure boundary
        //    if (size < 0)
        //    {
        //        return appendMe;
        //    }
        //    if (size == str.Length)
        //    {
        //        return str;
        //    }

        //    return str.Substring(0, size) + appendMe;
        //}
        /// <summary>
        /// Cut a string by a english word size.
        /// CutChinese("abcdefg",3,"...") => "abc..."
        /// CutChinese("a胡庆访",3,"...") => "a胡..."
        /// CutChinese("胡庆访",3,"...")  => "胡..."
        /// </summary>
        /// <param name="str"></param>
        /// <param name="maxSize">the max size of English chars.</param>
        /// <param name="appendMe">if longer, append this string. This value could be null.</param>
        /// <returns></returns>
        public static string CutChinese(this string str, int maxSize, string appendMe)
        {
            //input check
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }
            if (maxSize < 0)
            {
                throw new ArgumentOutOfRangeException("size");
            }
            int strLen = str.Length;

            //Both chinese and english words, if this is ture, it means "no cut".
            if (maxSize > strLen)
            {
                return str;
            }
            //convert to the size of english words.
            int maxEnglishSize = maxSize * 2;

            //find the position to cut.
            int englishLength = 0;
            int index = 0;
            for (; englishLength < maxEnglishSize && index < strLen; index++)
            {
                //if current char is a chinese char, cut size should be substracted.
                if (((int)(str[index]) & 0xFF00) != 0)
                {
                    englishLength += 2;
                }
                else
                {
                    englishLength++;
                }
            }
            //at the end of circle, the index indicate where to remove.

            //If last char is a chinese word, this judgement is true.
            if (englishLength > maxEnglishSize)
            {
                index--;
            }

            //no cut.
            if (index == strLen)
            {
                return str;
            }

            return str.Remove(index) + appendMe;
        }

        /// <summary>
        /// Cut a string by a english word size.
        /// CutChinese("abcdefg",3) => "abc"
        /// CutChinese("a胡庆访",3) => "a胡"
        /// CutChinese("胡庆访",3)  => "胡"
        /// </summary>
        /// <param name="str"></param>
        /// <param name="maxSize">the max size of English chars.</param>
        /// <returns></returns>
        public static string CutChinese(this string str, int maxSize)
        {
            return str.CutChinese(maxSize, string.Empty);
        }

        /// <summary>
        /// 比较两个字符串是否相等。忽略大小写
        /// </summary>
        /// <param name="str"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool EqualsIgnoreCase(this string str, string target)
        {
            return string.Compare(str, target, true) == 0;
        }

        /// <summary>
        /// judge this string is :
        /// null/String.Empty/all white spaces.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsAllWhite(this string str)
        {
            return string.IsNullOrEmpty(str) || str.Trim() == string.Empty;
        }

        public static bool IsNullOrLengthBetween(this string str, int minLength, int maxLength)
        {
            if (str == null)
            {
                return true;
            }
            return str.IsLengthBetween(minLength, maxLength);
        }

        public static bool IsLengthBetween(this string str, int minLength, int maxLength)
        {
            if (str == null)
            {
                return false;
            }
            int length = str.Length;
            return length <= maxLength && length >= minLength;
        }

        public static string FormatArgs(this string str, object arg0)
        {
            return string.Format(str, arg0);
        }

        public static string FormatArgs(this string str, object arg0, object arg1)
        {
            return string.Format(str, arg0, arg1);
        }

        public static string FormatArgs(this string str, object arg0, object arg1, object arg2)
        {
            return string.Format(str, arg0, arg1, arg2);
        }

        public static string FormatArgs(this string str, params object[] args)
        {
            return string.Format(str, args);
        }

        /// <summary>
        /// Removes all leading and trailing white-space characters from the current System.String object.
        /// if it is null, return the string.Empty.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string TrimNull(this string s)
        {
            return string.IsNullOrEmpty(s) ? string.Empty : s.Trim();
        }
    }
}
