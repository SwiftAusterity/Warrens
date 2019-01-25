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
function initModal() {
    var dialogContainer = $("#modal-form");
    var dialogHeight = dialogContainer.attr('data-height');
    var dialogWidth = dialogContainer.attr('data-width');

    dialogContainer.attr('data-returnUrl', window.location);

    //Automates modal forms a bit, a function by the data-submitName MUST exist on the page otherwise this will error
    var dialog = dialogContainer.dialog({
        autoOpen: false,
        height: parseInt(dialogHeight),
        width: parseInt(dialogWidth),
        modal: true,
        buttons: {
            Cancel: function () {
                dialog.dialog("close");
                document.location = $("#modal-form").attr('data-returnUrl');
            }
        }
    });

    $("#modal-form-open").button().on("click", function () {
        dialog.dialog("open");
    });

    dialogContainer.on('dialogclose', function (event) {
        dialog.dialog("close");
        document.location = $("#modal-form").attr('data-returnUrl');
    });
}


function replacePart(me, newHtml, firstCount, rowsName) {
    var $me = $(me);

    var rI = parseInt($me.attr('data-current-row'));

    if (rI !== parseInt(firstCount)) {
        newHtml = newHtml.replaceAll('[' + firstCount + ']', '[' + rI + ']').replaceAll('_' + firstCount + '_', '_' + rI + '_');
    }

    $('#' + rowsName).append(newHtml);
   $me.attr('data-current-row', rI + 1);
}

String.prototype.replaceAll = function (search, replacement) {
    var target = this;
    return target.split(search).join(replacement);
};