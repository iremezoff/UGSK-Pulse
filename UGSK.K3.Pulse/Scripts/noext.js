define(function () {
    var QUERY_PARAM = 'noext';

    //API
    return {
        load: function (name, req, onLoad, config) {
            var url = req.toUrl(name).replace(/\.js$/, '');
            req([url], function (mod) {
                onLoad(mod);
            });
        },
        normalize: function (name, norm) {
            //append query string to avoid adding .js extension
            //name += (name.indexOf('?') < 0) ? '?' : '&';
            return name;// + QUERY_PARAM + '=1';
        }

    };
});