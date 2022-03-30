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

Ext.define('Rafy.data.EntityRepository', {
    singleton: true,

    query: function (opt) {
        /// <summary>
        /// 通过一定的查询条件来查询实体列表
        /// </summary>
        /// <param name="opt">
        /// model : 必填，实体模型类型或名称。
        /// pageSize: 默认 25。
        /// isTree: 默认 false。
        /// storeConfig : Ext store config.
        /// 
        /// method：string； 如果定义了此参数，则使用服务端仓库中对应的方法名来进行查询。
        /// params：[]；如果定义了 method 参数，则此参数用于指定对应方法的参数列表。参数的顺序必须与服务端定义的参数一致。
        /// 
        /// id: 如果定义了此参数，则按照 id 查询某个指定的实体
        /// 
        /// criteria：如果定义了此参数，则按照此参数在服务端进行过滤。
        /// 
        /// callback: 用于查询完成后的回调函数。参数与 store.load 的回调一致。
        /// </param>

        opt = Ext.apply({
            pageSize: 25,
            isTree: false,
            storeConfig: {}
        }, opt);

        var store = this.createStore(opt);

        if (opt.id) {
            opt.method = 'GetById';
            opt.params = [opt.id];
        }
        if (opt.method) {
            this.filterByMethod(store, opt.method, opt.params);
        }
        else if (opt.criteria) {
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
        else { fields = Rafy.getModel(model).prototype.fields; }

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
        /// callback: 回调函数。
        /// entity/store/_changeSetData(internal): 这三个只需要设置其中一个就可以了。
        /// </param>

        if (!opt.model) return;

        var serializer = Rafy.data.Serializer;

        var changeSet = null;
        if (opt.entity) {
            changeSet = serializer.serialize(opt.entity, true);
        }
        else if (opt.store) {
            changeSet = serializer.serialize(opt.store, true);
        }
        else {
            changeSet = opt._changeSetData;
        }

        if (changeSet.isEmpty()) {
            if (opt.callback) {
                res = { Success: true };
                opt.callback(res, true);
            }
            return;
        }

        var model = opt._changeSetData.getModel();
        var proxyUrl = model.getProxy().url;
        var data = opt._changeSetData;
        var ajaxCfg = {
            url: proxyUrl,
            params: {
                action: 'save',
                entityList: Ext.encode(data)
            },
            callback: function (options, success, response) {
                if (opt.callback) {
                    var res = null;
                    if (success) {
                        res = Ext.decode(response.responseText);
                    }
                    else {
                        res = {
                            Success: false,
                            Message: response.responseText
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
            property: '_useCriteria', value: Ext.getClassName(criteria)
        }), new Ext.util.Filter({
            property: '_criteria', value: criteria.data
        })]);
    },

    filterByMethod: function (store, method, params) {
        /// <summary>
        /// 为某个指定的 store 设置过滤方法条件。
        /// </summary>
        /// <param name="store"></param>
        /// <param name="method" type="String">服务端对应的查询方法名。</param>
        /// <param name="params" type="Array">指定对应方法的参数列表。参数的顺序必须与服务端定义的参数一致。</param>

        if (!params) params = [];

        var f = store.filters;
        f.clear();
        f.addAll([new Ext.util.Filter({
            property: '_useMethod', value: method
        }), new Ext.util.Filter({
            property: '_params', value: params
        })]);
    },

    createStore: function (opt) {
        /// <summary>
        /// 为指定的实体创建一个 Ext 仓库
        /// </summary>
        /// <param name="opt">
        /// model：实体模型类型或名称
        /// storeConfig：数据集配置
        /// </param>
        /// <returns></returns>

        var model = Rafy.getModel(opt.model);

        var storeConfig = Ext.apply({
            model: Rafy.getModelName(model)
        }, opt.storeConfig);

        if (model.isTree) {
            storeConfig.root = {
                loaded: true
            }; //如果没有 root，则会自动发起一次查询。
        }
        else {
            storeConfig.remoteFilter = true;
            //if (opt.groupBy) { storeConfig.groupField = opt.groupBy; }
        }

        var storeName = model.isTree ? 'Ext.data.TreeStore' : 'Ext.data.Store';
        var store = Ext.create(storeName, storeConfig);

        return store;
    }
});