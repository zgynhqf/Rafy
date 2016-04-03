//*******************************************************
// * 
// * 作者：胡庆访
// * 创建时间：20110320
// * 说明：此文件只包含一个类，具体内容见类型注释。
// * 运行环境：.NET 4.0
// * 版本号：1.0.0
// * 
// * 历史记录：
// * 创建文件 胡庆访 20100320
// * 
//*******************************************************/

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace Rafy.Library
//{
//    /// <summary>
//    /// 数据库创建的方式
//    /// </summary>
//    public enum DbCreationType
//    {
//        /// <summary>
//        /// An implementation of IDatabaseInitializer that will recreate and optionally
//        /// re-seed the database only if the database does not exist.  To seed the database,
//        /// create a derived class and override the Seed method.
//        /// </summary>
//        CreateIfNotExists,

//        /// <summary>
//        /// An implementation of IDatabaseInitializer that will DELETE, recreate, and
//        /// optionally re-seed the database only if the model has changed since the database
//        /// was created. This is achieved by writing a hash of the store model to the
//        /// database when it is created and then comparing that hash with one generated
//        /// from the current model.  To seed the database, create a derived class and
//        /// override the Seed method.
//        /// </summary>
//        CreateIfModelChanges,

//        /// <summary>
//        /// do nothing with db creation.
//        /// </summary>
//        Ignore
//    }
//}
