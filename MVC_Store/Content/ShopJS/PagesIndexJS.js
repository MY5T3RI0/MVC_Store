$(function () {
    $("a.delete").click(function () {
        if (!confirm("Confirm the deletion")) return false;
    });

    //------------------------------------

    $("table#pages tbody").sortable({
        items: "tr:not(.home)",
        placeholder: "ui-state-highlight",
        update: function () {
            var ids = $("table#pages tbody").sortable("serialize");
            var url = "/Admin/Page/ReorderPages";

            $.post(url, ids, function (data) {

            });
        }
    });
});