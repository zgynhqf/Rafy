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

Ext.define('Oea.data.EntityRepository', {
    singleton: true,

    query: function (opt) {
        /// <summary>
        /// 通过一定的查询条件来查询实体列表
        /// </summary>
        /// <param name="opt">
        /// model : 必填。
        /// pageSize: 25
        /// isTree: false
        /// storeConfig : Ext store config.
        /// id: 如果定义了此参数，则按照 id 查询某个指定的实体
        /// criteria：如果定义了此参数，则按照此参数在服务端进行过滤。
        /// callback: 用于查询完成后的回调函数。参数与 store.load 的回调一致。
        /// </param>

        opt = Ext.apply({
            pageSize: 25,
            isTree: false,
            storeConfig: {}
        }, opt);

        var store = this.createStore(opt);

        if (opt.id) {
            opt.criteria = Ext.create('OEA.GetByIdCriteria', { Id: opt.id });
        }
        if (opt.criteria) {
            this.filterByCriteria(store, opt.criteria);
        }

        store.load({ callback: opt.callback });
    },

    getPersistFields: function (model) {
        /// <summary>
        /// 获取某个实体可以被存储的字段列表
        /// </summary>
        /// <param name="model"></param>
        /// <returns type="Ext.data.Field[]"></returns>
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

    save: function (opt) {
        /// <summary>
        /// 保存实体/实体列表
        /// </summary>
        /// <param name="opt">
        /// model: 必需的。
        /// isTree: 是否为树型实体，默认为 false。
        /// callback: 回调函数。
        /// withUnchanged: 是否需要把未更改的值也提交到服务端，默认为 false。
        /// entity/store/_changeSetData(internal): 这三个只需要设置其中一个就可以了。
        /// </param>

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
                res = { success: true };
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
        /// <summary>
        /// 为某个指定的 store 设置过滤条件
        /// </summary>
        /// <param name="store">要过滤的 ext store</param>
        /// <param name="criteria">过滤参数，应该是一个 Criteria 类的实例。</param>
        var f = store.filters;
        f.clear();
        f.addAll([new Ext.util.Filter({
            property: '_useCriteriaType', value: Ext.getClassName(criteria)
        }), new Ext.util.Filter({
            property: 'criteria', value: criteria.data
        })]);
    },

    createStore: function (opt) {
        /// <summary>
        /// 为指定的实体创建一个 Ext 仓库
        /// </summary>
        /// <param name="opt">
        /// model, isTree, storeConfig
        /// </param>
        /// <returns></returns>

        opt = Ext.apply({
            isTree: false
        }, opt);

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