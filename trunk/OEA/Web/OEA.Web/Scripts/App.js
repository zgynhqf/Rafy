Ext.define('Oea.App', {
    singleton: true,
    showModel: function (opt) {
        opt = Ext.apply({
            isAggt: true,
            viewName: ""
        }, opt);
        var model = opt.model;
        var module = opt.module;
        var container = opt.container;

        var callBack = null;
        if (!opt.isAggt) {
            callBack = function (meta) {
                var listView = Oea.AutoUI.createListView(meta);

                listView.loadData();

                if (!container) container = Ext.widget('viewport', {
                    border: 0,
                    layout: 'fit',
                    autoScroll: true
                });
                container.add(listView.getControl());
            };
        }
        else {
            callBack = function (meta) {
                var controlResult = Oea.AutoUI.generateAggtControl(meta);

                if (meta.surrounders.length == 0) {
                    var listView = controlResult.getView();
                    listView.loadData();
                }

                if (!container) container = Ext.widget('viewport', {
                    border: 0,
                    layout: 'fit',
                    autoScroll: true
                });

                //生成的聚合界面可能比较大，所以需要显示“加载中”
                container.setLoading(true);
                setTimeout(function () {
                    container.add(controlResult.getControl());
                    container.setLoading(false);
                }, 10);
            };
        }
        Oea.AutoUI.getMeta({
            module: module,
            model: model,
            viewName: opt.viewName,
            isAggt: opt.isAggt,
            callback: callBack
        });
    }
});