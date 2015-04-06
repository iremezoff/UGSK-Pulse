(function () {
    "use strict";
    angular.module('adm.product')
    .controller('ProductsController', ['$scope', '$state', 'ProductService', ProductsController]);

    function ProductsController($scope, $state, ProductService) {
        var vm = this;
        vm.items = [];
        vm.add = add;
        vm.saveAll = saveAll;
        vm.remove = remove;        

        init();

        function init() {
            ProductService.query(function (data) {
                vm.items = data;
            });
        }

        function add() {
            var newProduct = new ProductService({ Product: '', Value: 100 })
            vm.items.push(newProduct);
        }

        function saveAll() {
            vm.items.forEach(function (item) {
                if (item.Id) {
                    ProductService.$update(item);
                }
                else {
                    item.$save();
                }
            })

        }
        function remove(item) {
            if (item.Id) {
                ProductService.remove({ Id: item.Id })
                .$promise.then(
                function (deletedItem) {
                    removeFromArray(deletedItem);
                }
                , function (err) {
                    console.error(err);
                });
            } else {
                removeFromArray(item)
            }

        }

        function removeFromArray(deletedItem) {
            var itemIndex = -1;            
            vm.items.forEach(function (curValue, index) {
                if (curValue.Id === deletedItem.Id)
                {
                    itemIndex = index;
                    return;
                }
            })
            
            if (index > -1) {
                vm.items.splice(index, 1);
            }
        }
    }
})();
