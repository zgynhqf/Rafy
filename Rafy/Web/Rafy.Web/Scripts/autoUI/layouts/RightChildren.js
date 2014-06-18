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

Ext.define('Rafy.autoUI.layouts.RightChildren', {
    extend: 'Rafy.autoUI.layouts.Common',
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