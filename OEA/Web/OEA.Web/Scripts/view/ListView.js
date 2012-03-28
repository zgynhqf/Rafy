Ext.define('Oea.view.ListView', {
    extend: 'Oea.view.View',

    //private
    _treeStoreInited: false,

    //internal 
    _pagingBar: null,

    constructor: function (meta) {
        this.callParent(arguments);
    },

    getData: function () {
        return this.getControl().getStore();
    },
    setData: function (value) {
        var me = this;
        var c = me.getControl();
        var data = c.getStore();
        if (data != value) {
            if (value != null) {
                c.reconfigure(value);
                if (me._pagingBar) { me._pagingBar.bind(value); }
            }
            else {
                data.loadData([]);
            }
            me._onDataChanged(value);

            me.setCurrent(null);
        }
    },
    _setControl: function (value) {
        this.callParent(arguments);

        if (value != null) {
            this.getControl().on('select', this._onControlSelectionChanged, this);
        }
    },

    //Current
    //    getCurrent: function () {
    //        var selection = this.getSelection();
    //        if (selection.length > 0) { return selection[0]; }
    //        return null;
    //    },
    //    setCurrent: function (entity) {
    //        var sm = this._getSelectionModel();
    //        sm.select(entity);
    //    },
    _onControlSelectionChanged: function (sm, record) {
        this._OnControlStarted = true;
        this.setCurrent(record);
        delete this._OnControlStarted;
    },
    _onCurrentChanged: function (oldValue, value) {
        if (!this._OnControlStarted) {
            var sm = this._getSelectionModel();
            if (value != null) {
                sm.select(value);
            }
            else {
                sm.deselectAll();
            }
        }

        this.callParent(arguments);
    },

    //Selection
    getSelection: function () {
        return this._getSelectionModel().getSelection();
    },
    _getSelectionModel: function () {
        return this.getControl().getSelectionModel();
    },

    //以下是方便 CRUD 的方法

    //在同级添加一个结点
    addNew: function () {
        var me = this;
        if (!me._isTree) {
            var store = me.getData();
            var models = store.add({});
            //            store.insert(0, {}); //调用这个方法可以自动设置父外键
            return models[0];
        }
        else {
            var parent;
            var s = me.getSelection();
            if (s[0]) parent = s[0].parentNode;
            parent = parent || me._getTreeRoot();

            if (parent.isLeaf()) { parent.set('leaf', false); }
            var model = parent.appendChild(me._createTreeNode(parent));
            if (!parent.isExpanded()) { parent.expand(); }
            return model;
        }
    },
    //为当前选择的树型控件添加一个子结点
    insertNewChild: function () {
        var me = this;
        if (me._isTree) {
            var s = me.getSelection();
            var parent = s[0] || me._getTreeRoot();

            if (parent.isLeaf()) { parent.set('leaf', false); }
            var model = parent.insertChild(0, me._createTreeNode(parent));
            if (!parent.isExpanded()) { parent.expand(); }
            return model;
        }
    },
    _createTreeNode: function (parent) {
        var n = { leaf: true };
        var pIdField = this._getPIdField().name;
        n[pIdField] = parent.getId();
        return n;
    },
    removeSelection: function () {
        var me = this;
        var selection = me.getSelection();
        if (selection.length > 0) {
            if (!me._isTree) {
                me.getData().remove(selection);
            }
            else {
                //删除树结点时，在客户端展开全部再删除。
                Oea.each(selection, function (i) {
                    i.remove();
                    //                    i.expand(true, function () {
                    //                        me._removeTreeNode(i);
                    //                    });
                    //                    _removeTreeNode: function (item) {
                    //                        var me = this;
                    //                        Oea.each(item.childNodes, function (i) { me._removeTreeNode(i); });
                    //                        item.remove();
                    //                    },
                });                       
            }
        }
        return selection;
    },

    _serializeData: function (opt) {
        var me = this;

        var api = Oea.data.ListChangeSet;
        var data = api._getChangeSetData(me.getData(), me._isTree, opt.withUnchanged);

        if (opt.withChildren) {
            var c = me.getCurrent();
            if (c != null) {
                var id = c.getId();
                api._eachItemInData(data, me._isTree, function (item) {
                    if (item[Oea._IdPropertyName] == id) {
                        me._serailizeChildrenData(item, opt);
                        return false;
                    }
                });
            }
        }

        return data;
    },

    loadData: function (args) {
        var me = this;

        args = args || {};
        if (Ext.isFunction(args)) {
            args = { callback: args };
        }

        var store = me.getData();

        if (args.criteria) {
            Oea.data.EntityRepository.filterByCriteria(store, args.criteria);
        }

        //加载数据，并清空当前选择项。
        if (me._isTree) {

            //由于创建时配置了 root 为已经加载来防止 treePanel 的自动加载数据，
            //所以这里在第一次查询时，需要把该值设置为 false。
            if (!me._treeStoreInited) {
                me._treeStoreInited = true;

                var root = store.getRootNode();
                root.set('loaded', false);
            }

            me.setCurrent(null);
            store.load();
        }
        else {
            store.rejectChanges();

            store.load(function () {
                me.setCurrent(null);
                if (args.callback) args.callback(arguments);
            });
        }

        //        store.getRemovedRecords().length = 0;
    },

    //Tree Operations

    expandSelection: function () {
        var me = this;
        if (me._isTree) {
            var s = me.getSelection();
            if (!s.length) {
                s = me._getTreeRootNodes();
            }
            Oea.each(s, function (i) { i.expand(true); });
        }
    },
    collapseSelection: function () {
        var me = this;
        if (me._isTree) {
            var s = me.getSelection();
            if (!s.length) {
                s = me._getTreeRootNodes();
            }
            Oea.each(s, function (i) { i.collapse(true); });
        }
    },
    _getTreeRoot: function () {
        return this.getData().getRootNode();
    },
    _getTreeRootNodes: function () {
        return this._getTreeRoot().childNodes;
    },

    startEdit: function (entity, columnHeader) {
        var grid = this.getControl();
        grid.getPlugin('editingPlugin').startEdit(entity, columnHeader);
    },
    locateDefault: function () {
        var me = this;

        if (me._isTree) {
            alert("树型的 locateDefault 还没有实现。");
        }
        else {
            var list = me.getData();
            if (list.getCount() > 0) {
                var item = list.getAt(0);
                me.setCurrent(item);
            }
        }
    }
});