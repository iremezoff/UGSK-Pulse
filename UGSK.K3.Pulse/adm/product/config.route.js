(function () {
    "use strict";
    angular.module('adm.product', ['ui.router']).config(['$stateProvider', '$urlRouterProvider', configRoute]);

    function configRoute($stateProvider, $urlRouterProvider) {

        $urlRouterProvider
        .when('/p', '/products')
        .when('/p?code', '/products/:code')
        .otherwise('/');

        $stateProvider
        .state("products", {
            url: "/products",
            abstract: true,
            templateUrl: 'product/list.html'
        })
        .state("products.detail", {
            url: "/:code",
            templateUrl : "product/product.html"
        });
    }
})();
