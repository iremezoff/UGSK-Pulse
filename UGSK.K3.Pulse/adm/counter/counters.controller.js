(function () {
    "use strict";
    angular.module('adm.counter')
    .controller('CountersController', ['Counter', CountersController]);

    function CountersController(Counter) {
        //Product, PeriodKind, Value, CounterKind, PeriodStart, IsClosed
        var vm = this;
        var items = [];

        init();

        function init(){
            Counter.query(function(data){
                vm.items = data;
            });
        }
    }
})();
