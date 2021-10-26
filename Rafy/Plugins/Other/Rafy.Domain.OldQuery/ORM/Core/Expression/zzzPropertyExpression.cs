//暂留。如果以后需要自定义表达式的话，可以尝试使用这些代码。


//*******************************************************
// * 
// * 作者：胡庆访
// * 创建日期：20130529
// * 运行环境：.NET 4.0
// * 版本号：1.0.0
// * 
// * 历史记录：
// * 创建文件 胡庆访 20130529 11:48
// * 
//*******************************************************/

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace Rafy.Domain
//{
//    /// <summary>
//    /// 属性表达式
//    /// <remarks>
//    /// 不使用 Linq 表达式的原因是：
//    /// Linq 性能较差。
//    /// </remarks>
//    /// </summary>
//    public abstract class PropertyExpression
//    {
//        public abstract PropertyExpressionType Type { get; }

//        #region 操作符

//        public static PropertyExpression operator &(PropertyExpression left, PropertyExpression right)
//        {
//            return new AndExpression
//            {
//                Left = left,
//                Right = right
//            };
//        }

//        public static PropertyExpression operator |(PropertyExpression left, PropertyExpression right)
//        {
//            return new OrExpression
//            {
//                Left = left,
//                Right = right
//            };
//        }

//        #endregion
//    }

//    public enum PropertyExpressionType
//    {
//        PropertyComparison,
//        And,
//        Or
//    }

//    public class AndExpression : PropertyExpression
//    {
//        public PropertyExpression Left { get; internal set; }
//        public PropertyExpression Right { get; internal set; }
//        public override PropertyExpressionType Type
//        {
//            get { return PropertyExpressionType.And; }
//        }
//    }

//    public class OrExpression : PropertyExpression
//    {
//        public PropertyExpression Left { get; internal set; }
//        public PropertyExpression Right { get; internal set; }
//        public override PropertyExpressionType Type
//        {
//            get { return PropertyExpressionType.Or; }
//        }
//    }
//}