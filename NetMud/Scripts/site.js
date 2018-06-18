function Tutorial(parent, text) {
    if (Cookies.get('tutorialMode') === 'off') {
        return;
    }

    var options = {
        title: text,
        trigger: 'hover',
        template: '<div class="tooltip" role="tooltip"><div class="tooltip-arrow"></div><div class="tooltip-inner"></div></div>',
        offset: 5,
        popperOptions: {
            removeOnDestroy: true,
            placements: 'auto',
        }
    };

    var instance = new Tooltip(parent, options);
    instance.show();

    parent.on('mouseenter', function () {
        instance.dispose();
        parent.off('mouseenter');
    });
}
