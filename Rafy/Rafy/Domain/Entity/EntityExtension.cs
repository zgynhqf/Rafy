////*******************************************************
// * 
// * 作者：胡庆访
// * 创建日期：20211011
// * 说明：此文件只包含一个类，具体内容见类型注释。
// * 运行环境：.Net Standard 2.0
// * 版本号：1.0.0
// * 
// * 历史记录：
// * 创建文件 胡庆访 20211011 18:10
// * 
//*******************************************************/

//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace Rafy.Domain
//{
//    public static class EntityExtension
//    {
//        /// <summary>
//        /// 标记当前对象为需要保存的状态。
//        /// 
//        /// 只有实体的状态是 Unchanged 状态时（其它状态已经算是 Dirty 了），调用本方法才会把实体的状态改为 Modified。
//        /// </summary>
//        internal static void MarkModifiedIfSaved(Entity entity)
//        {
//            //只有 Unchanged 状态时，才需要标记，这是因为其它状态已经算是 Dirty 了。
//            if (entity.PersistenceStatus == PersistenceStatus.Saved)
//            {
//                entity.PersistenceStatus = PersistenceStatus.Modified;
//            }
//        }
//    }
//}