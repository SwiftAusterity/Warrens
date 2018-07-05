(function () {
    if (document.getElementById('guildWebRing' === undefined)) {
        return;
    }

    var ringHtml =
        '<div style="width: 250px; height: 100px; padding: 10px; border-radius: 15px; max-width: 250px; max-height: 100px; text-align: center; font-size: larger; background-color: navajowhite">' +
        '<div style="padding: 10px 0; max-height: 70px;">' +
        '<a href="#"><span class="glyphicon glyphicon-chevron-left" style="float: left; padding-top: 15px;" title="previous member"></span></a>' +
        '<span id="thisRing">' +
        '<img src="https://upload.wikimedia.org/wikipedia/commons/d/dc/Sylvilagus_floridanus_14136.JPG" style = "max-height: 50px; max-width: 150px" title = "Under the Eclipse" />' +
        '</span>' +
        '<a href="#"><span class="glyphicon glyphicon-chevron-right" style="float: right; padding-top: 15px;" title="next member"></span></a>' +
        '</div>' +
        '<span style="font-size: smaller"><a href="https://mudcoders.com/">MUD Coders Guild Web Ring</a></span>' +
        '</div>';

    document.getElementById('guildWebRing').innerHTML = ringHtml;
})();