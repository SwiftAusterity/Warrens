$(document).ready(function () {
    if (window.soundMuted) {
        $("#muteSounds").children().attr('class', 'glyphicon glyphicon-volume-off');
        $("#muteSounds").children().attr('style', 'color: red;');
    }
    else {
        $("#muteSounds").children().attr('class', 'glyphicon glyphicon-volume-up');
        $("#muteSounds").children().attr('style', 'color: green;');
    }

    if (window.musicMuted) {
        $("#muteMusic").children().attr('class', 'glyphicon glyphicon-volume-off');
        $("#muteMusic").children().attr('style', 'color: red;');
    }
    else {
        $("#muteMusic").children().attr('class', 'glyphicon glyphicon-volume-up');
        $("#muteMusic").children().attr('style', 'color: green;');
    }

    if ($('.audioTrackSelector') !== undefined) {
        changeTrack($('.audioTrackSelector')[0]);
    }

    Tutorial($('#locationBreadcrumbs'), "This is where you are in the world.", window.tutorialMode);
    Tutorial($('.statusIndicators'), "The current level of illumination in the area as well as weather conditions and sun/moon cycle indicators.", window.tutorialMode);
    Tutorial($('.inputContainer'), "This is where you type commands. Use the INTERACT command to interact with things.", window.tutorialMode);
    Tutorial($('#userControls'), "Music tracks (and muting controls) in addition to disconnect/reconnect can be found here.", window.tutorialMode);
    Tutorial($('#healthBars'), "Health and stamina levels are shown here.", window.tutorialMode);
    Tutorial($('#inventoryContainers'), "Your inventory will show up here.", window.tutorialMode);

    $('#disconnect').click(function (e) {
        $('#input').val('');
        $("#parserClientOutput").html('');
        $("#parserClientOutput")[0].scrollTop = $("#parserClientOutput")[0].scrollHeight;

        AppendTextToOutput('Connection TERMINATED.');
        window.connection.close();

        $('disconnect').off('click');
        return false;
    });

    $("#clientReload").click(function (e) {
        ReloadUI();
        return false;
    });

    $("#keyMap").click(function (e) {
        $('.keyLegend').toggleClass('expanded');
        return false;
    });

    $('#loopTracks').click(function () {
        changeLoopTrackMode(this);
        return false;
    });


    $("#muteSounds").click(function (e) {
        window.soundMuted = !soundMuted;

        $.post('/api/ClientDataApi/ToggleSoundMute');

        if (window.soundMuted) {
            $(this).children().attr('class', 'glyphicon glyphicon-volume-off');
            $(this).children().attr('style', 'color: red;');
        }
        else {
            $(this).children().attr('class', 'glyphicon glyphicon-volume-up');
            $(this).children().attr('style', 'color: green;');
        }

        return false;
    });

    $("#muteMusic").click(function (e) {
        window.musicMuted = !musicMuted;

        $.post('/api/ClientDataApi/ToggleMusicMute');

        if (window.musicMuted) {
            $('#backgroundMusic')[0].pause();
            $(this).children().attr('class', 'glyphicon glyphicon-volume-off');
            $(this).children().attr('style', 'color: red;');
        }
        else {
            $('#backgroundMusic')[0].play();
            $(this).children().attr('class', 'glyphicon glyphicon-volume-up');
            $(this).children().attr('style', 'color: green;');
        }

        return false;
    });

    TestBrowser();
});

