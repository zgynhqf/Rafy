/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110314
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100314
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Csla;
using Csla.Core;
using OEA.MetaModel;

namespace OEA
{
    public static class ObjectViewExtension
    {
        /// <summary>
        /// 查找子对象(childObjectType)在树(currView.Data)中的外键名称.
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        public static string GetChildForeignKey(this ObjectView currView, Type childObjectType)
        {
            foreach (var info in AppModel.Entities.FindEntityMeta(childObjectType).EntityProperties)
            {
                var r = info.ReferenceInfo;
                if (r != null && r.RefType == currView.EntityType)
                {
                    return info.Name;
                }
            }

            return null;
        }

        /// <summary>
        /// 查找子对象(childObjectType)在树(currView.Data)中的父对象名称.
        /// </summary>
        /// <param name="currView"></param>
        /// <param name="newObject"></param>
        /// <returns></returns>
        public static string GetChildForeignParent(this ObjectView currView, Type childObjectType)
        {
            foreach (var info in AppModel.Entities.FindEntityMeta(childObjectType).EntityProperties)
            {
                var r = info.ReferenceInfo;
                if (r != null && r.RefType == currView.EntityType)
                {
                    return r.RefEntityProperty;
                }
            }

            return null;
        }
    }
}