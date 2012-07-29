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

Ext.define('Oea.svc.ServiceInvoker', {
    singleton: true,
    invokeSvc: function (svc, svcParams) {
        this.invoke({
            svc: svc,
            svcParams: svcParams,
            callback: null
        });
    },
    invoke: function (op) {
        /// <summary>
        /// 调用指定的服务
        /// </summary>
        /// <param name="op">
        /// svc: 必选，字符串表示的服务名称。
        /// svcParams: 可选，服务参数对应的 json 对象。
        /// callback: 可选，回调。
        /// async: true。
        /// </param>
        op = Ext.apply({ async: true }, op);

        var url = this._url(op);

        if (!op.svcParams) { op.svcParams = {}; }

        Ext.Ajax.request({
            url: url,
            async: op.async,
            params: { svcParams: Ext.encode(op.svcParams) },
            success: function (response, opts) {
                if (op.callback) {
                    var meta = Ext.decode(response.responseText);
                    op.callback(meta);
                }
            },
            failure: function (response, opts) { }
        });
    },
    _url: function (op) {
        var res = Ext.String.format("/OEA_ServiceInvoker.ashx?svc={0}", op.svc);
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