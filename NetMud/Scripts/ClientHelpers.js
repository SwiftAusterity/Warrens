function AppendTextToOutput(output) {
    if (output === '' || output == undefined) {
        return;
    }

    var $outputArea = $("#parserClientOutput");

    $outputArea.append('<div>' + output + '</div>');
    $outputArea[0].scrollTop = $outputArea[0].scrollHeight;
};

function RedrawMap(fullMap) {
    if (fullMap === '' || fullMap == undefined) {
        return;
    }

    var $outputArea = $("#OutputArea");

    $outputArea.html(fullMap);

    SetupTileTips($('.tileTip'));
};

function PlaySound(foleyUri) {
    if (foleyUri === '' || foleyUri == undefined || window.soundMuted) {
        return;
    }

    var audio = new Audio(foleyUri);

    audio.play();
}

function ProcessPartials(deltas) {
    if (deltas === '' || deltas == undefined) {
        return;
    }

    //<a href='#' x-data='{2}' y-data='{3}' class='editData tile' title='Edit' style='color: {1}; background-color: {4}; {5}'><span>{0}</span></a>
    //[ ###, ###, string ]
    deltas.forEach(function (delta) {
        $("div[x-data='" + delta['Item1'] + "'][y-data='" + delta['Item2'] + "']").replaceWith(delta['Item3']);
    });

    if ($("div[content-name-data='you']").length === 0) {
        submitCommand('look');
    }

    SetupTileTips($('.tileTip'));
};

function ProcessInventory(inventory) {
    if (inventory === '' || inventory == undefined) {
        return;
    }

    var container = $('#inventoryContainer');

    container.html('');

    var itemCount = 0;
    inventory.forEach(function (itemToDisplay) {
        var name = itemToDisplay['Name'];
        var icon = itemToDisplay['Icon'];
        var color = itemToDisplay['Color'];
        var desc = itemToDisplay['Description'];
        var uses = itemToDisplay['UseNames'];
        var stackSize = itemToDisplay['StackSize'];

        var newItem = $('<div />');

        newItem.attr('class', 'inventoryItem');
        newItem.attr('data-inventory-slot', itemCount);
        newItem.attr('data-inventory-name', name.toLowerCase());
        newItem.attr('data-inventory-stack-size', stackSize);

        if (uses.isArray) {
            newItem.attr('data-uses', uses);
        } else {
            newItem.attr('data-uses', uses.toString().toLowerCase());
        }

        newItem.html('<span style="color: ' + color + ';">' + icon + '</span>')

        newItem.ready(function () {
            newItem.click(function () {
                var usesList = $('#useList');
                if ($(this).hasClass('selected')) {
                    $(this).removeClass('selected');
                    window.selectedInventoryItem = -1;
                    usesList.css('display', 'none');
                    return;
                }

                $('.inventoryItem.selected').removeClass('selected');
                $(this).addClass('selected');
                window.selectedInventoryItem = $(this).attr('data-inventory-slot');

                var usesHtml = '<span>Uses: </span>';

                uses.forEach(function (use) {
                    usesHtml = usesHtml + '<span>' + use.toLowerCase() + '</span>';
                });

                usesList.html(usesHtml);
                usesList.css('display', 'inline-flex');
            });

            var options = {
                title: desc,
                trigger: 'hover',
                template: '<div class="tooltip" role="tooltip"><div class="tooltip-arrow"></div><div class="tooltip-inner"></div></div>',
                offset: 5,
                popperOptions: {
                    removeOnDestroy: true,
                    placements: 'auto',
                }
            };

            window.tooltips.push(new Tooltip(newItem, options));
        });

        if (window.selectedInventoryItem == itemCount) {
            newItem.addClass('selected');
        }

        itemCount++;
        container.append(newItem);
    });

    var i = 0;
    var itemTotal = itemCount;

    for (i = 0; i < 50 - itemTotal; i++) {

        var emptyBox = $('<div />');

        emptyBox.attr('data-inventory-slot', i + itemCount);
        emptyBox.attr('class', 'inventoryItem empty');
        emptyBox.css('opacity', '.25');

        emptyBox.html('<span>&nbsp;</span>');

        container.append(emptyBox);
    }
}

