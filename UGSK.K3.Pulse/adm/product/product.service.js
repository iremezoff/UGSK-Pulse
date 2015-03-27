(function () {
    "use strict";
    angular.module('adm.product')
    .factory('ProductService', ['$resource', ProductService ]);

    function ProductService($resource) {
        var products = ['uauto', 'dealer+', 'ifl'];

        return {
            getProducts: getProducts,
            getProduct: getProduct,
            add:add

        };       

        function getProducts() {
            return products;
        }

        function getProduct(code) {
            return code;
        }

        function add(code) {
            products.add(code);
        }
    }
})();