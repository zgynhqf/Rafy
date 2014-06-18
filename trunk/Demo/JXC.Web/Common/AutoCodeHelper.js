Ext.define('Jxc.AutoCodeHelper', {
    singleton: true,
    generateCode: function (model) {
        model = Rafy.getModelName(model);

        var code = '';
        Rafy.invokeService({
            async: false, svc: 'JXC.AutoCodeService', svcInput: { EntityType: model },
            callback: function (res) { code = res.code; }
        });
        return code;
    }
});