function AppendTextToOutput(output) {
    if (output === '' || output === undefined) {
        return;
    }

    var $outputArea = $("#OutputArea");

    $outputArea.append('<div>' + output + '</div>');
    $outputArea[0].scrollTop = $outputArea[0].scrollHeight;
}


function PlaySound(foleyUri) {
    if (foleyUri === '' || foleyUri === undefined || window.soundMuted) {
        return;
    }

    var audio = new Audio(foleyUri);

    audio.play();
}

function AppendOutput(output, UIOnly) {
    if (window.UILoading) {
        waitForUI();
    }

    $('[output-data-binding]').each(function () {
        var $this = $(this);
        var sourceKey = $this.attr('output-data-binding');
        var dataToAppend = getObjects(output, sourceKey, 0);

        if (dataToAppend !== undefined && dataToAppend.length > 0) {
            if (dataToAppend.length === 1 && dataToAppend[0] !== $this.html()) {
                $this.html(dataToAppend);

                //blink?
                $this.fadeOut(50, function () {
                    $this.fadeIn(100);
                });
            }
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

            if (dataToAppend !== undefined && dataToAppend.length > 0) {
                $this.html(dataToAppend);
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

    //Environment
    if (output.Environment !== '' && output.Environment !== undefined) {
        $('[output-binding="Environment.Visibility"]').each(function () {
            var $this = $(this);
            var value = output.Environment.Visibility;

            this.style.opacity = Math.min(1, value / 5);
            if ($this.attr('data-generated') === undefined) {
                MakeTooltip($this, "Brightness");
                LoadSVG("feather_see", this, true);
                $this.attr('data-generated', 'true');
            }
        });

        $('[output-binding="Environment.Sun"]').each(function () {
            var $this = $(this);
            var value = output.Environment.Sun;

            this.style.opacity = value;

            if ($this.attr('data-generated') === undefined) {
                $this.attr('title', "Solar Position");
                MakeTooltip($this, "Solar Position");
                LoadSVG("feather_radiate", this, true);
                $this.attr('data-generated', 'true');
            }
        });

        $('[output-binding="Environment.Moon"]').each(function () {
            var $this = $(this);
            var value = output.Environment.Moon;

            this.style.opacity = value;

            if ($this.attr('data-generated') === undefined) {
                $this.attr('title', "Lunar Position");
                MakeTooltip($this, "Lunar Position");
                LoadSVG("feather_glow", this, true);
                $this.attr('data-generated', 'true');
            }
        });

        $('[output-binding="Environment.TimeOfDay"]').each(function () {
            var $this = $(this);
            var timeString = output.Environment.TimeOfDay;
            var tempString = output.Environment.Temperature;
            var humidityString = output.Environment.Humidity;

            if (tempString !== undefined && tempString.length > 0) {
                timeString += ' (' + tempString + ' C)';
            }

            if (humidityString !== undefined && humidityString.length > 0) {
                timeString += ' (' + humidityString + ' bar)';
            }

            if (timeString !== undefined && timeString.length > 0) {
                $this.html(timeString);
            }
        });

        $('[output-binding="Environment.Weather"]').each(function () {
            var $this = $(this);
            var weatherCluster = output.Environment.Weather;
            var rainType = weatherCluster.Item2;
            var rainAmount = weatherCluster.Item1;
            var weatherFlags = weatherCluster.Item3;
            var titleString = "Conditions: ";
            var weatherIcon = '';
            var fill = 'blue';

            if (rainType !== undefined) {
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

            if (rainAmount !== undefined) {
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

            if (weatherFlags !== undefined && weatherFlags.length > 0) {
                weatherFlags.forEach(function (value) {
                    if (titleString !== "Conditions: ") {
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

            if (fill !== '') {
                $this.css('fill', fill);
            }
        });
    }

    //Output (just the map and partial map updates and text output)
    if (!UIOnly) {
        AppendTextToOutput(output.Occurrence);

        if (output.SoundToPlay !== undefined && output.SoundToPlay !== '' && output.SoundToPlay !== null) {
            PlaySound(output.SoundToPlay);
        }
    }
}

function waitForUI() {
    if (window.UILoading === true) {
        window.setTimeout(waitForUI, 100); /* this checks the flag every 100 milliseconds*/
    } 
}

function getObjects(collection, key, depth) {
    var objects = [];

    if (key !== undefined) {
        var keyHierarchy = key.split('.');

        if (collection !== undefined && collection[keyHierarchy[depth]] !== undefined) {
            if (depth === keyHierarchy.length - 1) {
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

    var w = window.open("", windowTitle);

    w.close();

    var NFW = window.open('/GameClient/ModularWindow', windowTitle, s, true);

    if (NFW !== null) {
        NFW.addEventListener('load', function () {
            var contentArea = this.document.querySelector('#contentArea');

            $(contentArea).attr('data-module-name', windowTitle);
            $(contentArea).children('ul').children('li#quadrantName').text(windowTitle);

            $(content).appendTo(contentArea);

            this.document.querySelectorAll('[output-data-binding]').forEach(function (element) {
                var $this = $(element);
                var sourceKey = $this.attr('output-data-binding');
                var dataToAppend = getObjects(window.lastOutput, sourceKey, 0);

                if (dataToAppend !== undefined && dataToAppend.length > 0) {
                    $this.html(dataToAppend);
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

function LoadSVG(fileName, containerElement, removeInner) {
    if (fileName === '' || fileName === undefined) {
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

                if (svgXml === null) {
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
    };

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
            };
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
    audioPlayer.play();
}

function changeLoopTrackMode(me) {
    var $me = $(me);
    var audioPlayer = $('#backgroundMusic')[0];

    window.loopTrack = !window.loopTrack;

    $('#backgroundMusic')[0].loop = window.loopTrack;

    if (window.loopTrack) {
        $me.children().attr('style', 'color: green;');
        audioPlayer.removeEventListener('ended', nextTrackLoop);
        AppendTextToOutput("Music looping toggled ON.");
   } else {
        $me.children().attr('style', 'color: red;');
        changePlaylist($('.audioTrackSelector')[window.currentTrack]);
        AppendTextToOutput("Music looping toggled OFF.");
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
        AppendTextToOutput("Gossip network toggled ON.");
   } else {
        $me.children().attr('style', 'color: red;');
        AppendTextToOutput("Gossip network toggled OFF.");
   }

    return false;
}

function bindClientCommand(name, funct) {
    window.clientCommands[name] = funct;
}