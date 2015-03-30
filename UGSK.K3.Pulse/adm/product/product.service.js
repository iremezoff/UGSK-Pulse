(function () {
    "use strict";
    angular.module('adm.product')
    .factory('ProductService', ['$resource', ProductService ]);

    function ProductService($resource) {
        var actions = { $update: { method: "PUT"} };
        return $resource('/api/Index/(:id)', { code: "@code" }, actions);
    }
})();
