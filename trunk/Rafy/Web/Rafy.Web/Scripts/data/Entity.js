/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：201201
 * 说明：
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 201201
 * 
*******************************************************/

Ext.define('Rafy.data.Entity', {
    extend: 'Ext.data.Model',
    idProperty: Rafy._IdPropertyName,
    constructor: function () {
        this.callParent(arguments);

        this.addEvents('propertyChanged');
    },
    //Id 已经支持多种类型，所以它的声明放到子类中。
    //fields: [
    //    { name: 'Id', type: 'int' }
    //],
    proxy: {
        type: 'ajax',
        url: 'Rafy_EntityDataPortal.ashx',
        reader: {
            type: 'json',
            root: 'entities',
            totalProperty: 'totalCount'
        }
    },

    isSelfDirty: function () {
        /// <summary>
        /// 判断当前对象的某个属性是否已经被修改。
        /// </summary>
        return this.dirty || this.phantom;
    },
    isNew: function () {
        /// <summary>
        /// 判断整个组合对象是否是刚构造出来的对象。（还没有提交到服务端。）
        /// </summary>
        /// <returns type=""></returns>
        return this.phantom;
    },
    isDirty: function () {
        /// <summary>
        /// 判断整个组合对象中某个部分是否已经被修改。
        /// </summary>

        if (this.isSelfDirty()) return true;

        //如果加载的组合子集合中任意一个被修改，则整个组合对象也是脏的。
        var loadedChildren = this.getLoadedChildren();
        for (var i = 0, lcc = loadedChildren.length; i < lcc; i++) {
            var childrenStore = loadedChildren.getAt(i);
            if (childrenStore.isDirty()) return true;
        }

        //整个组合对象都没有被修改。
        return false;
    },
    getLoadedChildren: function () {
        /// <summary>
        /// 返回所有的组合子集合的数据集集合。
        /// </summary>
        /// <returns type="MixedCollection">Key: 属性名。Value：组合子集合数据集。</returns>
        var me = this;

        var res = new Ext.util.MixedCollection();

        //entity.associations 中存储了 entity 所有的关联配置 AssociationConfig。
        Rafy.each(me.associations, function (a) {
            if (a.type == 'hasMany') {
                var childrenStore = me[a.storeName];
                if (childrenStore) {
                    res.add(a.name, childrenStore);
                }
            }
        });

        return res;
    },

    set: function (property, value) {
        /// <summary>
        /// override
        /// 设置某个属性的值。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        this.callParent(arguments);

        if (arguments.length > 1 || !Ext.isObject(property)) {
            this._onPropertyChanged({ property: property, value: value, entity: this });
        }
    },
    _onPropertyChanged: function (e) {
        /// <summary>
        /// protected virtual
        /// 属性变更事件。
        /// </summary>
        /// <param name="e"></param>
        this.fireEvent('propertyChanged', e);
    }
});