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

Ext.define('Oea.view.ListView', {
    extend: 'Oea.view.View',

    //private
    _treeStoreInited: false,
    //最后一次使用的数据加载参数
    _lastDataArgs: null,

    //internal
    _pagingBar: null,

    isListView: true,

    constructor: function (meta) {
        this.callParent(arguments);

        this.addEvents('itemCreated');
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

                //如果原来有分组信息，则新的 store 也需要分组。注意：只支持一级分组。
                var g = data.groupers;
                if (g.getCount() > 0) {
                    var gi = g.getAt(0);
                    value.group(gi.property, gi.direction);
                }

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

    //------------------------------------- Relations -------------------------------------
    getConditionView: function () {
        return this.findRelationView(Oea.view.RelationView.condition);
    },
    getNavigationView: function () {
        return this.findRelationView(Oea.view.RelationView.navigation);
    },

    //------------------------------------- Selection -------------------------------------
    getSelection: function () {
        return this._getSelectionModel().getSelection();
    },
    _getSelectionModel: function () {
        return this.getControl().getSelectionModel();
    },

    //------------------------------------- Readonly -------------------------------------
    //protected override
    onIsReadonlyChanged: function (value) {
        var editing = this._getEditing();
        if (value) {
            editing.on('beforeedit', this._readonlyHandler, this);
        }
        else {
            editing.un('beforeedit', this._readonlyHandler, this);
        }
    },
    _readonlyHandler: function (editor, e, opt) {
        e.cancel = true;
    },

    //------------------------------------- 以下是方便 CRUD 的方法 -------------------------------------
    //在同级添加一个结点
    addNew: function () {
        /// <summary>
        /// 为当前列表视图添加一个新的对象，同时设置好它的关系。
        /// </summary>
        /// <returns type="Oea.data.Entity">返回新加的实体对象</returns>

        var result = null;
        var me = this;
        if (!me._isTree) {
            var store = me.getData();
            var models = store.add({});
            //            store.insert(0, {}); //调用这个方法可以自动设置父外键
            result = models[0];
        }
        else {
            var parent;
            var s = me.getSelection();
            if (s[0]) parent = s[0].parentNode;
            parent = parent || me._getTreeRoot();

            if (parent.isLeaf()) { parent.set('leaf', false); }
            result = parent.appendChild(me._createTreeNode(parent));
            if (!parent.isExpanded()) { parent.expand(); }
        }

        this.fireEvent('itemCreated', { item: result });

        return result;
    },
    //为当前选择的树型控件添加一个子结点
    insertNewChild: function () {
        /// <summary>
        /// 为当前列表视图插入一个新的对象，同时设置好它的关系。
        ///
        /// 注意，此方法只能在树型视图中被调用。
        /// </summary>
        /// <returns type="Oea.data.Entity">返回新加的实体对象</returns>
        var me = this;
        if (me._isTree) {
            var s = me.getSelection();
            var parent = s[0] || me._getTreeRoot();

            if (parent.isLeaf()) { parent.set('leaf', false); }
            var model = parent.insertChild(0, me._createTreeNode(parent));
            if (!parent.isExpanded()) { parent.expand(); }

            this.fireEvent('itemCreated', { item: model });

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
        /// <summary>
        /// 实现列表类的数据序列化逻辑：
        /// 把实体列表仓库转换为 ListChangeSet 对象。
        /// </summary>
        /// <param name="opt"></param>
        /// <returns></returns>
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

    reloadData: function () {
        /// <summary>
        /// 使用最后一次使用的加载参数，重新加载整个列表的数据。
        /// </summary>
        this.loadData(this._lastDataArgs);
    },
    loadData: function (args) {
        /// <summary>
        /// 为这个视图异步加载数据
        /// </summary>
        /// <param name="args">
        /// criteria: 使用这个参数来进行数据查询。
        /// callback: 加载完成后的回调。（目前此参数只在非树型列表时有用！！！ 待修正。）
        /// </param>
        args = args || this._lastDataArgs || {};
        if (Ext.isFunction(args)) {
            args = { callback: args };
        }
        this._lastDataArgs = args;

        var store = this.getData();

        if (args.criteria) {
            Oea.data.EntityRepository.filterByCriteria(store, args.criteria);
        }

        //加载数据，并清空当前选择项。
        if (this._isTree) {

            //由于创建时配置了 root 为已经加载来防止 treePanel 的自动加载数据，
            //所以这里在第一次查询时，需要把该值设置为 false。
            if (!this._treeStoreInited) {
                this._treeStoreInited = true;

                var root = store.getRootNode();
                root.set('loaded', false);
            }

            this.setCurrent(null);
            store.load();
        }
        else {
            store.rejectChanges();

            var me = this;
            store.load(function () {
                me.setCurrent(null);
                if (args.callback) args.callback(arguments);
            });
        }

        //        store.getRemovedRecords().length = 0;
    },

    //------------------------------------- Tree Operations -------------------------------------
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

    //------------------------------------- Edit -------------------------------------
    startEdit: function (entity, columnHeader) {
        this._getEditing().startEdit(entity, columnHeader);
        //------------------------------------- Tree Operations -------------------------------------
        //------------------------------------- Tree Operations -------------------------------------
        //------------------------------------- Tree Operations -------------------------------------
        //------------------------------------- Tree Operations -------------------------------------
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
    },

    _getEditing: function () {
        var res = this.getControl().getPlugin('editingPlugin');
        return res;
    }
});