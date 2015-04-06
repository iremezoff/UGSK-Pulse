﻿(function () {
    "use strict";
    angular.module('adm.product')
    .factory('ProductService', ['$resource', 'productModuleConfiguration', ProductService]);

    function ProductService($resource, productModuleConfiguration) {
        var actions = { $update: { method: "PUT"} };
        return $resource(productModuleConfiguration.IndexUrl + '/:id', { id: "@id" }, actions);
    }
})();