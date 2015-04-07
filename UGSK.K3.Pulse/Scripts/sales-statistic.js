
function pulse() {

    var pulses = {};

    $.connection.hub.url = config.baseAddress + "/signalr";
    var chat = $.connection.statisticHub;

    chat.client.broadcastCounter = function (counter) {

        var pulse = pulses[counter.Product];

        if (pulse == undefined)
            return;

        // total daily counter
        if (counter.Kind === config.constants.TOTAL_COUNTER && counter.PeriodKind === config.constants.DAILY_PERIOD) {
            pulse.value(counter.Value);
        }
        // average weekly counter
        if (counter.Kind === config.constants.AVERAGE_COUNTER && counter.PeriodKind === config.constants.WEEKLY_PERIOD) {
            pulse.value2(counter.Value);
        }

        //pulse.label(currentDate());
        pulse.render();
    };

    chat.client.broadcastIndex = function (product, value) {
        var pulse = pulses[product];

        if (pulse == undefined)
            return;

        pulse.maxValue(value).render();
    };

    this.addCounter = function (domDiv, product, diameter, fontSize) {

        var uriPattern = config.baseAddress + "/{0}";

        var rp2 = radialProgress(domDiv, uriPattern.format("Content"))
            .label(product)
            .diameter(diameter)
            .fontSize(fontSize);

        pulses[product] = rp2;

        var counterAddress = uriPattern.format("/api/counter");
        $.ajax({
            method: "GET",
            url: counterAddress,
            data: { product: product }
        })
            .done(function (counter) {
                rp2.value(counter.Value);
                $.ajax({
                    method: "GET",
                    url: uriPattern.format("/api/index"),
                    data: { product: product }
                })
                    .done(function (index) {
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

    return this;
}








