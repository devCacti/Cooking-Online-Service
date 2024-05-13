$("#loginFirst").hover(function () {
    var altText = $(this).attr("alt");
    $(this).append("<span>" + altText + "</span>");
}, function () {
    $("#loginFirst span").remove();
});