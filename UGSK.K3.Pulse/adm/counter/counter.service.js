(function () {
    "use strict";
    angular.module('adm.counter')
    .factory('Counter',['$resource','productModuleConfiguration', CounterService]);

    function CounterService($resource, productModuleConfiguration) {
        var actions = { $update: { method: "PUT"} };
        return $resource(productModuleConfiguration.CounterUrl + '/:id', { id: "@Id" }, actions);
    }
})();
