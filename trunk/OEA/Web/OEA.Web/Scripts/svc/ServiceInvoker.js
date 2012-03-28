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
        var url = this._url(op);

        if (!op.svcParams) { op.svcParams = {}; }

        Ext.Ajax.request({
            url: url,
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
});