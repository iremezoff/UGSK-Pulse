(function () {
    "use strict";
    angular.module('adm.product').config(['$stateProvider', '$urlRouterProvider', configRoute]);

    function configRoute($stateProvider, $urlRouterProvider) {

        $urlRouterProvider
        .when('/p', '/products')
        .when('/p?code', '/products/:code')
        .otherwise('/');

        $stateProvider
        .state("products", {
            url: "/products",
            templateUrl: 'product/products.html',
            controller: 'ProductsController',
            controllerAs: "products"
        });
    }
})();
