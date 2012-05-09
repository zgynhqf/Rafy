Ext.define('Jxc.AutoCodeHelper', {
    singleton: true,
    generateCode: function (model) {
        var code = '';
        Oea.invokeSvc({
            async: false, svc: 'JXC.AutoCodeService', svcParams: { EntityType: model },
            callback: function (res) { code = res.code; }
        });
        return code;
    }
});