function ProcessQualities(qualities) {
    if (qualities === '' || qualities == undefined) {
        return;
    }

    var container = $('#qualitiesContainer');

    container.html('');

    qualities.forEach(function (itemToDisplay) {
        var visible = itemToDisplay['Visible'];

        if (visible === 'False') {
            return;
        }

        var name = itemToDisplay['Name'];
        var value = itemToDisplay['Value'];

        var newItem = $('<div />');

        newItem.attr('class', 'qualityItem');
        newItem.attr('title', name + ' : ' + value);

        newItem.html('<span>' + value + '</span>');

        newItem.ready(function () {
            var options = {
                title: name + ' : ' + value,
                trigger: 'hover',
                template: '<div class="tooltip" role="tooltip"><div class="tooltip-arrow"></div><div class="tooltip-inner"></div></div>',
                offset: 5,
                popperOptions: {
                    removeOnDestroy: true,
                    placements: 'auto',
                }
            };

            window.tooltips.push(new Tooltip(newItem, options));
        });


        container.append(newItem);
    });
}

function AppendOutput(output, UIOnly) {
    if (window.UILoading) {
        waitForUI();
    }

    //Process the UI stuff if we have it
    //Self
    if (output.Self !== '' && output.Self != undefined) {
        $('[output-data-binding="Self.CurrentHealth"]').each(function () {
            var $this = $(this);
            var value = output.Self.CurrentHealth / output.Self.TotalHealth * 100;

            $this.ready(function () {
                var options = {
                    title: output.Self.CurrentHealth + '/' + output.Self.TotalHealth,
                    trigger: 'hover',
                    template: '<div class="tooltip" role="tooltip"><div class="tooltip-arrow"></div><div class="tooltip-inner"></div></div>',
                    offset: 5,
                    popperOptions: {
                        removeOnDestroy: true,
                        placements: 'auto',
                    }
                };

                window.tooltips.push(new Tooltip($this, options));
            });

            $this.css('width', value + '%');
        });

        $('[output-data-binding="Self.CurrentStamina"]').each(function () {
            var $this = $(this);
            var value = output.Self.CurrentStamina / output.Self.TotalStamina * 100;

            $this.ready(function () {
                var options = {
                    title: output.Self.CurrentStamina + '/' + output.Self.TotalStamina,
                    trigger: 'hover',
                    template: '<div class="tooltip" role="tooltip"><div class="tooltip-arrow"></div><div class="tooltip-inner"></div></div>',
                    offset: 5,
                    popperOptions: {
                        removeOnDestroy: true,
                        placements: 'auto',
                    }
                };

                window.tooltips.push(new Tooltip($this, options));
            });

            $this.css('width', value + '%');
        });

        ProcessQualities(output.Self.Qualities);
    }

    //Local
    if (output.Local !== '' && output.Local != undefined) {
        $('[output-data-binding="Local.ZoneName"]').each(function () {
            var $this = $(this);
            var dataToAppend = output.Local.ZoneName;

            if (dataToAppend != undefined && dataToAppend.length > 0) {
                $this.html(dataToAppend);
            }
        });

        $('[output-data-binding="Local.LocationDescriptive"]').each(function () {
            var $this = $(this);
            var dataToAppend = output.Local.LocationDescriptive;

            if (dataToAppend != undefined && dataToAppend.length > 0) {
                $this.html(dataToAppend);
            }
        });
    }

    //Environment
    if (output.Environment !== '' && output.Environment != undefined) {
        $('[output-data-binding="Environment.Visibility"]').each(function () {
            var $this = $(this);
            var value = output.Environment.Visibility;

            this.style.opacity = Math.max(1, value / 5);
            if ($this.attr('data-generated') == undefined) {
                MakeTooltip($this, "Brightness");
                LoadSVG("feather_see", this, true);
                $this.attr('data-generated', 'true');
            }
        });

        $('[output-data-binding="Environment.Sun"]').each(function () {
            var $this = $(this);
            var value = output.Environment.Sun;

            this.style.opacity = value;

            if ($this.attr('data-generated') == undefined) {
                $this.attr('title', "Solar Position");
                MakeTooltip($this, "Solar Position");
                LoadSVG("feather_radiate", this, true);
                $this.attr('data-generated', 'true');
            }
        });

        $('[output-data-binding="Environment.Moon"]').each(function () {
            var $this = $(this);
            var value = output.Environment.Moon;

            this.style.opacity = value;

            if ($this.attr('data-generated') == undefined) {
                $this.attr('title', "Lunar Position");
                MakeTooltip($this, "Lunar Position");
                LoadSVG("feather_glow", this, true);
                $this.attr('data-generated', 'true');
            }
        });

        $('[output-data-binding="Environment.TimeOfDay"]').each(function () {
            var $this = $(this);
            var timeString = output.Environment.TimeOfDay;
            var tempString = output.Environment.Temperature;
            var humidityString = output.Environment.Humidity;

            if (tempString != undefined && tempString.length > 0) {
                timeString += ' (' + tempString + ' C)';
            }

            if (humidityString != undefined && humidityString.length > 0) {
                timeString += ' (' + humidityString + ' bar)';
            }

            if (timeString != undefined && timeString.length > 0) {
                $this.html(timeString);
            }
        });

        $('[output-data-binding="Environment.Weather"]').each(function () {
            var $this = $(this);
            var weatherCluster = output.Environment.Weather;
            var rainType = weatherCluster.Item2;
            var rainAmount = weatherCluster.Item1;
            var weatherFlags = weatherCluster.Item3;
            var titleString = "Conditions: ";
            var weatherIcon = '';
            var fill = 'blue';

            if (rainType != undefined) {
                switch (rainType) {
                    case 'Clear':
                        weatherIcon = '';
                        break;
                    case 'Rain':
                        fill = 'blue';
                        titleString += "Raining";
                        break;
                    case 'Acid':
                        fill = 'green';
                        titleString += "Acid raining";
                        break;
                    case 'Sleet':
                        fill = 'antiquewhite';
                        titleString += "Sleeting";
                        break;
                    case 'Snow':
                        fill = 'white';
                        titleString += "Snowing";
                        break;
                    case 'Hail':
                        fill = 'slategrey';
                        titleString += "Hailing";
                        break;
                }
            }

            if (rainAmount != undefined) {
                weatherIcon = 'feather_rain';

                switch (rainAmount) {
                    case 'Clear':
                        this.style.opacity = 0;
                        weatherIcon = '';
                        break;
                    case 'Drizzle':
                        this.style.opacity = .2;
                        break;
                    case 'Steady':
                        this.style.opacity = .4;
                        break;
                    case 'Downpour':
                        this.style.opacity = .6;
                        break;
                    case 'Torrential':
                        this.style.opacity = 1;
                        break;
                }
            }

            if (weatherFlags != undefined && weatherFlags.length > 0) {
                weatherFlags.forEach(function (value, index) {
                    if (titleString != "Conditions: ") {
                        titleString += ', ';
                    }

                    titleString += value;
                });
            }

            $this.attr('title', titleString);
            MakeTooltip($this, titleString);

            if (weatherIcon.length > 0) {
                LoadSVG(weatherIcon, this, true);
            }

            if (fill != '') {
                $this.css('fill', fill);
            }
        });
    }

    //Output (just the map and partial map updates and text output)
    if (!UIOnly) {
        AppendTextToOutput(output.Occurrence);
        RedrawMap(output.FullMap);
        ProcessPartials(output.VisibleMapDeltas);
        ProcessInventory(output.Inventory);

        PlaySound(output.SoundToPlay);
    }
}

