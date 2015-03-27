(function () {
    "use strict";
    angular.module('adm.product')
    .controller('ProductController', ['$scope','$state','$stateParams',ProductController]);

    function ProductController($scope, $state, $stateParams ) {
        this.code = $stateParams.code || undefined;
    }
})();
