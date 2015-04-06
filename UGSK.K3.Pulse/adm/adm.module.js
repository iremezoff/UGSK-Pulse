(function () {
    "use strict";

    // Swap 2 following lines to enable backend service mocking 
    angular.module("adm", ["ui.router", "adm.product", "adm.counter", "adm.serverMock"])
    // angular.module("adm", ["ui.router", "adm.product", "adm.counter"])
    .constant("version", "1.0.0")
    .run(['$rootScope', '$state', '$stateParams', function ($rootScope, $state, $stateParams) {
        $rootScope.$state = $state;
        $rootScope.$stateParams = $stateParams;
    }])
    .config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {
        $urlRouterProvider.otherwise('/');
        
        $stateProvider
        .state("home", {
            url: "/",
            templateUrl: 'home.html'
        })
    }]);
})();
