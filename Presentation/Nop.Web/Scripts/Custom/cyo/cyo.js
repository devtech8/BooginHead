﻿$(function () {

    $("#cyoOverlay").draggable();

    // Custom pattern menu
    $.each($('#cyoPatternMenu div'), function (index, element) {
        var url = $(element).attr('data-image-url');
        $(element).css('background-image', 'url("' + url + '")');
        $(element).click(function () {
            $('#cyoPatternMenu div').removeClass('selected');
            $(this).addClass('selected');
            $('#cyoCustomPattern').attr('value', url);
            $('#cyoSample').css('background-image', 'url("' + url.replace(/_thumb/, '') + '")');
        });
    });

    // Custom font menu
    $.each($('#cyoFontMenu div'), function (index, element) {
        var font = $(element).text();
        $(element).css('font-family', font);
        $(element).css('font-size', '40px');
        $(element).click(function () {
            $('#cyoFontMenu div').removeClass('selected');
            $(this).addClass('selected');
            $('#cyoCustomFont').attr('value', font);
            $('#cyoOverlay').css('font-family', font);
        });
    });

    // Font size
    $('#cyoCustomFontSize').change(function () {
        $('#cyoOverlay').css('font-size', $(this).val());
    });

    // Custom text
    $('#cyoCustomText').keyup(function () {
        var text = $(this).val().replace(/\n/, "<br/>");
        $('#cyoOverlay').html(text);
    });

    // Font color
    $('#cyoCustomColor').change(function () {
        var hexColor = '#' + document.getElementById('cyoCustomColor').color.toString();
        $('#cyoOverlay').css('color', hexColor);
    });

});
