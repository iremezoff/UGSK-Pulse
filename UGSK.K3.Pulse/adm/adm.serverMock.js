//serviceMock.js
(function(){
	angular.module('adm.serverMock',['ngMockE2E'])
	.run(function($httpBackend){
		var indexes = [{Id:1, Product:'uauto',Value:100}];

		$httpBackend.whenGET("/api/Index/").respond(indexes);

		$httpBackend.whenPOST("/api/Index/").respond(function(method, url, data) {
			var index = angular.fromJson(data);
			indexes.push(index);
			console.debug('Index created:');
			console.table(index);
			console.debug('Indexes array:');
			console.table(indexes);

			return [200, index, {}];
		});

		$httpBackend.whenPUT("/api/Index/").respond(function(method,url,data){
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

		$httpBackend.whenDELETE("/api/Index/*").respond(function(method,url,data){
			var index = angular.fromJson(data);
			for (var i = indexes.length - 1; i >= 0; i--) {
				if (indexes[i]['Id'] == index.Id) {
					console.debug("Index deleted:\nDeleted value");
					console.table(index);
					indexes.splice(i,1);
					break;
				};
			};
			console.debug('Indexes array:');
			console.table(indexes);

			return [200, index, {}]
		});

		$httpBackend.whenGET(/.*/).passThrough();
	});	
})();