function waitForUI() {
    if (window.UILoading == true) {
        window.setTimeout(waitForUI, 100); /* this checks the flag every 100 milliseconds*/
    }
}

function ReloadUI() {
    AppendOutput(window.lastOutput, true);
}

function LoadSVG(fileName, containerElement, removeInner) {
    if (fileName === '' || fileName == undefined) {
        return;
    }

    if (!fileName.startsWith("/content/images/icons/")) {
        fileName = "/content/images/icons/" + fileName;
    }

    if (!fileName.endsWith(".svg")) {
        fileName = fileName + ".svg";
    }

    xhr = new XMLHttpRequest();
    xhr.open("GET", fileName, false);
    //xhr.overrideMimeType("image/svg+xml");

    xhr.onload = function () {
        if (xhr.readyState === 4) {
            if (xhr.status === 200) {
                if (removeInner) {
                    $(containerElement).empty();
                }

                var svgXml = xhr.responseXML.documentElement;

                if (svgXml == null) {
                    return;
                }

                containerElement.appendChild(svgXml);
            }
        }
    };

    xhr.send(null);
}

function SetupTileTips($entities) {
    $entities.click(function () {
        var $this = $(this);
        var tipElement = this;
        var x = $this.attr('x-data');
        var y = $this.attr('y-data');
        var zoneId = $this.attr('zone-data');

        $.get('/api/ClientDataApi/GetTileInfo/' + zoneId + '/' + x + '/' + y, function (tipInfo) {
            MakeTipper($this, tipInfo, tipElement);
        });
    });
}

