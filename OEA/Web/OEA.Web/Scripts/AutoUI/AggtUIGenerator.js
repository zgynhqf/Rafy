Ext.define('Oea.autoUI.AggtUIGenerator', {
    constructor: function (viewFactory) {
        this._vf = viewFactory;
    },
    /// <summary>创建一个聚合控件</summary>
    /// <param name="aggtMeta" type="OEA.Web.ClientMetaModel.AggtMeta">服务端生成的元数据对象</param>
    /// <returns type="Oea.autoUI.ControlResult" />
    generateControl: function (aggtMeta) {
        var mainView = null;
        var mk = aggtMeta.mainBlock;
        if (mk.gridConfig) {
            mainView = this._vf.createListView(mk);
        }
        else {
            mainView = this._vf.createDetailView(mk);
        }

        return this._generateAggt(aggtMeta, mainView, true);
    },
    _generateAggt: function (aggtMeta, mainView, isRoot) {
        //regions
        var main = new Oea.autoUI.ControlResult(mainView, mainView.getControl());
        var regions = new Oea.autoUI.Regions(main);
        regions.isRoot = isRoot;

        this._generateChildren(aggtMeta.children, regions);

        this._generateSurrounders(aggtMeta.surrounders, regions);

        var control = this._layout(aggtMeta, regions);

        return new Oea.autoUI.ControlResult(mainView, control);
    },
    /// <returns type="Oea.autoUI.ControlResult[]" />
    _generateChildren: function (childrenAggt, regions) {
        var mainView = regions.main.getView();

        for (var i = 0; i < childrenAggt.length; i++) {
            var childAggt = childrenAggt[i];

            var childView = this._vf.createListView(childAggt.mainBlock);
            childView._propertyNameInParent = childAggt.childProperty;

            var childResult = this._generateAggt(childAggt, childView, false);
            regions.children.push(childResult);

            childView.setParent(mainView);
        }
    },
    _generateSurrounders: function (surroundersAggt, regions) {
        var me = this;
        var mainView = regions.main.getView();

        for (var i = 0; i < surroundersAggt.length; i++) {
            var surrounderAggt = surroundersAggt[i];

            var surrounderView = this._generateSurrounder(mainView, surrounderAggt);

            var surrounderResult = this._generateAggt(surrounderAggt, surrounderView, false);

            regions.surrounders.push({
                type: surrounderAggt.surrounderType,
                result: surrounderResult
            });
        }
    },
    /// <returns type="Oea.view.View" />
    _generateSurrounder: function (mainView, surrounderAggt) {
        var vf = this._vf;
        var cr = Oea.view.RelationView; //common realtion

        var surrounderType = surrounderAggt.surrounderType;
        var surrounderBlock = surrounderAggt.mainBlock;

        var surrounderView = null;
        var relation = null;
        var reverseRelation = null; //相反的关系类型

        if (surrounderType == cr.condition) {
            var surrounderView = vf.createConditionView(surrounderBlock);
            reverseRelation = new Oea.view.RelationView(cr.result, mainView);
        }
        else if (surrounderType == cr.navigation) {
            var surrounderView = vf.createNavigationView(surrounderBlock);
            reverseRelation = new Oea.view.RelationView(cr.result, mainView);
        }
        else {
            Oea.notSupport();
        }

        relation = new Oea.view.RelationView(surrounderType, surrounderView);

        //直接使用 surrounderType 作为关系的类型，把 surrounderView 添加到 mainView 的关系。
        mainView.setRelation(relation);

        //相反的关系设置
        surrounderView.setRelation(reverseRelation);

        return surrounderView;
    },
    /// <param name="aggtMeta" type="OEA.Web.ClientMetaModel.AggtMeta"></param>
    /// <param name="regions" type="Oea.autoUI.Regions"></param>
    /// <returns type="Ext.Component" />
    _layout: function (aggtMeta, regions) {
        var layout = null;
        if (aggtMeta.layoutClass) {
            layout = Ext.create(aggtMeta.layoutClass);
        }
        else {
            layout = new Oea.autoUI.layouts.Common();
        }

        var res = layout.layout(regions);

        return res;
    }
});