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

Ext.define('Oea.view.View', {
    extend: 'Ext.util.Observable',

    statics: {
        currentId: 1
    },

    //private
    _id: 0,//当前视图的 Id，主要是为了防止同一个实体类的多个视图上的命令 id 冲突。
    _meta: null,
    _control: null,
    _current: null,
    _relations: null,
    _isActive: true,
    _parent: null,
    _children: null,// []

    //protected
    _isTree: false,

    //internal
    _propertyNameInParent: null,

    $model: null, //用于调试

    constructor: function (meta) {
        var me = this;
        me._id = Oea.view.View.currentId++;
        me._meta = meta;
        me._children = [];
        me._relations = []; // Oea.view.RelationView
        me._isTree = meta.isTree;
        me.$model = meta.model;

        me._commands = new Ext.util.MixedCollection();

        me.addEvents('currentChanged', 'dataChanged');
    },

    //------------------------------------- Common Properties -------------------------------------
    getId: function () {
        /// <summary>
        /// 返回当前视图在客户端的运行时 Id
        /// </summary>
        /// <returns type="int"></returns>
        return this._id;
    },
    getMeta: function () { return this._meta; },
    getModel: function () {
        /// <summary>
        /// 获取本实体视图对应实体的类型名称
        /// </summary>
        /// <returns></returns>
        return this._meta.model;
    },
    getModelClass: function () {
        /// <summary>
        /// 获取本实体视图对应实体的类型
        /// </summary>
        /// <returns></returns>
        return Ext.ModelManager.getModel(this._meta.model);
    },
    getIsTree: function () { return this._isTree; },
    getProxyUrl: function () {
        return this.getModelClass().getProxy().url;
    },
    getControl: function () { return this._control; },
    getIsActive: function () { return this._isActive; },
    //internal virtual
    _setControl: function (value) {
        var old = this._control;
        if (old) { delete old.oeaView; }

        this._control = value;

        //为 value 附加一个属性，表明这个控件所对应的 oeaView。
        //目前，这个值会被 ComboList 使用到。
        if (value) { value.oeaView = this; }
    },

    //------------------------------------- Data and Current -------------------------------------
    getData: function () { Oea.markAbstract(); },
    setData: function (value) { Oea.markAbstract(); },
    _onDataChanged: function (value) { this.fireEvent('dataChanged', { value: value }); },
    getCurrent: function () { return this._current; },
    setCurrent: function (value) {
        if (this._current != value) {
            var oldValue = this._current;
            this._current = value;

            this._onCurrentChanged(oldValue, value);
        }
    },
    _onCurrentChanged: function (oldValue, value) {
        this._resetChildrenData();
        this.fireEvent('currentChanged', { oldValue: oldValue, mewValue: value });
    },
    _resetChildrenData: function () {
        var l = this._children;
        for (var i = 0; i < l.length; i++) {
            var c = l[i];
            c.setData(null);
            if (c._isActive) {
                c._loadDataFromParent();
            }
        }
    },
    _loadDataFromParent: function () {
        var me = this;
        if (me._parent && me._propertyNameInParent && me._parent._current) {
            var children = me._parent._current[me._propertyNameInParent]();
            this.setData(children);
            children.load();
        }
    },

    //------------------------------------- Parent - Children -------------------------------------
    getParent: function () { return this._parent; },
    setParent: function (value) {
        if (value == null) {
            if (this._parent != null) {
                this._parent.removeChild(this);
            }
        }
        else {
            value.addChild(this);
        }
    },
    getChildren: function () { return this._children; },
    findChild: function (model, recur) {
        /// <summary>
        /// 查找某个子实体对应的视图
        /// </summary>
        /// <param name="model"></param>
        /// <param name="recur">是否迭归查找更下层的子。</param>
        /// <returns></returns>
        for (var i = 0; i < this._children.length; i++) {
            var c = this._children[i];
            if (c._meta.model == model) { return c; }

            if (recur) {
                c = c.findChild(model);
                if (c != null) return c;
            }
        }

        return null;
    },
    addChild: function (value) {
        var c = this._children;
        if (!Ext.Array.contains(c, value)) { c.push(value); }
        value._parent = this;
    },
    removeChild: function (value) {
        if (value._parent == this) {
            Ext.Array.remove(this._children, value);
            value._parent = null;
        }
    },

    //------------------------------------- Relations -------------------------------------
    getRelations: function () { return this._relations; },
    setRelation: function (relation) {
        var exist = Oea.findFirst(this._relations, function (r) { return r.getName() == relation.getName(); });
        if (exist != null) {
            Ext.Array.remove(this._relations, exist);
        }

        relation._owner = this;
        this._relations.push(relation);
    },
    findRelationView: function (name) {
        var target = Oea.findFirst(this._relations, function (r) { return r.getName() == name; });
        if (target != null) return target.getTarget();
        return null;
    },

    //------------------------------------- Command -------------------------------------
    getCmdControl: function (cmdName) {
        var id = this._getCmdControlId(cmdName);
        return this._control.queryById(id);
    },
    //internal
    _getCmdControlId: function (cmdName) {
        var m = this._meta;
        var name = m.model + m.viewName;
        var id = name.replace('.', '_') + '_' + cmdName + '_' + this._id;;
        return id;
    },
    //private,mixed collection, key:cmdName, value:cmd.
    _commands: null,
    getCommands: function () { return this._commands; },
    findCmd: function (cmdType) {
        /// <summary>
        /// 通过命令类型来查找命令
        /// </summary>
        /// <param name="cmdType">类型，可以直接通过父类型来查找子类型按钮。</param>
        /// <returns></returns>
        return Oea.findFirst(this._commands, function (c) {
            return c instanceof cmdType;
        });
        //return this._commands.getByKey(cmdType);
    },
    //internal
    _addCmd: function (cmdName, cmd) {
        this._commands.add(cmdName, cmd);
    },

    //------------------------------------- Readonly -------------------------------------
    _isReadonly: false,
    getIsReadonly: function () { return this._isReadonly; },
    setIsReadonly: function (value) {
        if (this._isReadonly != value) {
            this.onIsReadonlyChanged(value);
            this._isReadonly = value;
        }
    },
    //protected virtual
    onIsReadonlyChanged: function (value) { },

    //------------------------------------- 数据 -------------------------------------
    save: function (opt) {
        /// <summary>
        /// 保存当前列表中“修改”的数据到服务端
        /// </summary>
        /// <param name="opt">
        /// withUnchanged: false。是否需要把未更改的数据也提交到服务端。
        /// withChildren: false。是否需要把聚合子视图的数据也提交到服务端。
        /// autoLoad: true。是否在提交后自动重新加载数据。
        /// isTree: 保存的实体类型是否为树型。默认自动设置为本视图的实体。
        /// model: 保存的实体类型。默认自动设置为本视图的实体。
        /// callback: null。回调函数。
        /// </param>
        var me = this;

        opt = opt || {};
        if (Ext.isFunction(opt)) { opt = { callback: opt }; }
        opt = Ext.apply({
            withUnchanged: false,
            withChildren: false,
            isTree: me._isTree,
            model: me._meta.model,
            autoLoad: true
        }, opt);

        opt._changeSetData = me.serializeData(opt);

        if (Oea.data.ListChangeSet._needUpdateToServer(opt._changeSetData, me._isTree)) {
            var outerCallback = opt.callback;
            opt.callback = function (res, success) {
                me.getControl().setLoading(false);

                if (outerCallback) { outerCallback(res); }

                if (success && opt.autoLoad) { me.loadData(); }
            };

            Oea.data.EntityRepository.save(opt);

            me.getControl().setLoading(true);
        }
    },
    //virtual
    loadData: Ext.emptyFn,
    serializeData: function (opt) {
        /// <summary>
        /// 把当前视图中的数据转换为可以直接序列化并提交到服务端的 json 对象
        /// </summary>
        /// <param name="opt">
        /// withUnchanged: false, 是否需要转换化未变更的数据。
        /// withChildren: false, 是否需要转换聚合子视图中的数据。
        /// </param>
        /// <returns></returns>

        opt = Ext.apply({
            withUnchanged: false,
            withChildren: false
        }, opt);

        var data = this._serializeData(opt);
        return data;
    },
    //protected abstract
    _serializeData: Oea.markAbstract,
    //protected
    _serailizeChildrenData: function (curItemData, option) {
        Oea.each(this._children, function (c) {
            curItemData[c._propertyNameInParent] = c._serializeData(option);
        });
    },
    //protected
    _getPersistData: function (item, option) {
        var data = {}, me = this;

        option = Ext.apply({ withChildren: false }, option);

        //属性需要可以保存，并且是大写
        var fields = Oea.data.EntityRepository.getPersistFields(item);
        Oea.each(fields, function (f) { data[f.name] = item.get(f.name); });

        if (option.withChildren) {
            Oea.each(me._children, function (c) {
                data[c._propertyNameInParent] = c._serializeData(option);
            });
        }

        return data;
    },
    //protected
    _getPIdField: function (item) {
        /// <summary>
        ///
        /// </summary>
        /// <param name="item"></param>
        /// <returns type="Ext.data.Field" />
        var fields = Oea.data.EntityRepository.getPersistFields(this._meta.model);
        var pIdField = Oea.first(fields, function (f) {
            return f.name == "TreePId" || f.name == "PId";
        });
        return pIdField;
    }
});