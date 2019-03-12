function Tutorial(parent, text, tutorialMode) {
    if (!tutorialMode) {
        return;
    }

    parent.ready(function () {
        var parentCoords = parent[0].getBoundingClientRect();

        var modal = $('<div />');
        modal.html('<div class="glyphicon glyphicon-info-sign tutorialOverlayTip" title="Tutorial Tip"></div>');
        modal.css('display', 'inline-flex');
        modal.css('left', parentCoords.left);
        modal.css('width', parentCoords.width + 'px');

        if (parentCoords.height > 0) {
            modal.css('height', parentCoords.height + 'px');
        }
        modal.attr('class', 'tutorialContainer');

        parent.append(modal);

        modal.ready(function () {
            var options = {
                title: text,
                trigger: 'hover',
                template: '<div class="tooltip" role="tooltip"><div class="tooltip-arrow"></div><div class="tooltip-inner"></div></div>',
                offset: 5,
                popperOptions: {
                    removeOnDestroy: true,
                    placements: 'auto'
                }
            };

            var instance = new Tooltip(modal, options);

            modal.click(function () {
                instance.dispose();
                $(this).remove();
            });
        });
    });
}

function HelpTipTutorial(parent, text) {
    var options = {
        title: text,
        trigger: 'click',
        template: '<div class="tooltip" role="tooltip"><div class="tooltip-arrow"></div><div class="tooltip-inner"></div></div>',
        offset: 5,
        popperOptions: {
            placements: 'auto'
        }
    };

    var instance = new Tooltip(parent, options);
}

function openFrameless(width, height, fromLeft, fromTop, targetUrl, windowTitle) {
    var s = 'menubar=no, toolbar=no, location=no, resizable=no, scrollbars=yes, status=no, width = ' + width + ', height = ' + height;

    if (windowTitle === '') {
        windowTitle = 'adminModal';
    }

    var NFW = window.open(targetUrl, windowTitle, s);

    NFW.blur();

    window.focus();

    NFW.resizeTo(width, height);

    NFW.moveTo(fromLeft, fromTop);

    NFW.focus();

    var timer = setInterval(function () {
        if (NFW.closed) {
            clearInterval(timer);
            submitFrameless(window);
        }
    }, 1000);
}

function submitFrameless(baseDocument) {
    baseDocument.location.reload(false);
}

function renumberControlArray(wrapperParent) {
    var $wrapperParent = $(wrapperParent);

    var inputChild = $wrapperParent.find('input')[0];

    var bracketIndex = inputChild.name.indexOf("[");
    var nameValue = Number.parseInt(inputChild.name.substr(bracketIndex + 1, 1));

    $wrapperParent.nextAll().find('input').each(function (index) {
        var nameFirstHalf = this.name.substr(0, this.name.indexOf("[") + 1);
        var nameSecondHalf = this.name.substr(this.name.indexOf("[") + 2);

        this.name = nameFirstHalf + (nameValue + index) + nameSecondHalf;
    });

    //Get rid of the element
    $wrapperParent.remove();
}