(function () {
    "use strict";
    angular.module('adm.counter')
    .config(['$stateProvider', '$urlRouterProvider', configRoute]);

    function configRoute($stateProvider, $urlRouterProvider) {
        $urlRouterProvider
        .when('/c', '/counter')        
        .otherwise('/');

        $stateProvider
        .state("counter", {
            url: "/counter",
            templateUrl: 'counter/counters.html',
            controller: 'CountersController',
            controllerAs: 'counters'
        });
    }
})();
