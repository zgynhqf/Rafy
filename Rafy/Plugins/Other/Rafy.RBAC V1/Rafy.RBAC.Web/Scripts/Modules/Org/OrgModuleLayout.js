/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130904
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130904 14:58
 * 
*******************************************************/

Ext.define('Rafy.rbac.org.OrgModuleLayout', {
    extend: 'Rafy.autoUI.layouts.Common',
    _layoutChildrenCore: function (mainControl, childrenTab) {
        return Ext.widget('container', {
            border: 0,
            layout: 'border',
            items: [{
                region: 'west',
                width: 500,
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