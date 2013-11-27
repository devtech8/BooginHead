$(function () {

    $("#cyoOverlay").draggable();
    $("#font-size-slider").slider();

    var modalProperties = {
        height: 300,
        width: 500,
        modal: true,
        autoOpen: false,
        show: { effect: "blind", duration: 600 }
    };

    $("#cyoModalUpload").dialog(modalProperties);
    $("#cyoModalBackground").dialog(modalProperties);
    $("#cyoModalText").dialog(modalProperties);
    $("#cyoModalGraphic").dialog(modalProperties);

    $('#btnShowModalUpload').click(function () {
        $("#cyoModalUpload").dialog("open");
    });

    $('#btnShowModalBackground').click(function () {
        $("#cyoModalBackground").dialog("open");
    });

    $('#btnShowModalText').click(function () {
        $("#cyoModalText").dialog("open");
    });

    $('#btnShowModalGraphic').click(function () {
        $("#cyoModalGraphic").dialog("open");
    });


    // Initialize font size slider position and font size
    $('#font-size-slider a').css('left', '60px');
    setFontSize($('#font-size-slider a'));

    // Copy product size to hidden form when user changes it
    $('input[name=size]').change(function () {
        $('#cyoProductSize').val($(this).val());
    });

    // When user click while/pink/blue binky, load that image into the customizer on the left
    $('.shields img').click(function () {
        var bgImage = $('#cyoSample').css('background-image');
        $('#cyoSample img').attr('src', $(this).attr('data-large-image'));
        $('#cyoSample').css('background-image', bgImage);
        // Copy product color to hidden form
        $('#cyoProductColor').val($(this).attr('data-shield'));
    });

    // Load clicked image into pacifier
    $('.chooser img').click(function () {
        var url = $(this).attr('src');
        $('#cyoSample').css('background-image', 'url("' + url.replace(/_thumb/, '') + '")');
        $('#cyoImage').val(url);
        return false;
    });
    
    // Clear image under pacifier
    $('.btn-clear-design').click(function () {
        $('#cyoSample').css('background-image', 'none');
        $('#cyoImage').val('');
    });

    // Set custom text as user types
    $('#cyoCustomText').keyup(function () {
        var text = $(this).val().replace(/\n/, "<br/>");
        $('#cyoOverlay').html(text);
        $('#cyoText').val(text);
    });

    // Clear custom text when user clicks Clear button
    $('#cyoClearText').click(function () {
        $('#cyoOverlay').html('');
        $('#cyoCustomText').val('');
        $('#cyoText').val('');
    });

    // Custom font menu
    $('#cyoFontMenu li').click(function () {
        $('#cyoFontMenu li').removeClass('selected');
        $(this).addClass('selected');
        var font = $(this).css('font-family');
        $('#cyoCustomFont').attr('value', font);
        $('#cyoOverlay').css('font-family', font);
        $('#cyoFontFamily').val(font.replace(/'/, ''));
    });

    // Allow font color input to set font color in overlay
    $('#cyoCustomColor').change(function () {
        var hexColor = '#' + document.getElementById('cyoCustomColor').color.toString();
        $('#cyoOverlay').css('color', hexColor);
        $('#cyoFontColor').val(hexColor);
    });

    // When user clicks color wheel, focus color input so pop-up colorpicker appears.
    $('#colorWheel').click(function () {
        $('#cyoCustomColor').trigger('focus');
        return false;
    });

    // Allow slider to set font-size in the overlay
    $('#font-size-slider a').mouseup(function () {
        setFontSize($(this));
    });

    function setFontSize(sliderControl) {
        var position = parseInt(sliderControl.css('left'), 10);
        var fontSize = parseInt((position / 1.5), 10);
        $('#cyoOverlay').css('font-size', fontSize);
        $('#cyoFontSize').val(fontSize + 'px');
    }

    // Create Proof
    $('#btn-create-proof').click(function () {
        alert('Proof is not hooked up yet')
    });

});
