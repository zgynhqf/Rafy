//*******************************************************
// * 
// * 作者：胡庆访
// * 创建日期：20130529
// * 运行环境：.NET 4.0
// * 版本号：1.0.0
// * 
// * 历史记录：
// * 创建文件 胡庆访 20130529 12:02
// * 
//*******************************************************/

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace Rafy.Domain
//{
//    public abstract class PropertyExpressionVisitor
//    {
//        protected PropertyExpression Visit(PropertyExpression exp)
//        {
//            switch (exp.Type)
//            {
//                case PropertyExpressionType.PropertyComparison:
//                    exp = this.Visit(exp as PropertyComparisonExpression);
//                    break;
//                case PropertyExpressionType.And:
//                    exp = this.Visit(exp as AndExpression);
//                    break;
//                case PropertyExpressionType.Or:
//                    exp = this.Visit(exp as OrExpression);
//                    break;
//                default:
//                    break;
//            }

//            return exp;
//        }

//        protected virtual PropertyComparisonExpression Visit(PropertyComparisonExpression exp)
//        {
//            return exp;
//        }

//        protected virtual AndExpression Visit(AndExpression exp)
//        {
//            return exp;
//        }

//        protected virtual OrExpression Visit(OrExpression exp)
//        {
//            return exp;
//        }
//    }
//}
