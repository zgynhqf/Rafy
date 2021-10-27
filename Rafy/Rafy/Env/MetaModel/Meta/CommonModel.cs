/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120311
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120311
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.MetaModel
{
    /// <summary>
    /// WPF Web 通用的模型
    /// </summary>
    public  class CommonModel
    {
        public static CommonModel Instance = new CommonModel();

        protected CommonModel() { }

        private static EntityMetaRepository _entities;

        static CommonModel()
        {
            Instance.Reset();
        }

        /// <summary>
        /// 所有实体元数据
        /// </summary>
        public static EntityMetaRepository Entities
        {
            get { return _entities; }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                _entities = value;
            }
        }

        public virtual void Reset()
        {
            _entities = new EntityMetaRepository();
            _entities.FreezeItems();
        }
    }
}