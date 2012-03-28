Ext.define('Oea.autoUI.layouts.RightChildren', {
    extend: 'Oea.autoUI.layouts.Common',
    _layoutChildrenCore: function (mainControl, childrenTab) {
        return Ext.widget('container', {
            border: 0,
            layout: 'border',
            items: [{
                region: 'west',
                width: 300,
                border: 0,
                split: true,
                layout: 'fit',
                items: mainControl
            }, {
                region: 'center',
                border: 0,
                autoScroll: true,
                items: childrenTab
            }]
        });
    }
});