(function() {
    "use strict";
    angular
        .module("adm")
        .directive("formatDate", formatDate);

    function formatDate() {
        return {
            require: "ngModel",
            link: function (scope, element, attrs, ngModelController) {
                ngModelController.$parsers.push(function (data) {
                    //convert data from view format to model format
                    if (!data)
                        return data;

                    return moment(data).format("YYYY-MM-DD") + "T00:00:00Z";
                });

                ngModelController.$formatters.push(function (data) {
                    //convert data from model format to view format
                    if (!data)
                        return data;

                    return new Date(data); //converted
                });
            }
        }
    }
})();