function MakeTooltip(parent, text) {
    var options = {
        title: text,
        trigger: 'hover',
        template: '<div class="tooltip" role="tooltip"><div class="tooltip-arrow"></div><div class="tooltip-inner"></div></div>',
        popperOptions: {
            positionFixed: true,
            position: 'top'
        }
    };

    var instance = new Tooltip(parent, options);
}

function MakeTipper($this, tipInfo, tipElement) {
    var poptions = {
        positionFixed: true,
        position: 'auto',
        removeOnDestroy: true
    }

    $this.append(decodeHtml(tipInfo));

    var pop = new Popper(tipElement, $this.find('div.helpItem'), poptions);
    $this.find('div.helpItem').click(function () {
        pop.destroy();
        return false;
    });
}

function GetEntityInfo(type, key) {

    $entities.click(function () {
        $.get('/api/ClientDataApi/GetEntityInfo/' + key + '/' + type, function (tipInfo) {
            var poptions = {
                positionFixed: true,
                position: 'auto',
                removeOnDestroy: true,
                onCreate: function (data) {
                    $(data.instance.popper).click(function () { $(this).hide(); });
                }
            }
            var pop = new Popper(this, decodeHtml(tipInfo), poptions);
            pop.show();
        });
    });
}


function changeTrack(me) {
    var $me = $(me);
    var newTrackUri = $me.attr('data-song-uri');
    var playlistIndex = $me.attr('data-playlist-index');
    window.currentTrack = parseInt(playlistIndex);

    var audioPlayer = $('#backgroundMusic')[0];

    if (!window.musicMuted) {
        audioPlayer.pause();
    }

    $('#backgroundMusic').attr('src', newTrackUri);

    audioPlayer.removeEventListener('ended', nextTrackLoop);
    audioPlayer.removeEventListener('ended', nextPlaylistLoop);
   if (!window.loopTrack) {
        audioPlayer.addEventListener('ended', nextTrackLoop);
    }

    $('.songSelection').attr('class', 'glyphicon glyphicon-volume-off songSelection');
    $('.songSelection').attr('style', 'color: red;');

    $me.children().attr('class', 'glyphicon glyphicon-volume-up songSelection');
    $me.children().attr('style', 'color: green;');

    if (!window.musicMuted) {
        audioPlayer.play();
    }

    return false;
}

