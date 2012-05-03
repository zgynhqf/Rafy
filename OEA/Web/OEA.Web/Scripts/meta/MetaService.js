Ext.define('Oea.meta.MetaService', {
    singleton: true,
    getMeta: function (op) {
        var url = this._url(op);
        var aOp = {
            url: url,
            async: op.async,
            success: function (response, opts) {
                var meta = Ext.decode(response.responseText);

                meta.model = op.model;

                op.callback(meta);
            },
            failure: function (response, opts) { }
        };
        Ext.Ajax.request(aOp);
    },
    _url: function (op) {
        var res = Ext.String.format("/OEA_MetaModelPortal.ashx?type={0}", op.model);
        if (op.module) { res += "&module=" + encodeURIComponent(op.module); }
        if (op.viewName) { res += "&viewName=" + encodeURIComponent(op.viewName); }
        if (op.isAggt) { res += "&isAggt=1"; }
        if (op.isReadonly) { res += "&isReadonly=1"; }
        if (op.ignoreCommands) { res += "&ignoreCommands=1"; }
        if (op.isDetail) { res += "&isDetail=1"; }
        else if (op.isLookup) { res += "&isLookup=1"; }
        return res;
    }
});