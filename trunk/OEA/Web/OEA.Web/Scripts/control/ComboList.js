Ext.define('Oea.control.ComboList', {
    extend: 'Ext.form.field.ComboBox',
    //extend: 'Oea.control.ComboBoxTest',
    alias: 'widget.combolist',

    //override default config
    editable: false,
    autoSelect: false,
    pageSize: 10,
    matchFieldWidth: false,

    //config
    model: '',

    //private fields
    _view: null,
    _refProperty: '',
    _refId: '',
    _isTree: false,

    //refProperty 的初始化放在 initComponent 中时，如果在 List 中下拉编辑时，是获取不到 Name 的。
    //    initComponent: function () {
    //        var me = this;
    //        me.callParent();
    //        
    //        me._refProperty = me.getName().replace("_Label", "");
    //    },

    /// <summary>重写父类构造控件方法，生成列表控件</summary>
    createPicker: function () {
        var me = this;

        var v = Oea.AutoUI.getMeta({
            async: false, //同步
            model: me.model,
            isLookup: true,
            isReadonly: true,
            ignoreCommands: true,
            callback: function (meta) {
                me._isTree = meta.isTree;

                Ext.applyIf(meta.gridConfig, {
                    floating: true,
                    hidden: true,
                    minWidth: 250,
                    ownerCt: me.up('[floating]')
                });
                if (me._isTree) {
                    meta.gridConfig.useArrows = true;
                }

                Ext.apply(meta.storeConfig, {
                    pageSize: me.pageSize
                });

                var v = Oea.AutoUI.createListView(meta);

                me._view = v;

                //重新设置数据源，这时，picker 还没有值，所以不会造成重复绑定。
                me.bindStore(v.getData());
            }
        });

        var grid = me._view.getControl();

        me.mon(grid.getView(), {
            itemclick: me.onItemClick,
            refresh: me.onListRefresh,
            scope: me
        });
        me.mon(grid.getSelectionModel(), {
            'beforeselect': me.onBeforeSelect,
            'beforedeselect': me.onBeforeDeselect,
            'selectionchange': me.onListSelectionChange,
            scope: me
        });

        return grid;
    },
    setValue: function (value, doSelect) {
        var me = this;
        me.callParent(arguments);

        var ls = me.lastSelection;
        if (ls.length > 0) {
            me._refId = ls[0].getId();
        }
        else {
            me._refId = '';
        }
    },

    //重写以下方法处理 Tree 的兼容。（TreeStore 没有的一些方法的问题。）

    findRecord: function (field, value) {
        var me = this;
        if (me._isTree) { return false; }

        return me.callParent(arguments);
    },
    loadPage: function (pageNum) {
        var me = this;

        //如果是树型表格控件，则不支持分页
        if (me._isTree) {
            //在加载第一页时，表示需要进行数据的初始化，树型 Store 开始加载数据。
            if (pageNum == 1) {
                me._view.loadData();
            }

            return false;
        }

        return me.callParent(arguments);
    },
    //树型控件的滚动条有问题，需要手动设置它的高度。
    expand: function () {
        var me = this;
        me.callParent(arguments);

        if (me._isTree) {
            var tree = me._view.getControl();
            if (!tree.height) { tree.setHeight(250); }
        }
    },

    //重写以下两个方法，在数据时把 引用实体的 id 也返回。

    getModelData: function () {
        var me = this;
        var data = me.callParent();
        me._addRefId(data);
        return data;
    },
    getSubmitData: function () {
        var me = this;
        var data = me.callParent();
        me._addRefId(data);
        return data;
    },
    _addRefId: function (data) {
        var me = this;
        if (me._refId) {
            if (!me._refProperty) me._refProperty = me.getName().replace("_Label", "");

            data[me._refProperty] = me._refId;
        }
    }
});