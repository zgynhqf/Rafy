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

Ext.define('Rafy.svc.ServiceInvoker', {
    singleton: true,
    invokeService: function (svc, svcInput) {
        this.invoke({
            svc: svc,
            svcInput: svcInput,
            callback: null
        });
    },
    invoke: function (op) {
        /// <summary>
        /// 调用指定的服务
        /// </summary>
        /// <param name="op">
        /// svc: 必选，字符串表示的服务名称。
        /// svcInput: 可选，服务参数对应的 json 对象。
        /// callback: 可选，回调。
        /// async: true。
        /// </param>
        op = Ext.apply({ async: true }, op);

        var me = this;

        var url = me._url(op.svc);

        if (!op.svcInput) { op.svcInput = {}; }

        Ext.Ajax.request({
            url: url,
            async: op.async,
            params: { svcInput: me._seriaizeInput(op.svcInput) },
            success: function (response, opts) {
                if (op.callback) {
                    var res = me._deserializeOutput(response.responseText);
                    op.callback(res);
                }
            },
            failure: function (response, opts) { }
        });
    },
    _seriaizeInput: function (input) {
        for (var property in input) {
            var value = input[property];
            if (value) {
                //如果是实体或者实体集合，则需要序列化为变更集。
                if (value.isModel || value.isStore) {
                    //都使用组合实体序列化方式，否则需要传入的实体本身没有加载任何子。
                    var changeSet = Rafy.data.Serializer.serialize(value, true);
                    input[property] = changeSet;
                }
            }
        }

        return Ext.encode(input);
    },
    _deserializeOutput: function (responseText) {
        var res = Ext.decode(responseText);

        for (var property in res) {
            var value = res[property];

            //如果定义了 model 属性，则表示这个属性是一个实体或者实体的集合，这时需要自动转换为 Model、Store。
            if (value && value.model) {
                if (value._isEntity) {
                    var entity = Ext.create(value.model, value);
                    res[property] = entity;
                }
                else {
                    var store = Rafy.data.EntityRepository.createStore({
                        model: value.model
                    });

                    store.loadRawData(value);

                    res[property] = store;
                }
            }
        }

        return res;
    },
    _url: function (svc) {
        var res = Ext.String.format("/Rafy_ServiceInvoker.ashx?svc={0}", svc);
        return res;
    }
    //,_encodeParams: function (params) {
    //    for (var p in params) {
    //        var value = params[p];
    //        if (Ext.isObject(value)) {
    //            if (value.isStore) {

    //            }
    //            else if (value.isModel) {

    //            }
    //        }
    //    }
    //}
});