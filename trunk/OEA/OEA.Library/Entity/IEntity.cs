﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using OEA.MetaModel;

namespace OEA.Library
{
    /// <summary>
    /// “实体”
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// 主键
        /// </summary>
        int Id { get; set; }

        /// <summary>
        /// 获取当前实体的 仓库
        /// </summary>
        /// <returns></returns>
        IRepository GetRepository();
    }
}