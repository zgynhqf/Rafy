(function () {
    var timeSpanExtend = {
        constructor: function () {
            this.callParent(arguments);

            //动态默认值
            var weeksAgo = new Date();
            weeksAgo.setTime(weeksAgo.getTime() - (7 * 24 * 60 * 60 * 1000));//7 天前
            this.set({
                From: weeksAgo,
                To: new Date()
            });
        }
        //,
        ////protected override
        //onPropertyChanged: function (e) {
        //    this.callParent(arguments);
        //    alert(e.property + "Changed");
        //}
    };

    Ext.define('JXC.TimeSpanCriteria_Extension', Ext.apply({
        override: 'JXC.TimeSpanCriteria'
    }, timeSpanExtend));
    Ext.define('JXC.ClientTimeSpanCriteria_Extension', Ext.apply({
        override : 'JXC.ClientTimeSpanCriteria'
    }, timeSpanExtend));
})();