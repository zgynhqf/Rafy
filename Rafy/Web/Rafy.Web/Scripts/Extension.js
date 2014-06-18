/*******************************************************
 * 
 * 作者：胡庆访
 * 说明：此文件中的代码用于扩展 js、Ext 现有的类型。
 * 创建日期：20130815
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130815 16:10
 * 
*******************************************************/

(function () {
    Ext.apply(String.prototype, {
        t: function () {
            /// <summary>
            /// 扩展在字符串上的方法。使用多国语言引擎，翻译某个固定的字符串。
            /// </summary>
            /// <returns type="String"></returns>
            var raw = this.toString();
            return Rafy.translate(raw);
        }
    });

    var storeExt = {
        isDirty: function () {
            //如果有删除的项，则已经是脏的。
            if (this.getRemovedRecords().length) { return true; }

            //如果其它的任意项是脏的，则整个集合也是脏的。
            var dirtyItem = Rafy.each(this, function (entity) {
                if (entity.isDirty()) return false;
            });
            if (dirtyItem) { return true; }

            //集合未更改。
            return false;
        }
    };
    Ext.apply(Ext.data.Store.prototype, storeExt);
    Ext.apply(Ext.data.TreeStore.prototype, storeExt);

    //给 TreeStore 添加一个 loadRawData 的方法，这样，Store 和 TreeStore 都有这个方法了。
    Ext.apply(Ext.data.TreeStore.prototype, {
        loadRawData: function (serverData) {
            this.setRootNode(serverData);
        }
    });
})();