function nextTrackLoop() {
    var audioPlayer = $('#backgroundMusic')[0];
    var maxTracks = $('.audioTrackSelector').length;

    var i = window.currentTrack + 1;

    if (i >= maxTracks) {
        i = 0;
    }

    window.currentTrack = i;

    var trackItem = $('.audioTrackSelector[data-playlist-index=' + i + ']');

    audioPlayer.src = trackItem.attr('data-song-uri');

    $('.songSelection').attr('class', 'glyphicon glyphicon-volume-off songSelection');
    $('.songSelection').attr('style', 'color: red;');

    trackItem.children().attr('class', 'glyphicon glyphicon-volume-up songSelection');
    trackItem.children().attr('style', 'color: green;');

    audioPlayer.pause();
    audioPlayer.load();
    audioPlayer.play();
}

function changePlaylist(me) {
    var $me = $(me);
    var audioPlayer = $('#backgroundMusic')[0];
    var playlistIndex = $me.attr('data-playlist-index');
    window.currentPlaylist = playlistIndex;
    var playlist = window.playlists[playlistIndex];

    audioPlayer.loop = false;

    if (!window.musicMuted) {
        audioPlayer.pause();
    }

    $('#backgroundMusic').attr('src', playlist.songs[0]);
    window.currentPlaylistTrack = 0;

    audioPlayer.removeEventListener('ended', nextTrackLoop);
    audioPlayer.removeEventListener('ended', nextPlaylistLoop);
    audioPlayer.addEventListener('ended', nextPlaylistLoop);

    $('.songSelection').attr('class', 'glyphicon glyphicon-volume-off songSelection');
    $('.songSelection').attr('style', 'color: red;');

    $me.children().attr('class', 'glyphicon glyphicon-volume-up songSelection');
    $me.children().attr('style', 'color: green;');

    if (!window.musicMuted) {
        audioPlayer.load();
        audioPlayer.play();
    }

    return false;
}

function nextPlaylistLoop() {
    var audioPlayer = $('#backgroundMusic')[0];
    var i = window.currentPlaylistTrack + 1;

    var playlist = window.playlists[window.currentPlaylist];

    if (playlist.songs.length <= i) {
        i = 0;
    }

    window.currentPlaylistTrack = i;
    audioPlayer.src = playlist.songs[i];
    audioPlayer.pause();
    audioPlayer.load();
    thaudioPlayeris.play();
}

function changeLoopTrackMode(me) {
    var $me = $(me);
    var audioPlayer = $('#backgroundMusic')[0];

    window.loopTrack = !window.loopTrack;

    $('#backgroundMusic')[0].loop = window.loopTrack;

    if (window.loopTrack) {
        $me.children().attr('style', 'color: green;');
        audioPlayer.removeEventListener('ended', nextTrackLoop);
    } else {
        $me.children().attr('style', 'color: red;');
        changePlaylist($('.audioTrackSelector')[window.currentTrack]);
    }

    return false;
}

function changeTutorialMode(me) {
    var $me = $(me);

    window.tutorialMode = !window.tutorialMode;

    $.post('/api/ClientDataApi/ToggleTutorialMode');

    if (window.tutorialMode) {
        $me.children().attr('style', 'color: green;');
    } else {
        $me.children().attr('style', 'color: red;');
        $('.tutorialContainer').remove();
    }

    return false;
}

function changeGossipMode(me) {
    var $me = $(me);

    window.gossipMode = !window.gossipMode;

    $.post('/api/ClientDataApi/ToggleGossipParticipation');

    if (window.gossipMode) {
        $me.children().attr('style', 'color: green;');
    } else {
        $me.children().attr('style', 'color: red;');
    }

    return false;
}
