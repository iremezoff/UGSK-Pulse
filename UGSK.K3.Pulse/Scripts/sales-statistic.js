
function pulse(domDiv, product, diameter, fontSize) {

    $.connection.hub.url = config.baseAddress + "/signalr";
    var chat = $.connection.statisticHub;

    chat.client.broadcastCounter = function (counter) {
        if (counter.Product !== product)
            return;

        // total daily counter
        if (counter.Kind === config.constants.TOTAL_COUNTER && counter.PeriodKind === config.constants.DAILY_PERIOD) {
            rp2.value(counter.Value);
        }
        // average weekly counter
        if (counter.Kind === config.constants.AVERAGE_COUNTER && counter.PeriodKind === config.constants.WEEKLY_PERIOD) {
            rp2.value2(counter.Value);
        }

        rp2.label(currentDate());
        rp2.render();
    };

    chat.client.broadcastIndex = function (product, value) {
        rp2.maxValue(value).render();
    };

    var uriPattern = config.baseAddress + "/{0}";

    var rp2 = radialProgress(domDiv, uriPattern.format("Content"))
        .label(product)
        .diameter(diameter)
        .fontSize(fontSize);

    var counterAddress = uriPattern.format("/api/counter");
    $.ajax({
            method: "GET",
            url: counterAddress,
            data: { product: product }
        })
        .done(function(counter) {
            rp2.value(counter.Value);
            $.ajax({
                    method: "GET",
                    url: uriPattern.format("/api/index"),
                    data: { product: product }
                })
                .done(function(index) {
                    rp2.maxValue(index.Value);
                    rp2.maxValue2(index.Value);

                    rp2.render();
                });
        });

    $.ajax({
        method: "GET",
        url: counterAddress,
        data: {
            product: product,
            periodKind: config.constants.WEEKLY_PERIOD,
            counterKind: config.constants.AVERAGE_COUNTER
        }
    })
        .done(function (counter) {
            rp2.value2(counter.Value);
        });

    $.connection.hub.start();
}

function currentDate() {
    var date = new Date();

    var dd = date.getDate();
    var mm = date.getMonth() + 1;
    var yyyy = date.getFullYear();
    var hh = date.getHours();
    var MM = date.getMinutes();
    var ss = date.getSeconds();

    return (hh < 10 ? '0' + hh : hh) + ':' + (MM < 10 ? '0' + MM : MM) + ':' + (ss < 10 ? '0' + ss : ss) + ' ' + (dd < 10 ? '0' + dd : dd) + '.' + (mm < 10 ? '0' + mm : mm) + '.' + yyyy;
}






