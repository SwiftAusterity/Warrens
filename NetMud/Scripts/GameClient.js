$(document).ready(function () {
    $('#input').bind("enterKey", function (e) {
        submitCommand();

        $(this).val('');
    }).keyup(function (e) {
        if (e.keyCode == 13) {
            $(this).trigger("enterKey");
        }
    }).focus();
});

function submitCommand()
{
    var commandText = $('#input').val();

    $.ajax({
        url: "/api/GameCommand/RenderCommand",
        data: {
            "command": commandText
        }
    }).done(function (output) {
        var $outputArea = $("#OutputArea");

        $outputArea.append(output);

        $outputArea[0].scrollTop = $outputArea[0].scrollHeight;
    });
}