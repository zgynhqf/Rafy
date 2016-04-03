/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110704
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110704
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Utils
{
    [Serializable]
    public class NumberRange
    {
        private static readonly string SPLIT_STRING = " - ";

        private double _beginValue;

        private double _endValue;

        public NumberRange() { }

        /// <summary>
        /// 克隆一个对象
        /// </summary>
        /// <param name="a"></param>
        public NumberRange(NumberRange a)
        {
            this._beginValue = a._beginValue;
            this._endValue = a._endValue;
        }

        public double BeginValue
        {
            get
            {
                return this._beginValue;
            }
            set
            {
                this._beginValue = value;
                if (this._beginValue > this._endValue)
                {
                    this._endValue = this._beginValue;
                }
            }
        }

        public double EndValue
        {
            get
            {
                return this._endValue;
            }
            set
            {
                this._endValue = value;
                if (this._beginValue > this._endValue)
                {
                    this._beginValue = this._endValue;
                }
            }
        }

        public override string ToString()
        {
            return this._beginValue.ToString() + SPLIT_STRING + this._endValue.ToString();
        }

        public static NumberRange Parse(string value)
        {
            var result = new NumberRange();

            if (string.IsNullOrEmpty(value) == false)
            {
                var values = value.Split(new string[] { SPLIT_STRING }, StringSplitOptions.RemoveEmptyEntries);
                if (values.Length > 1)
                {
                    result._beginValue = Convert.ToDouble(values[0]);
                    result._endValue = Convert.ToDouble(values[1]);
                }
                else if (values.Length == 1)
                {
                    result._endValue = Convert.ToDouble(values[0]);
                }
            }

            return result;
        }
    }
}