require.config({

    paths: {
        dc3: 'Scripts/d3.min',
        jquery: 'Scripts/jquery-1.6.4.min',
        radial: 'Scripts/radialProgress',
        'signalr-jquery': 'Scripts/jquery.signalR-2.2.0.min',
        noext: 'Scripts/noext',
        stat: 'Scripts/sales-statistic'
    }
});

String.prototype.format = function () {
    var formatted = this;
    for (var i = 0; i < arguments.length; i++) {
        var regexp = new RegExp('\\{' + i + '\\}', 'gi');
        formatted = formatted.replace(regexp, arguments[i]);
    }
    return formatted;
};

require(['radial', 'dc3', 'jquery', 'noext!config'], function () {
    console.log('loaded!');
    require(['signalr-jquery'], function () {
        console.log('loaded2!');
        require(['noext!signalr/hubs'], function () {
            console.log('loaded3!');
            require(['stat'], function () {

                var c = pulse();

                $.each(pulseCounters, function (product, params) {
                    c.addCounter(params.div,
                        product,
                        params.diameter != undefined ? params.diameter : config.radialDiameter,
                        params.fontSize != undefined ? params.fontSize : config.fontSize);
                });
            });
        });
    });
});