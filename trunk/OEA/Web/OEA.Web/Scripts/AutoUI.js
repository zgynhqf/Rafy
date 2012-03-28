Ext.define('Oea.AutoUI', {
    singleton: true,
    constructor: function () {
        this.viewFactory = new Oea.autoUI.ViewFactory();
        this.aggtUI = new Oea.autoUI.AggtUIGenerator(this.viewFactory);
    },
    //meta
    getMeta: function () {
        var o = Oea.meta.MetaService;
        return o.getMeta.apply(o, arguments);
    },
    //ui
    createListView: function () {
        var o = this.viewFactory;
        return o.createListView.apply(o, arguments);
    },
    createDetailView: function () {
        var o = this.viewFactory;
        return o.createDetailView.apply(o, arguments);
    },
    generateAggtControl: function () {
        var o = this.aggtUI;
        return o.generateControl.apply(o, arguments);
    }
});