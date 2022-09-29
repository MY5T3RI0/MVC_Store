/* Incriment product */
$(function () {

    $("a.incproduct").click(function (e) {
        e.preventDefault();

        var productId = $(this).data("id");
        var url = "/cart/IncrementProduct";

        $.getJSON(url, { productId: productId }, function (data) {
            $("td.cnt" + productId).html(data.cnt);

            var price = data.cnt * data.price;
            var priceHtml = price.toFixed(2) + "$";

            $("td.total" + productId).html(priceHtml);

            var gt = parseFloat($("td.grandtotal span").text());
            var grandtotal = (gt + data.price).toFixed(2) + "$";

            $("td.grandtotal span").text(grandtotal);
        });
    });
});

/* Decriment product */
$(function () {

    $("a.decproduct").click(function (e) {
        e.preventDefault();

        var $this = $(this);
        var productId = $(this).data("id");
        var url = "/cart/DecrementProduct";

        $.getJSON(url, { productId: productId }, function (data) {

            if (data.cnt == 0) {
                $this.parent().fadeOut("fast", function () {
                    location.reload();
                });
            }
            else {
                $("td.cnt" + productId).html(data.cnt);

                var price = data.cnt * data.price;
                var priceHtml = "$" + price.toFixed(2);

                $("td.total" + productId).html(priceHtml);

                var gt = parseFloat($("td.grandtotal span").text());
                var grandtotal = (gt - data.price).toFixed(2) + "$";

                $("td.grandtotal span").text(grandtotal);
            }
        });
    });
});
/*-----------------------------------------------------------*/

/* Remove product */
$(function () {

    $("a.removeproduct").click(function (e) {
        e.preventDefault();

        var $this = $(this);
        var productId = $(this).data("id");
        var url = "/cart/RemoveProduct";

        $.get(url, { productId: productId }, function (data) {
            location.reload();
        });
    });
});