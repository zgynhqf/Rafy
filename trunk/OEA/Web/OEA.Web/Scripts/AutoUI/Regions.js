/// <summary>除了 main 和 children，可以为这个对象添加其它的约定属性，如 navigate、condition</summary>
Ext.define('Oea.autoUI.Regions', {
    constructor: function (main) {
        this.main = main;
        this.children = [];
        this.surrounders = []; //{type,result}

        //当前 main 是否为根对象
        this.isRoot = false;
    },
    getCondition: function () {
        return this.getRegion(Oea.view.RelationView.condition);
    },
    getNavigation: function () {
        return this.getRegion(Oea.view.RelationView.navigation);
    },
    getRegion: function (name) {
        var s = Oea.findFirst(this.surrounders, function (r) { return r.type == name; });
        if (s != null) return s.result;
        return null;
    }
});