Ext.define('Oea.view.View', {
    extend: 'Ext.util.Observable',

    //private 
    _meta: null,
    _control: null,
    _current: null,
    _parent: null,
    _children: null,
    _relations: null,
    _isActive: true,

    //protected
    _isTree: false,

    _propertyNameInParent: null, //internal
    $model: null, //用于调试

    constructor: function (meta) {
        var me = this;
        me._meta = meta;
        me._children = [];
        me._relations = []; // Oea.view.RelationView
        me._isTree = meta.isTree;
        me.$model = meta.model;

        me.addEvents('currentChanged', 'dataChanged');
    },

    //--------------------Common Properties----------------------
    getMeta: function () { return this._meta; },
    getModel: function () {
        return Ext.ModelManager.getModel(this._meta.model);
    },
    getIsTree: function () { return this._isTree; },
    getProxyUrl: function () {
        return this.getModel().getProxy().url;
    },
    getControl: function () { return this._control; },
    //protected
    _setControl: function (value) { this._control = value; },

    //IsActive
    getIsActive: function () { return this._isActive; },

    //--------------------Data and Current----------------------
    getData: function () { Oea.markAbstract(); },
    setData: function (value) { Oea.markAbstract(); },
    _onDataChanged: function (value) { this.fireEvent('dataChanged'); },
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
        this.fireEvent('currentChanged', oldValue, value);
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

    //--------------------Parent - Children----------------------
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

    //--------------------Relations----------------------
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

    //--------------------Command----------------------
    getCmdControl: function (cmdName) {
        var id = this._getCmdControlId(cmdName);
        return this._control.queryById(id);
    },
    //internal
    _getCmdControlId: function (cmdName) {
        var m = this._meta;
        var name = m.model + m.viewName;
        var id = name.replace('.', '_') + "_" + cmdName;
        return id;
    },

    //--------------------数据----------------------

    //保存当前列表中“修改”的数据到服务端
    save: function (opt) {
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

        opt._changeSetData = me._serializeData(opt);

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
    //protected
    _serializeData: Oea.markAbstract,
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
    /// <returns type="Ext.data.Field" />
    _getPIdField: function (item) {
        var fields = Oea.data.EntityRepository.getPersistFields(this._meta.model);
        var pIdField = Oea.first(fields, function (f) {
            return f.name == "TreePId" || f.name == "PId";
        });
        return pIdField;
    }
});