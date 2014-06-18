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

/// <summary>除了 main 和 children，可以为这个对象添加其它的约定属性，如 navigate、condition</summary>
Ext.define('Rafy.autoUI.Regions', {
    constructor: function (main) {
        this.main = main;
        this.children = [];
        this.surrounders = []; //{type,result}

        //当前 main 是否为根对象
        this.isRoot = false;
    },
    getCondition: function () {
        return this.getRegion(Rafy.view.RelationView.condition);
    },
    getNavigation: function () {
        return this.getRegion(Rafy.view.RelationView.navigation);
    },
    getRegion: function (name) {
        var s = Rafy.findFirst(this.surrounders, function (r) { return r.type == name; });
        if (s != null) return s.result;
        return null;
    }
});