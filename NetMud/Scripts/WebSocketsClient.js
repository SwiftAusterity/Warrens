$(document).ready(function () {
    var connection;

    TestBrowser();

    submitCharacter();

    //bind the change event of the currently selected character dropdown to call the ajax thingy to set the player's character
    $('#currentCharacter').change(function () {
        submitCharacter();
    });
});

function submitCharacter()
{
    var cscVal = $('#currentCharacter').val();

    $.post("GameAdmin/SelectCharacter/" + cscVal, function (data) { });
}

function submitCommand()
{
    var commandText = $('#input').val();

    connection.send(commandText);
}

function TestBrowser()
{
    if ('WebSocket' in window) {
        connection = new WebSocket('ws://localhost:2929/');

        connection.onopen = function () {
            //Send a small message to the console once the connection is established
            AppendOutput('Connection established.');
        }

        connection.onclose = function () {
            AppendOutput('Connection closed');
        }

        connection.onerror = function (error) {
            AppendOutput('Error detected: ' + error);
        }

        connection.onmessage = function (e) {
            var server_message = e.data;
            AppendOutput(server_message);
        }

        $('#input').bind("enterKey", function (e) {
            submitCommand();

            $(this).val('');
        }).keyup(function (e) {
            if (e.keyCode == 13) {
                $(this).trigger("enterKey");
            }
        }).focus();
    } else {
        //No support, this is a websock only client page, tell them that.
        AppendOutput('Your browser does not support the WebSockets protocol. Please visit us with one that does.');
    }
}

function AppendOutput(output)
{
    var $outputArea = $("#OutputArea");

    $outputArea.append(output);

    $outputArea[0].scrollTop = $outputArea[0].scrollHeight;
}