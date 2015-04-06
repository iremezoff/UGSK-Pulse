require.config({
    
    paths: {
        dc3: 'Scripts/d3.min',
        jquery: 'Scripts/jquery-1.6.4',
        radial: 'Scripts/radialProgress',
        'signalr-jquery': 'Scripts/jquery.signalR-2.2.0',
        noext: 'noext'
    }
    
});



require(['radial', 'dc3', 'jquery'], function () {
    console.log('loaded!');
    require(['signalr-jquery'], function () {
        console.log('loaded2!');
        require(['noext!signalr/hubs'], function() {
            console.log('loaded3!');
            require(['noext!sales-statistic'], function (_) {

                $.each(pulseCounters, function (product, params) {
                    pulse(params.div,
                        product,
                        params.diameter != undefined ? params.diameter : 300,
                        params.fontSize != undefined ? params.fontSize : 5);
                });

                //pulseCounters.forEach(function (name, value) {
                    
                //});

                
            });
        });
    });
});