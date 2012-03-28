Ext.define('Oea', {
    singleton: true,

    _IdPropertyName: 'Id',

    notSupport: function (name) {
        name = name || "";
        alert("这个方法暂时不被支持。" + name);
        throw new Error("这个方法暂时不被支持。" + name);
    },
    notImplement: function (name) {
        name = name || "";
        alert("这个方法没有被实现。" + name);
        throw new Error("这个方法没有被实现。" + name);
    },
    markAbstract: function (name) {
        this.notImplement(name);
    },
    invokeSvc: function () {
        var o = Oea.svc.ServiceInvoker;
        return o.invoke.apply(o, arguments);
    },
    //数组扩展方法
    findFirst: function (array, filter) {
        for (var i = 0; i < array.length; i++) {
            var item = array[i];
            if (filter(item)) { return item; }
        }

        return null;
    },
    first: function (array, filter) {
        var r = this.findFirst(array, filter);
        if (r == null) throw new Error("没有找到对应的元素。");
        return r;
    },
    each: function (array, fn) {
        if (array) {
            for (var i = 0; i < array.length; i++) {
                var item = array[i];
                var res = fn(item);
                if (res === false) return false;
            }
        }
        return true;
    },

    defineEntity: function (model, entityConfig) {
        Ext.apply(entityConfig, {
            "extend": "Oea.data.Entity",
            "proxy": {
                "url": "/OEA_EntityDataPortal.ashx?type=" + model,
                "reader": {
                    "type": "json",
                    "root": "entities"
                },
                "type": "ajax"
            }
        });
        Ext.define(model, entityConfig);
    },
    queryEntities: function () {
        var o = Oea.data.EntityRepository;
        return o.query.apply(o, arguments);
    }
});