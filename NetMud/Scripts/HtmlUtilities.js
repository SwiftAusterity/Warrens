function decodeHtml(html) {
    var txt = document.createElement("textarea");
    txt.innerHTML = html;
    return txt.value;
}

function GetQueryStringParams(sParam) {
    var sPageURL = window.location.search.substring(1);
    var sURLVariables = sPageURL.split('&');

    for (var i = 0; i < sURLVariables.length; i++) {
        var sParameterName = sURLVariables[i].split('=');

        if (sParameterName[0] == sParam) {
            return decodeURI(sParameterName[1]);
        }
    }
}

//Stuff always run on doc load
$(function () {

    //Automates modal forms a bit, a function by the data-submitName MUST exist on the page otherwise this will error
    var dialog = $("#modal-form").dialog({
        autoOpen: false,
        height: parseInt($(this).attr('data-height')),
        width: parseInt($(this).attr('data-width')),
        modal: true,
        buttons: {
            Cancel: function () {
                $(this).dialog("close");
            }
        }
    });

    var form = dialog.find("form").on("submit", function (event) {
        dialog.dialog("close");
    });

    $("#modal-form-open").button().on("click", function () {
        dialog.dialog("open");
    });
});