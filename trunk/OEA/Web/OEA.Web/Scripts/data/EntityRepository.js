Ext.define('Oea.data.EntityRepository', {
    singleton: true,

    query: function (opt) {
        opt = Ext.apply({
            pageSize: 25,
            isTree: false,
            storeConfig: {}
        }, opt)

        var store = this.createStore(opt);

        if (opt.id) {
            opt.criteria = Ext.create('OEA.GetByIdCriteria', { Id: opt.id });
        }
        if (opt.criteria) {
            this.filterByCriteria(store, opt.criteria);
        }

        store.load({ callback: opt.callback });
    },

    /// <returns type="Ext.data.Field[]" />
    getPersistFields: function (model) {
        var res = [];

        var fields;
        //注意：fields 是私有变量
        if (model.isModel) { fields = model.fields; }
        else { fields = Ext.ModelManager.getModel(model).prototype.fields; }

        //属性需要可以保存，并且是小写。
        fields.each(function (field) {
            if (field.persist) {
                var name = field.name;
                var fc = name[0];
                if (fc >= 'A' && fc <= 'Z') {
                    res.push(field);
                }
            }
        });

        return res;
    },

    //opt: isTree, model, callback, withUnchanged, entity/store/_changeSetData(internal),
    save: function (opt) {
        opt = Ext.apply({
            isTree: false,
            withUnchanged: false
        }, opt);
        if (!opt.model) return;

        var csApi = Oea.data.ListChangeSet;

        var data = null;
        if (opt.entity) {
            data = csApi._getItemData(opt.entity, opt.isTree, opt.withUnchanged);
        }
        else if (opt.store) {
            data = csApi._getChangeSetData(opt.store, opt.isTree, opt.withUnchanged);
        }
        else {
            data = opt._changeSetData;
        }

        if (!csApi._needUpdateToServer(data, opt.isTree)) {
            if (opt.callback) {
                res = { success: true }
                opt.callback(res, true);
            }
            return;
        }
        var o = Ext.encode(opt._changeSetData);

        var model = Ext.ModelManager.getModel(opt.model);
        var proxyUrl = model.getProxy().url;
        var ajaxCfg = {
            url: proxyUrl,
            params: {
                action: 'save',
                entityList: o
            },
            callback: function (options, success, response) {
                if (opt.callback) {
                    var res = null;
                    if (success) {
                        res = Ext.decode(response.responseText);
                    }
                    else {
                        res = {
                            success: false,
                            msg: response.responseText
                        };
                    }
                    opt.callback(res, success);
                }
            }
        };
        Ext.Ajax.request(ajaxCfg);
    },

    filterByCriteria: function (store, criteria) {
        var f = store.filters;
        f.clear();
        f.addAll([new Ext.util.Filter({
            property: '_useCriteriaType', value: Ext.getClassName(criteria)
        }), new Ext.util.Filter({
            property: 'criteria', value: criteria.data
        })]);
    },

    //opt: model, isTree, storeConfig
    createStore: function (opt) {
        opt = Ext.apply({
            isTree: false
        }, opt)

        var storeConfig = Ext.apply({ model: opt.model }, opt.storeConfig);

        if (opt.isTree) {
            storeConfig.root = {
                loaded: true
            }; //如果没有 root，则会自动发起一次查询。
        }
        else {
            storeConfig.remoteFilter = true;
            //            if (opt.groupBy) { storeConfig.groupField = opt.groupBy; }
        }

        var storeName = opt.isTree ? 'Ext.data.TreeStore' : 'Ext.data.Store';
        var store = Ext.create(storeName, storeConfig);

        return store;
    }
});