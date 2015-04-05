//product.controller.js
(function () {
    "use strict";
    angular.module('adm.product')
    .controller('ProductsController', ['$scope', '$state', 'Product', ProductsController]);

    function ProductsController($scope, $state, Product) {
        var vm = this;
        vm.items = [];
        vm.add = add;
        vm.saveAll = saveAll;
        vm.remove = remove;        

        init();

        function init() {
            Product.query(function (data) {
                vm.items = data;
            });
        }

        function add() {
            var newProduct = new Product({ Product: '', Value: 100 })
            vm.items.push(newProduct);
        }

        function saveAll() {
            vm.items.forEach(function (item) {
                if (item.Id) {
                    Product.$update(item);
                }
                else {
                    item.$save();
                }
            })

        }
        function remove(item) {           
            if(item.Id)
            {
                Product.remove(item, removeFromArray(item));
                //Product.remove(item).then(removeFromArray(item));
            } else 
            {
                removeFromArray(item);
            }
        }

        function removeFromArray(deletedItem) {
            var itemIndex = -1;

            for (var i = vm.items.length - 1; i >= 0; i--) {
                if (deletedItem.$$hash == vm.items[i].$$hash){
                    vm.items.splice(i, 1);
                }
            };
        }
    }
})();
