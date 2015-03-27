(function () {
    "use strict";

    angular.module("adm", ["ui.router", "adm.product", "adm.counter"])
    .constant("version", "1.0.0")
    .run(['$rootScope', '$state', '$stateParams', function ($rootScope, $state, $stateParams) {
        $rootScope.$state = $state;
        $rootScope.$stateParams = $stateParams;
    }
    ]
        )
    .config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {
        $urlRouterProvider
        .when('/p', '/products')
        .when('/p?id', '/products/:id')
        .when('/c', '/counter')
        .otherwise('/');
        $stateProvider
        .state("home", {
            url: "/",
            templateUrl: 'home.html'
        })
    }]);
})();
