require.config({
    
    paths: {
        dc3: 'Scripts/d3.min',
        jquery: 'Scripts/jquery-1.6.4.min',
        radial: 'Scripts/radialProgress',
        'signalr-jquery': 'Scripts/jquery.signalR-2.2.0.min',
        noext: 'noext'
    }
    
});



require(['radial', 'dc3', 'jquery'], function () {
    console.log('loaded!');
    require(['signalr-jquery'], function () {
        console.log('loaded2!');
        require(['noext!signalr/hubs'], function() {
            console.log('loaded3!');
            jQuery.connection.hub.url = "http://localhost:40438/signalr";
            require(['noext!sales-statistic']);
        });
    });
});