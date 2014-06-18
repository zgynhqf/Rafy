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

Ext.define('Rafy.view.ListView', {
    extend: 'Rafy.view.View',

    //private
    _treeStoreInited: false,

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
        var control = me.getControl();
        var data = control.getStore();
        if (data != value) {
            //当传入的数据集为空引用时，需要创建一个新的空数据集。
            if (value == null) {
                value = Rafy.data.EntityRepository.createStore({
                    model: this.getModel()
                });
            }
            //如果原来有分组信息，则新的 store 也需要分组。注意：只支持一级分组。
            var g = data.groupers;
            if (g.getCount() > 0) {
                var gi = g.getAt(0);
                value.group(gi.property, gi.direction);
            }

            //绑定到控件及分页控件上。
            control.reconfigure(value);
            if (me._pagingBar) {
                me._pagingBar.bind(value);
            }

            me._onDataChanged(value);

            me.setCurrent(null);
        }
    },
    _setControl: function (value) {
        /// <summary>
        /// internal override
        /// 设置视图对应的控件。
        /// </summary>
        /// <param name="value"></param>
        this.callParent(arguments);

        if (value != null) {
            value.on('select', this._onControlSelectionChanged, this);
        }
    },
    _loadDataFromParent: function () {
        /// <summary>
        /// protected override
        /// 通过当前子视图对应父视图的实体对象，加载它对应的数据。
        /// </summary>
        var me = this,
            pName = me._propertyNameInParent,
            parent = me._parent;

        if (pName && parent) {
            var parentEntity = parent.getCurrent();
            if (parentEntity) {
                //获取聚合子集合，并设置为本视图的数据源。
                var children = parentEntity[pName]();

                //直接变更数据源。这时会发生 dataChanged 事件。
                //但是如果是第一次加载，那么此时数据源中还没有数据，需要等待异步加载完成后，数据才到达客户端。
                this.setData(children);

                //此字段用于判断该 Store 是否已经加载过数据。
                if (!this._getHasDataLoaded(children)) {
                    //在父实体在 Id 是正数时，才表示父对象已经在服务器端有数据了，这时进行加载。
                    if (parentEntity.getId() > 0) {
                        children.load({
                            scope: this,
                            callback: function (records, operation, success) {
                                this._setHasDataLoaded(children, success);
                            }
                        });
                    }
                }

                return;
            }
        }

        //如果没有新的数据源，则需要把旧的数据源清空。
        me.setData(null);
    },
    _getHasDataLoaded: function (store) {
        /// <summary>
        /// 判断本视图对应的数据源是否已经加载过数据。
        /// 
        /// 一个视图的数据源只会主动加载一次数据。
        /// </summary>
        /// <returns type="Boolean"></returns>
        return store._hasLoaded;
    },
    _setHasDataLoaded: function (store, value) {
        store._hasLoaded = value;
    },

    //Current
    //    getCurrent: function () {
    //        var selection = this.getSelection();
    //        if (selection.length > 0) { return selection[0]; }
    //        return null;
    //    },
    //    setCurrent: function (entity) {
    //        var sm = this.getSelectionModel();
    //        sm.select(entity);
    //    },
    _onControlSelectionChanged: function (sm, record) {
        this._OnControlStarted = true;
        this.setCurrent(record);
        delete this._OnControlStarted;
    },
    _onCurrentChanged: function (oldValue, value) {
        if (!this._OnControlStarted) {
            var sm = this.getSelectionModel();
            if (value != null) {
                sm.select(value);
            }
            else {
                sm.deselectAll();
            }
        }

        this.callParent(arguments);
    },

    //-------------------------------------  Relations -------------------------------------
    getConditionView: function () {
        return this.findRelationView(Rafy.view.RelationView.condition);
    },
    getNavigationView: function () {
        return this.findRelationView(Rafy.view.RelationView.navigation);
    },

    //-------------------------------------  Selection -------------------------------------
    getSelection: function () {
        /// <summary>
        /// 获取列表中已经选择的实体列表。
        /// </summary>
        /// <returns type="Rafy.data.Entity[]"></returns>
        return this.getSelectionModel().getSelection();
    },
    getSelectionModel: function () {
        /// <summary>
        /// 获取列表控件对应的选择控制器。
        /// </summary>
        /// <returns type="Ext.selection.Model"></returns>
        return this.getControl().getSelectionModel();
    },

    //-------------------------------------  Readonly -------------------------------------
    //protected override
    _onIsReadonlyChanged: function (value) {
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

    //-------------------------------------  以下是方便 CRUD 的方法 -------------------------------------
    //最后一次使用的数据加载参数
    _lastDataArgs: null,

    addNew: function () {
        /// <summary>
        /// 为当前列表视图添加一个新的对象，同时设置好它的关系。
        /// 
        /// 在同级添加一个结点
        /// </summary>
        /// <returns type="Rafy.data.Entity">返回新加的实体对象</returns>

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
        /// <returns type="Rafy.data.Entity">返回新加的实体对象</returns>
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
                Rafy.each(selection, function (i) {
                    i.remove();
                    //                    i.expand(true, function () {
                    //                        me._removeTreeNode(i);
                    //                    });
                    //                    _removeTreeNode: function (item) {
                    //                        var me = this;
                    //                        Rafy.each(item.childNodes, function (i) { me._removeTreeNode(i); });
                    //                        item.remove();
                    //                    },
                });
            }
        }
        return selection;
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
        /// method：string； 如果定义了此参数，则使用服务端仓库中对应的方法名来进行查询。
        /// params：[]；如果定义了 method 参数，则此参数用于指定对应方法的参数列表。参数的顺序必须与服务端定义的参数一致。
        /// 
        /// criteria: 使用这个参数来进行数据查询。
        /// 
        /// callback: 加载完成后的回调。（目前此参数只在非树型列表时有用！！！ 待修正。）
        /// </param>
        args = args || this._lastDataArgs || {};
        if (Ext.isFunction(args)) {
            args = { callback: args };
        }
        this._lastDataArgs = args;

        var store = this.getData();

        if (args.method) {
            Rafy.data.EntityRepository.filterByMethod(store, args.method, args.params);
        }
        else if (args.criteria) {
            Rafy.data.EntityRepository.filterByCriteria(store, args.criteria);
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
            this._setHasDataLoaded(store, true);
            store.load();
        }
        else {
            store.rejectChanges();

            var me = this;
            store.load(function () {
                me.setCurrent(null);
                me._setHasDataLoaded(store, true);
                if (args.callback) args.callback(arguments);
            });
        }

        //        store.getRemovedRecords().length = 0;
    },

    //-------------------------------------  Tree Operations -------------------------------------
    expandSelection: function () {
        var me = this;
        if (me._isTree) {
            var s = me.getSelection();
            if (!s.length) {
                s = me._getTreeRootNodes();
            }
            Rafy.each(s, function (i) { i.expand(true); });
        }
    },
    collapseSelection: function () {
        var me = this;
        if (me._isTree) {
            var s = me.getSelection();
            if (!s.length) {
                s = me._getTreeRootNodes();
            }
            Rafy.each(s, function (i) { i.collapse(true); });
        }
    },
    _getTreeRoot: function () {
        return this.getData().getRootNode();
    },
    _getTreeRootNodes: function () {
        return this._getTreeRoot().childNodes;
    },

    //-------------------------------------  Edit -------------------------------------
    startEdit: function (entity, columnHeader) {
        /// <summary>
        /// 开始编辑指定实体对应行的指定列。
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="columnHeader"></param>
        this._getEditing().startEdit(entity, columnHeader);
    },
    locateDefault: function () {
        /// <summary>
        /// 定位到第一行。
        /// </summary>
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