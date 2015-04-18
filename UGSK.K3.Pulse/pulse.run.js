require.config({
    shim: {
        'signalr-client': { deps: ['jquery'] },
        'pulse': { deps: ['util','radial', 'signalr-client', 'noext!signalr/hubs', 'noext!config'] }
    },
    paths: {
        d3: 'Scripts/d3/d3.min',
        jquery: 'Scripts/jquery-2.1.3.min',
        'signalr-client': 'Scripts/jquery.signalR-2.2.0.min',
        pulse: 'app/pulse',
        util: 'app/utils',
        radial: 'app/radial',
        noext: 'app/noext'
    }
});

require(['jquery', 'counters', 'noext!config', 'radial', 'pulse'], function ($, counters, config, radial) {
    var p = pulse($, config, radial);

    $.each(counters, function (product, params) {
        p.addCounter(params.div,
            product,
            params.diameter,
            params.fontSize);
    });
});