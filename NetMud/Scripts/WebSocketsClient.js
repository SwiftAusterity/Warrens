$(document).ready(function () {
    window.connection;
    window.commandArray = ['look'];
    window.commandPointer = 0;
    window.lastOutput = '';
    window.openedWindows = [];
    window.UILoading = true;

    TestBrowser();

    submitCharacter();

    //bind the change event of the currently selected character dropdown to call the ajax thingy to set the player's character
    $('#currentCharacter').change(function () {
        submitCharacter();
    });
});

function submitCharacter() {
    var cscVal = $('#currentCharacter').val();

    $.post("Player/SelectCharacter/" + cscVal, function (data) {
    });
}

function submitCommand(overrideCommand) {
    var commandText = overrideCommand;
    var wipe = false;

    if (commandText === '') {
        commandText = $('#input').val();
        wipe = true;
    }

    window.connection.send(commandText);

    if (wipe) {
        window.commandArray.push(commandText);
        window.commandPointer = commandArray.length - 1;
    }
}

function TestBrowser() {
    if ('WebSocket' in window) {
        window.connection = new WebSocket('ws://localhost:2929');

        window.connection.onopen = function () {
            //Send a small message to the console once the connection is established
            AppendTextToOutput('Connection established with host.');
            $('#currentCharacter').prop('disabled', true);
        }

        window.connection.onclose = function () {
            AppendTextToOutput('Connection closed by host.');

            //Unlock this on close
            $('#currentCharacter').prop('disabled', false);
        }

        window.connection.onerror = function (error) {
            $('#currentCharacter').prop('disabled', false);
            AppendTextToOutput('Connection error detected: ' + error);
        }

        window.connection.onmessage = function (e) {
            var server_message = JSON.parse(e.data);

            window.lastOutput = server_message;
            AppendOutput(server_message);
        }

        $('#input').keydown(function (e) {
            switch (e.keyCode) {
                case 13: //enter
                    submitCommand();

                    $(this).val('');
                    break;
                case 27: //esc
                    $(this).val('');
                    e.preventDefault();
                    break;
                case 33: //pgup
                    $('#OutputArea').scrollTop(Math.max(0, $('#OutputArea')[0].scrollTop - 700));
                    e.preventDefault();
                    break;
                case 34: //pgdown
                    $('#OutputArea').scrollTop(Math.min($('#OutputArea')[0].scrollHeight, $('#OutputArea')[0].scrollTop + 700));
                    e.preventDefault();
                    break;
                case 35: //END
                    $('#OutputArea').scrollTop($('#OutputArea')[0].scrollHeight);
                    e.preventDefault();
                    break;
                case 36: //home
                    $('#OutputArea').scrollTop(0);
                    e.preventDefault();
                    break;
                case 38: //up
                    if (window.commandPointer === 0) {
                        $(this).val('');
                    }
                    else {
                        window.commandPointer = window.commandPointer - 1;
                        $(this).val(window.commandArray[window.commandPointer]);
                    }
                    e.preventDefault();
                    break;
                case 40: //down
                    if (window.commandPointer === window.commandArray.length - 1) {
                        $(this).val('');
                    }
                    else {
                        window.commandPointer = window.commandPointer + 1;
                        $(this).val(commandArray[window.commandPointer]);
                    }
                    e.preventDefault();
                    break;
            }
        }).focus();
    } else {
        //No support, this is a websock only client page, tell them that.
        AppendOutput('Your browser does not support the WebSockets protocol. Please visit us with one that does.');
    }
}

function AppendTextToOutput(output) {
    if (output === '' || output == undefined) {
        return;
    }

    var $outputArea = $("#OutputArea");

    $outputArea.append('<div>' + output + '</div>');
    $outputArea[0].scrollTop = $outputArea[0].scrollHeight;
};

