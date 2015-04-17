define(function () {
    return {
        currentDate: function () {
            var date = new Date();

            var dd = date.getDate();
            var mm = date.getMonth() + 1;
            var yyyy = date.getFullYear();
            var hh = date.getHours();
            var MM = date.getMinutes();
            var ss = date.getSeconds();

            return (hh < 10 ? '0' + hh : hh) + ':' + (MM < 10 ? '0' + MM : MM) + ':' + (ss < 10 ? '0' + ss : ss) + ' ' + (dd < 10 ? '0' + dd : dd) + '.' + (mm < 10 ? '0' + mm : mm) + '.' + yyyy;
        }
    }
});

String.prototype.format = function () {
    var formatted = this;
    for (var i = 0; i < arguments.length; i++) {
        var regexp = new RegExp('\\{' + i + '\\}', 'gi');
        formatted = formatted.replace(regexp, arguments[i]);
    }
    return formatted;
};