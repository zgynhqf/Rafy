Ext.define('Oea.autoUI.layouts.Common', {
    extend: 'Oea.autoUI.Layout',
    layout: function (regions) {
        var childrenUI = this._layoutChildren(regions);
        var res = this._layoutNaviCondition(regions, childrenUI);
        return res;
    },
    _layoutChildren: function (regions) {
        var main = regions.main;
        var children = regions.children;

        var mainControl = main.getControl();

        if (children.length == 0) {
            if (regions.isRoot) {
                mainControl = Ext.widget('container', {
                    border: 0,
                    layout: 'fit',
                    items: [mainControl]
                });
            }
            else {
                mainControl.setHeight(300);
            }

            return mainControl;
        }

        //Create a tab here
        var tabPanel = {
            xtype: 'tabpanel',
            activeTab: 0,
            items: []
        };
        Ext.each(children, function (child) {
            tabPanel.items.push({
                title: child.getView().getMeta().label,
                items: child.getControl()
            });
        });

        return this._layoutChildrenCore(mainControl, tabPanel);
    },
    _layoutNaviCondition: function (regions, childrenUI) {
        //        var navi = regions.getNavigation();
        var con = regions.getCondition();
        if (con == null) { return childrenUI; }

        return Ext.widget('container', {
            border: 0,
            layout: 'border',
            items: [
                {
                    region: 'west',
                    width: 300,
                    border: 0,
                    layout: 'fit',
                    split: true,
                    autoScroll: true,
                    items: con.getControl()
                },
                {
                    region: 'center',
                    border: 0,
                    layout: 'fit',
                    items: childrenUI
                }
            ]
        });

        //        return Ext.widget('container', {
        //            border: 0,
        //            layout: 'border',
        //            items: [{
        //                region: 'west',
        //                width: 300,
        //                border: 0,
        //                split: true,
        //                layout: 'fit',
        //                items: mainControl
        //            }, {
        //                region: 'center',
        //                border: 0,
        //                autoScroll: true,
        //                items: tabPanel
        //            }]
        //        });
    },
    //protected
    _layoutChildrenCore: function (mainControl, childrenTab) {
        mainControl.setHeight(300);

        return Ext.widget('container', {
            border: 0,
            items: [mainControl, childrenTab]
        });
    }
});