function submitCommand(overrideCommand) {
    var commandText = overrideCommand;
    var wipe = false;

    if (commandText === '' || commandText === undefined) {
        commandText = $('#input').val();

        var selectedItem = $('.inventoryItem.selected');

        //we have a selected item, are we trying to "use" it
        if (selectedItem.length > 0) {
            var uses = selectedItem.attr('data-uses');
            var firstCommandWord = commandText.substring(0, commandText.indexOf(' ')).toLowerCase();

            if (firstCommandWord === '' && commandText !== '') {
                firstCommandWord = commandText;
            }

            //add in "use objectName" so the command we're sending becomes "use command item direction"
            if (uses.includes(firstCommandWord)) {
                if (selectedItem.attr('data-inventory-stack-size') > 1) {
                    commandText = 'use 1."' + selectedItem.attr('data-inventory-name') + '" ' + commandText;
                } else {
                    commandText = 'use "' + selectedItem.attr('data-inventory-name') + '" ' + commandText;
                }
            }
        }

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
        var protocol = window.location.protocol.replace('http', 'ws');

        window.connection = new WebSocket(protocol + '//' + window.location.hostname + ':' + window.location.port + '/WebSocketService.ashx');

        window.connection.onopen = function () {
            //Send a small message to the console once the connection is established
            AppendTextToOutput('Connection established with host.');
            $('#currentCharacter').prop('disabled', true);
        };

        window.connection.onclose = function () {
            AppendTextToOutput('Connection closed by host.');

            //Unlock this on close
            $('#currentCharacter').prop('disabled', false);
        };

        window.connection.onerror = function (error) {
            $('#currentCharacter').prop('disabled', false);
            AppendTextToOutput('Connection error detected: ' + error);
        };

        window.connection.onmessage = function (e) {
            var server_message = JSON.parse(e.data);

            window.lastOutput = server_message;
            AppendOutput(server_message);
        };

        window.addEventListener('keydown', function (e) {
            switch (e.keyCode) {
                case 18:
                    if (!window.stopToggle) {
                        window.stopToggle = true;
                        window.tooltips.forEach(function (tooltip) {
                            tooltip.show();
                        });
                        ToggleBatmanMode();
                    }
                    e.preventDefault();
                    break;
                case 13: //enter
                    $('#input').focus();
                    e.preventDefault();
                    break;
                case 27: //esc
                    $('#input').val('');
                    e.preventDefault();
                    break;
                case 33: //pgup
                    $('#parserClientOutput').scrollTop(Math.max(0, $('#parserClientOutput')[0].scrollTop - 700));
                    e.preventDefault();
                    break;
                case 34: //pgdown
                    $('#parserClientOutput').scrollTop(Math.min($('#parserClientOutput')[0].scrollHeight, $('#parserClientOutput')[0].scrollTop + 700));
                    e.preventDefault();
                    break;
                case 35: //END
                    $('#parserClientOutput').scrollTop($('#parserClientOutput')[0].scrollHeight);
                    e.preventDefault();
                    break;
                case 36: //home
                    $('#parserClientOutput').scrollTop(0);
                    e.preventDefault();
                    break;
                case 104: //up num
                    submitCommand('north');
                    e.preventDefault();
                    break;
                case 98: //down num
                    submitCommand('south');
                    e.preventDefault();
                    break;
                case 100: //left num
                    submitCommand('west');
                    e.preventDefault();
                    break;
                case 102: //right num
                    submitCommand('east');
                    e.preventDefault();
                    break;
                case 105: //upright num
                    submitCommand('northeast');
                    e.preventDefault();
                    break;
                case 103: //upleft num
                    submitCommand('northwest');
                    e.preventDefault();
                    break;
                case 99: //downright num
                    submitCommand('southeast');
                    e.preventDefault();
                    break;
                case 97: //downleft num
                    submitCommand('southwest');
                    e.preventDefault();
                    break;
                case 101: //middle num
                    //Batman mode
                    ToggleBatmanMode();
                    e.preventDefault();
                    break;
                case 37: //left 
                case 39: //right
                    var currentItemNum = window.selectedInventoryItem;
                    var nextItemNum = currentItemNum;

                    if (e.keyCode === 37) {
                        nextItemNum = nextItemNum - 1;
                    } else {
                        nextItemNum = nextItemNum + 1;
                    }

                    var usesList = $('#useList');
                    if (nextItemNum < 0 || nextItemNum > 49) {
                        window.selectedInventoryItem = -1;
                        $('.inventoryItem.selected').removeClass('selected');
                        usesList.css('display', 'none');
                        return;
                    }

                    var newItem = $('.inventoryItem[data-inventory-slot="' + nextItemNum + '"]');

                    if (newItem.hasClass('empty')) {
                        return;
                    }

                    $('.inventoryItem.selected').removeClass('selected');

                    newItem.addClass('selected');
                    window.selectedInventoryItem = nextItemNum;

                    var uses = newItem.attr('data-uses');

                    var usesHtml = '<span>Uses: </span>';

                    if (uses.isArray) {
                        uses.forEach(function (use) {
                            usesHtml = usesHtml + '<span>' + use + '</span>';
                        });
                    }
                    else {
                        usesHtml = usesHtml + '<span>' + uses + '</span>';
                    }

                    usesList.html(usesHtml);
                    usesList.css('display', 'inline-flex');
                    break;
            }
        });

        window.addEventListener('keyup', function (e) {
            switch (e.keyCode) {
                case 18:
                    if (window.stopToggle) {
                        window.stopToggle = false;
                        window.tooltips.forEach(function (tooltip) {
                            tooltip.hide();
                        });
                        ToggleBatmanMode();
                    }
                    e.preventDefault();
                    break;
            }
        });

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

function ToggleBatmanMode() {
    window.BatmanMode = !window.BatmanMode;

    if (window.BatmanMode) {
        $('div.tile').each(function () {
            var $this = $(this);

            var style = $this.attr('style');

            $this.attr('data-oldStyle', style);
        });

        $('div.tile').attr('style', 'background-color: black !important; color: white !important; border-color: white !important');
    } else {
        $('div.tile').each(function () {
            var $this = $(this);

            var style = $this.attr('data-oldStyle');

            $this.attr('style', style);
        });
    }
}