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

Ext.define('Rafy.view.View', {
    extend: 'Ext.util.Observable',

    statics: {
        currentId: 1
    },

    //internal
    _propertyNameInParent: null,

    constructor: function (meta) {
        this.callParent(arguments);

        var me = this;
        me._id = Rafy.view.View.currentId++;
        me._meta = meta;
        me._children = [];
        me._relations = []; // Rafy.view.RelationView
        me._model = Rafy.getModel(meta.model);
        me._isTree = me._model.isTree;

        me._commands = new Ext.util.MixedCollection();

        me.addEvents('currentChanged', 'dataChanged');
    },

    //-------------------------------------  Common -------------------------------------
    _id: 0,//当前视图的 Id，主要是为了防止同一个实体类的多个视图上的命令 id 冲突。
    _control: null,
    _meta: null,
    _model: null,
    //internal
    _isTree: false,

    getId: function () {
        /// <summary>
        /// 返回当前视图在客户端的运行时 Id
        /// </summary>
        /// <returns type="int"></returns>
        return this._id;
    },
    getMeta: function () {
        /// <summary>
        /// 返回本视图对应的客户端元数据。
        /// </summary>
        /// <returns type=""></returns>
        return this._meta;
    },
    getModel: function () {
        /// <summary>
        /// 获取本实体视图对应实体的类型
        /// </summary>
        /// <returns></returns>
        return this._model;
    },
    getIsTree: function () {
        /// <summary>
        /// 返回本视图是否是一个树型实体的视图
        /// </summary>
        /// <returns type="Boolean"></returns>
        return this._isTree;
    },
    getProxyUrl: function () {
        /// <summary>
        /// 返回本视图对应的数据网关地址。
        /// </summary>
        /// <returns type=""></returns>
        return this.getModel().getProxy().url;
    },
    getControl: function () {
        /// <summary>
        /// 返回本视图对应的界面元素。
        /// </summary>
        /// <returns type=""></returns>
        return this._control;
    },
    _setControl: function (value) {
        /// <summary>
        /// internal virtual
        /// 设置本视图对应的界面元素。
        /// </summary>
        /// <param name="value"></param>
        var old = this._control;
        if (old) { delete old.rafyView; }

        this._control = value;

        //为 value 附加一个属性，表明这个控件所对应的 rafyView。
        //目前，这个值会被 ComboList 使用到。
        if (value) { value.rafyView = this; }
    },

    //-------------------------------------  Data and Current -------------------------------------
    _current: null,
    getData: function () {
        /// <summary>
        /// abstract
        /// 返回本视图对应的所有数据。
        /// </summary>
        Rafy.markAbstract();
    },
    setData: function (value) {
        /// <summary>
        /// 设置本视图对应的所有数据。
        /// </summary>
        /// <param name="value"></param>
        Rafy.markAbstract();
    },
    getCurrent: function () {
        /// <summary>
        /// 获取当前的实体。
        /// </summary>
        /// <returns type="Rafy.data.Entity"></returns>
        return this._current;
    },
    setCurrent: function (value) {
        /// <summary>
        /// 设置当前的实体对象。
        /// </summary>
        /// <param name="value"></param>
        if (this._current != value) {
            var oldValue = this._current;
            this._current = value;

            this._onCurrentChanged(oldValue, value);
        }
    },
    _onDataChanged: function (value) {
        /// <summary>
        /// protected virtual
        /// 当数据变更时调用此方法。并发生 dataChanged 事件。
        /// </summary>
        /// <param name="value"></param>
        this.fireEvent('dataChanged', { value: value });
    },
    _onCurrentChanged: function (oldValue, value) {
        /// <summary>
        /// protected virtual
        /// 当当前实体对象变更时调用此方法。并发生 currentChanged 事件。
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="value"></param>
        this._resetChildrenData();
        this.fireEvent('currentChanged', {
            oldValue: oldValue,
            newValue: value
        });
    },
    _resetChildrenData: function () {
        /// <summary>
        /// 重新加载所有子视图的所有数据。
        /// </summary>
        var l = this._children;
        for (var i = 0; i < l.length; i++) {
            var c = l[i];
            //c.setData(null);//清空的功能，直接放到子类的 _loadDataFromParent 实现中，这里不再需要。
            c._loadDataFromParent();
        }
    },
    _loadDataFromParent: function () {
        /// <summary>
        /// protected virtual
        /// 通过当前子视图对应父视图的实体对象，加载它对应的数据。
        /// </summary>
    },

    //-------------------------------------  Parent - Children -------------------------------------
    //internal
    _parent: null,
    _children: null,// []
    getParent: function () {
        /// <summary>
        /// 获取当前视图的聚合父视图。
        /// </summary>
        /// <returns type=""></returns>
        return this._parent;
    },
    getChildren: function () {
        /// <summary>
        /// 获取当前视图的所有子视图。
        /// </summary>
        /// <returns type="Array"></returns>
        return this._children;
    },
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
    _setParent: function (value) {
        /// <summary>
        /// internal
        /// 设置当前视图的聚合父视图。
        /// </summary>
        /// <param name="value"></param>
        if (value == null) {
            if (this._parent != null) {
                this._parent._removeChild(this);
            }
        }
        else {
            value._addChild(this);
        }
    },
    _addChild: function (value) {
        /// <summary>
        /// 添加指定的视图到本视图的子视图集合中。
        /// </summary>
        /// <param name="value"></param>
        var c = this._children;
        if (!Ext.Array.contains(c, value)) { c.push(value); }
        value._parent = this;
    },
    _removeChild: function (value) {
        /// <summary>
        /// 从本视图的子视图集合中删除指定的视图。
        /// </summary>
        /// <param name="value"></param>
        if (value._parent == this) {
            Ext.Array.remove(this._children, value);
            value._parent = null;
        }
    },

    //-------------------------------------  Relations -------------------------------------
    _relations: null,
    getRelations: function () {
        /// <summary>
        /// 获取当前视图的所有关系视图。
        /// </summary>
        /// <returns type=""></returns>
        return this._relations;
    },
    findRelationView: function (name) {
        /// <summary>
        /// 查找对应名称的关系视图。
        /// </summary>
        /// <param name="name"></param>
        /// <returns type=""></returns>
        var target = Rafy.findFirst(this._relations, function (r) { return r.getName() == name; });
        if (target != null) return target.getTarget();
        return null;
    },
    _setRelation: function (relation) {
        /// <summary>
        /// 为本视图添加一个关系视图。
        /// </summary>
        /// <param name="relation"></param>
        var exist = Rafy.findFirst(this._relations, function (r) { return r.getName() == relation.getName(); });
        if (exist != null) {
            Ext.Array.remove(this._relations, exist);
        }

        relation._owner = this;
        this._relations.push(relation);
    },

    //-------------------------------------  Command -------------------------------------
    //private,mixed collection, key:cmdName, value:cmd.
    _commands: null,
    getCmdControl: function (cmdName) {
        var id = this._getCmdControlId(cmdName);
        return this._control.queryById(id);
    },
    _getCmdControlId: function (cmdName) {
        /// <summary>
        /// internal
        /// </summary>
        /// <param name="cmdName"></param>
        /// <returns type=""></returns>
        var m = this._meta;
        var name = m.model + m.viewName;
        var id = name.replace('.', '_') + '_' + cmdName + '_' + this._id;;
        return id;
    },
    getCommands: function () {
        /// <summary>
        /// 返回本视图上的所有命令。
        /// </summary>
        /// <returns type="MixedCollection">key:cmdName, value:cmd</returns>
        return this._commands;
    },
    findCmd: function (cmdType) {
        /// <summary>
        /// 通过命令类型来查找命令。
        /// </summary>
        /// <param name="cmdType">类型，可以直接通过父类型来查找子类型按钮。</param>
        /// <returns></returns>
        return Rafy.findFirst(this._commands, function (c) {
            return c instanceof cmdType;
        });
        //return this._commands.getByKey(cmdType);
    },
    _addCmd: function (cmdName, cmd) {
        /// <summary>
        /// internal
        /// 为本视图添加一个命令。
        /// </summary>
        /// <param name="cmdName"></param>
        /// <param name="cmd"></param>
        this._commands.add(cmdName, cmd);
    },

    //-------------------------------------  Readonly -------------------------------------
    _isReadonly: false,
    getIsReadonly: function () { return this._isReadonly; },
    setIsReadonly: function (value) {
        if (this._isReadonly != value) {
            this._onIsReadonlyChanged(value);
            this._isReadonly = value;
        }
    },
    _onIsReadonlyChanged: function (value) {
        /// <summary>
        /// protected virtual
        /// IsReadonly 属性变化时调用此方法。
        /// </summary>
        /// <param name="value"></param>
    },

    //-------------------------------------  DTO -------------------------------------
    save: function (opt) {
        /// <summary>
        /// 保存当前列表中“修改”的数据到服务端
        /// </summary>
        /// <param name="opt">
        /// withChildren: false，是否需要把聚合子视图的数据也提交到服务端。
        /// model: 保存的实体类型。默认自动设置为本视图的实体。
        /// callback: null。回调函数。
        /// </param>
        var me = this;

        opt = opt || {};
        if (Ext.isFunction(opt)) { opt = { callback: opt }; }
        opt = Ext.apply({
            withChildren: false,
            model: me._meta.model
        }, opt);

        opt._changeSetData = this.serializeData(opt.withChildren);

        if (!opt._changeSetData.isEmpty()) {
            var outerCallback = opt.callback;
            opt.callback = function (res, success) {
                me.getControl().setLoading(false);

                if (outerCallback) { outerCallback(res); }
            };

            Rafy.data.EntityRepository.save(opt);

            me.getControl().setLoading(true);
        }
    },
    serializeData: function (withChildren) {
        /// <summary>
        /// 把当前视图中的数据转换为可以直接序列化并提交到服务端的 json 对象
        /// </summary>
        /// <param name="withChildren">
        /// 默认为: false, 是否需要转换聚合子视图中的数据。
        /// </param>
        /// <returns></returns>

        var data = this.getData();

        var dto = Rafy.data.Serializer.serialize(data, !!withChildren);

        return dto;
    },
    _getPIdField: function (entity) {
        /// <summary>
        /// protected
        /// 获取树型父 Id 对应的字符。
        /// </summary>
        /// <param name="entity"></param>
        /// <returns type="Ext.data.Field" />
        var fields = Rafy.data.EntityRepository.getPersistFields(this._meta.model);
        var pIdField = Rafy.first(fields, function (f) {
            return f.name == "TreePId" || f.name == "PId";
        });
        return pIdField;
    }
});