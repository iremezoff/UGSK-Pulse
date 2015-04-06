//serviceMock.js
(function () {
    angular.module('adm.serverMock', ['ngMockE2E'])
	.run(function ($httpBackend) {
	    var indexes = [{ Id: 1, Product: 'uauto', Value: 100, ActiveStart: new Date("2015-04-01"), IndexKind: 2 }];

	    $httpBackend.whenGET("/api/Index/").respond(indexes);

	    $httpBackend.whenPOST("/api/Index/").respond(function (method, url, data) {
	        var index = angular.fromJson(data);
	        index.Id = Math.floor(Math.random() * 1000);

	        indexes.push(index);
	        console.debug('Index created:');
	        console.table(index);
	        console.debug('Indexes array:');
	        console.table(indexes);

	        return [200, index, {}];
	    });

	    $httpBackend.whenPUT(new RegExp('/api/Index/\\d+')).respond(function (method, url, data) {
	        var index = angular.fromJson(data);
	        for (var i = indexes.length - 1; i >= 0; i--) {
	            if (indexes[i]['Id'] == index.Id) {
	                console.debug('Index updated.\nOld value:');
	                console.table(indexes[i]);
	                console.debug('New value:');
	                console.table(index);
	                indexes[i] = index;
	                break;
	            };
	        };
	        console.debug('Indexes array:');
	        console.table(indexes);
	        return [200, index, {}];
	    });

	    $httpBackend.whenDELETE(new RegExp('/api/Index/\\d+')).respond(function (method, url) {
	        var regexp = new RegExp('\\d+');
	        var id = url.match(regexp)[0];

	        for (var i = indexes.length - 1; i >= 0; i--) {
	            if (indexes[i]['Id'] == id) {
	                console.debug("Index with id = " + id + " deleted");
	                indexes.splice(i, 1);	                
	            };
	        };
	        console.debug('Indexes array:');
	        console.table(indexes);

	        return [200, undefined, {}]
	    });

	    $httpBackend.whenGET(/.*/).passThrough();
	});
})();