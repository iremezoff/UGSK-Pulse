(function () {
    "use strict";    
    angular.module('adm.product', ['ui.router', 'ngResource'])
    .config(['$resourceProvider', function ($resourceProvider) {
        // Don't strip trailing slashes from calculated URLs
        $resourceProvider.defaults.stripTrailingSlashes = false;
    }])
    .constant('productModuleConfiguration',{
        'IndexUrl':'/api/Index',
        'CounterUrl': '/api/Counter'
    });
})();