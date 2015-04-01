(function () {
    "use strict";
    angular.module('adm.product')
    .factory('ProductService', ['$resource', 'moduleConfiguration', ProductService ]);

    function ProductService($resource, moduleConfiguration) {
        var actions = { $update: { method: "PUT"} };
        return $resource(moduleConfiguration.IndexUrl + '/:id', { id: "@id" }, actions);
    }
})();