function AppendOutput(output, UIOnly) {
    if (window.UILoading) {
        waitForUI();
    }

    $('[output-data-binding]').each(function () {
        var $this = $(this);
        var sourceKey = $this.attr('output-data-binding');
        var dataToAppend = getObjects(output, sourceKey, 0);

        if (dataToAppend != undefined && dataToAppend.length > 0) {
            $this.html(dataToAppend)
        }
    });

    $('[output-eval-code]').each(function () {
        var $this = $(this);
        var evalKey = $this.attr('output-eval-key');
        var functionCode = $this.attr('output-eval-code');
        var dataToAppend = getObjects(output, evalKey, 0);

        var funct = new Function('element', 'data', functionCode);

        return funct($this, dataToAppend);
    });

    window.openedWindows.forEach(function (win) {
        win.window.document.querySelectorAll('[output-data-binding]').forEach(function (element) {
            var $this = $(element);
            var sourceKey = $this.attr('output-data-binding');
            var dataToAppend = getObjects(output, sourceKey, 0);

            if (dataToAppend != undefined && dataToAppend.length > 0) {
                $this.html(dataToAppend)
            }
        });

        win.window.document.querySelectorAll('[output-eval-code]').forEach(function (element) {
            var $this = $(element);
            var evalKey = $this.attr('output-eval-key');
            var functionCode = $this.attr('output-eval-code');
            var dataToAppend = getObjects(output, evalKey, 0);

            var funct = new Function('element', 'data', functionCode);

            return funct($this, dataToAppend);
        });
    });

    if (!UIOnly) {
        AppendTextToOutput(output.Occurrence);
    }
}

function waitForUI() {
    if (window.UILoading == true) {
        window.setTimeout(waitForUI, 100); /* this checks the flag every 100 milliseconds*/
    } 
}


function getObjects(collection, key, depth) {
    var objects = [];

    if (key != undefined) {
        var keyHierarchy = key.split('.');

        if (collection != undefined && collection[keyHierarchy[depth]] != undefined) {
            if (depth == keyHierarchy.length - 1) {
                objects.push(collection[keyHierarchy[depth]]);
            }
            else {
                objects = objects.concat(getObjects(collection[keyHierarchy[depth]], key, depth + 1));
            }
        }
    }

    return objects;
}

function ReloadUI() {
    AppendOutput(window.lastOutput, true);
}

function openModularUI(width, height, windowTitle, content) {
    window.UILoading = true;

    var s = 'menubar=no, toolbar=no, location=no, resizable=no, scrollbars=yes, status=no, width = ' + width + ', height = ' + height;
    var styles = '';
    var scripts = '';

    var w = window.open("", windowTitle);

    w.close();

    var NFW = window.open('/GameClient/ModularWindow', windowTitle, s, true);

    if (NFW != null) {
        NFW.addEventListener('load', function () {
            var contentArea = this.document.querySelector('#contentArea');

            $(contentArea).attr('data-module-name', windowTitle);
            $(contentArea).children('ul').children('li#quadrantName').text(windowTitle);

            $(content).appendTo(contentArea);

            this.document.querySelectorAll('[output-data-binding]').forEach(function (element) {
                var $this = $(element);
                var sourceKey = $this.attr('output-data-binding');
                var dataToAppend = getObjects(window.lastOutput, sourceKey, 0);

                if (dataToAppend != undefined && dataToAppend.length > 0) {
                    $this.html(dataToAppend)
                }
            });

            this.document.querySelectorAll('[output-eval-code]').forEach(function (element) {
                var $this = $(element);
                var evalKey = $this.attr('output-eval-key');
                var functionCode = $this.attr('output-eval-code');
                var dataToAppend = getObjects(window.lastOutput, evalKey, 0);

                var funct = new Function('element', 'data', functionCode);

                return funct($this, dataToAppend);
            });
        }, false);

        NFW.blur();

        window.focus();

        NFW.resizeTo(width, height);

        NFW.focus();

        window.openedWindows.push({ 'window': NFW, 'name': windowTitle });
    }

    window.UILoading = false;
}
