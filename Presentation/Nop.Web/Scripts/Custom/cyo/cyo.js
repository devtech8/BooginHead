﻿$(function () {

    $("#cyoOverlay").draggable();

    $('.chooser.shields img').click(function () {
        $('#cyoSample img').attr('src', $(this).attr('data-large-image'));
    });

    // Load clicked image into pacifier
    $('.chooser img').click(function () {
        var url = $(this).attr('src');
        //$('#cyoCustomPattern').attr('value', url);
        $('#cyoSample').css('background-image', 'url("' + url.replace(/_thumb/, '') + '")');
    });
    
    // Clear image under pacifier
    $('.btn-clear-design').click(function () {
        $('#cyoSample').css('background-image', 'none');
    });

    // Wire up tabs to show/hide image divs
    $('#tabs li').click(function () {
        var tabNumber = $(this).attr('aria-controls').split('-')[1];
        console.log(tabNumber);
        for (var i = 1; i < $('#tabs li').length; i++) {
            $('#tabs-' + i).hide();
        }
        $('#tabs-' + tabNumber).show();
        $('#tabs li').removeClass('ui-tabs-active').removeClass('ui-state-active');
        $(this).addClass('ui-tabs-active').addClass('ui-state-active');
    });


    // Set custom text as user types
    $('#cyoCustomText').keyup(function () {
        var text = $(this).val().replace(/\n/, "<br/>");
        $('#cyoOverlay').html(text);
    });

    // Clear custom text when user clicks Clear button
    $('#cyoClearText').click(function () {
        $('#cyoOverlay').html('');
        $('#cyoCustomText').val('');
    });

    // Custom font menu
    $('#cyoFontMenu li').click(function () {
        $('#cyoFontMenu li').removeClass('selected');
        $(this).addClass('selected');
        var font = $(this).css('font-family');
        console.log(font);
        $('#cyoCustomFont').attr('value', font);
        $('#cyoOverlay').css('font-family', font);
    });


    //// Custom pattern menu
    //$.each($('#cyoPatternMenu div'), function (index, element) {
    //    var url = $(element).attr('data-image-url');
    //    $(element).css('background-image', 'url("' + url + '")');
    //    $(element).click(function () {
    //        $('#cyoPatternMenu div').removeClass('selected');
    //        $(this).addClass('selected');
    //        $('#cyoCustomPattern').attr('value', url);
    //        $('#cyoSample').css('background-image', 'url("' + url.replace(/_thumb/, '') + '")');
    //    });
    //});

    //// Custom font menu
    //$.each($('#cyoFontMenu div'), function (index, element) {
    //    var font = $(element).text();
    //    $(element).css('font-family', font);
    //    $(element).css('font-size', '40px');
    //    $(element).click(function () {
    //        $('#cyoFontMenu div').removeClass('selected');
    //        $(this).addClass('selected');
    //        $('#cyoCustomFont').attr('value', font);
    //        $('#cyoOverlay').css('font-family', font);
    //    });
    //});

    //// Font size
    //$('#cyoCustomFontSize').change(function () {
    //    $('#cyoOverlay').css('font-size', $(this).val());
    //});

    //// Custom text
    //$('#cyoCustomText').keyup(function () {
    //    var text = $(this).val().replace(/\n/, "<br/>");
    //    $('#cyoOverlay').html(text);
    //});

    //// Font color
    //$('#cyoCustomColor').change(function () {
    //    var hexColor = '#' + document.getElementById('cyoCustomColor').color.toString();
    //    $('#cyoOverlay').css('color', hexColor);
    //});

});
