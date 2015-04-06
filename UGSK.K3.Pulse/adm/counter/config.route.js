(function () {
    "use strict";
    angular.module('adm.counter', ['ui.router']).config(['$stateProvider', '$urlRouterProvider', configRoute]);

    function configRoute($stateProvider, $urlRouterProvider) {
        /////////////////////////////
        // Redirects and Otherwise //
        /////////////////////////////

        // Use $urlRouterProvider to configure any redirects (when) and invalid urls (otherwise).
        $urlRouterProvider
        .when('/c', '/counter')        
        .otherwise('/');

        $stateProvider
        .state("counter", {
            // Use a url of "/" to set a state as the "index".
            url: "/counter",
            templateUrl: 'counter/counter.html',


        });
    }
})();
