/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111110
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111110
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Serialization.Mobile
{
    /// <summary>
    /// 低存储空间的多个 Boolean 值序列化器
    /// </summary>
    public struct BitContainer
    {
        private Bit _bits;

        public BitContainer(int intValue)
        {
            this._bits = (Bit)intValue;
        }

        public void SetValue(Bit bit, bool value)
        {
            if (value)
            {
                this._bits |= bit;
            }
            else
            {
                var reverse = (Bit)0x11111111 ^ bit;
                this._bits &= reverse;
            }
        }

        public bool GetValue(Bit bit)
        {
            return (this._bits & bit) == bit;
        }

        public int ToInt32()
        {
            return (int)this._bits;
        }
    }

    public enum Bit
    {
        _1 = 1,
        _2 = 2,
        _4 = 4,
        _8 = 8,
        _16 = 16,
        _32 = 32,
        _64 = 64
    }
}
