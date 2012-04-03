﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA.MetaModel
{
    public class TableMeta : Freezable
    {
        private string _TableName;

        private bool _SupprtMigrating;

        public TableMeta()
        {
            this._SupprtMigrating = true;
        }

        public TableMeta(string tableName)
        {
            this._TableName = tableName;
            this._SupprtMigrating = true;
        }

        /// <summary>
        /// 映射数据库中的字段名
        /// </summary>
        public string TableName
        {
            get { return this._TableName; }
        }

        public bool SupportMigrating
        {
            get { return this._SupprtMigrating; }
            set { this.SetValue(ref this._SupprtMigrating, value); }
        }
    }
}