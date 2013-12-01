$(function () {

    $("#cyoOverlayStockImage").draggable().resizable({
        resize: function (e, ui) {
            sizeImageToDiv("#cyoOverlayStockImage");
        }
    });
    $("#cyoOverlayUploadedImage").draggable().resizable({
        resize: function (e, ui) {
            sizeImageToDiv("#cyoOverlayUploadedImage");
        }
     });
    $("#cyoOverlayText1").draggable();
    $("#cyoOverlayText2").draggable();

    $("#font-size-slider").slider();

    var modalProperties = {
        height: 400,
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

    // Clear the background image when user clicks X
    $('#cyoSelectBackgroundContainer .ui-icon-closethick').click(function () {
        $('#cyoSample').css('background-image', 'none');
        $('#cyoImage').val('');
        clearSettings('#cyoSelectBackgroundContainer');
        return false;
    });

    // Clear uploaded image when user clicks X
    $('#cyoUploadImageContainer .ui-icon-closethick').click(function () {
        var imgContainer = $('#cyoOverlayUploadedImage .cyoImgContainer');
        imgContainer.html('');
        $('#cyoOverlayUploadedImage').hide();
        clearSettings('#cyoUploadImageContainer');
        return false;
    });

    // Clear stock image when user clicks X
    $('#cyoAddGraphicContainer .ui-icon-closethick').click(function () {
        $('#cyoOverlayStockImage .cyoImgContainer').html('');
        $('#cyoOverlayStockImage').hide();
        clearSettings('#cyoAddGraphicContainer');
        return false;
    });

    $('#cyoAddTextContainer .ui-icon-closethick').click(function () {

    });


    // Initialize font size slider position and font size
    $('#font-size-slider a').css('left', '60px');
    setFontSize($('#font-size-slider a'));

    // Copy product size to hidden form when user changes it
    $('input[name=size]').change(function () {
        $('#cyoProductSize').val($(this).val());
    });

    // When user clicks white/pink/blue binky, load that image into the customizer on the left
    $('.shields img').click(function () {
        var bgImage = $('#cyoSample').css('background-image');
        $('#cyoSample img').attr('src', $(this).attr('data-large-image'));
        $('#cyoSample').css('background-image', bgImage);
        // Copy product color to hidden form
        $('#cyoProductColor').val($(this).attr('data-shield'));
    });

    // Load clicked background image into pacifier
    $('#cyoModalBackground .chooser img').click(function () {
        var url = $(this).attr('src');
        $('#cyoSample').css('background-image', 'url("' + url.replace(/_thumb/, '') + '")');
        $('#cyoImage').val(url);
        showSettings('#cyoSelectBackgroundContainer', $(this).attr('title'));
        return false;
    });


    // Load clicked stock image into div
    $('#cyoModalGraphic .chooser img').click(function () {
        var url = $(this).attr('src');
        var img = '<img src=\"' + url + '\">';
        $('#cyoOverlayStockImage .cyoImgContainer').html(img);        
        sizeImageToDiv("#cyoOverlayStockImage");
        $('#cyoOverlayStockImage').show();
        showSettings('#cyoAddGraphicContainer', $(this).attr('title'));
        return false;
    });

    function showSettings(divId, setting) {
        var settingDiv = $(divId).find('.cyoDisplaySetting');
        settingDiv.html(setting);
        settingDiv.show();
        $(divId).find('.ui-icon-closethick').removeClass('hidden').addClass('inline-block');
        return false;
    }

    function clearSettings(divId) {
        var settingDiv = $(divId).find('.cyoDisplaySetting');
        settingDiv.html('');
        settingDiv.hide();
        $(divId).find('.ui-icon-closethick').removeClass('inline-block').addClass('hidden');
        return false;
    }

    
    // Clear image under pacifier
    $('.btn-clear-design').click(function () {
        $('#cyoSample').css('background-image', 'none');
        $('#cyoImage').val('');
    });

    // Set custom text as user types
    $('#cyoCustomText').keyup(function () {
        var text = $(this).val().replace(/\n/, "<br/>");
        $('#cyoOverlayText1').html(text);
        $('#cyoText').val(text);
    });

    // Clear custom text when user clicks Clear button
    $('#cyoClearText').click(function () {
        $('#cyoOverlayText1').html('');
        $('#cyoCustomText').val('');
        $('#cyoText').val('');
    });

    // Custom font menu
    $('#cyoFontMenu li').click(function () {
        $('#cyoFontMenu li').removeClass('selected');
        $(this).addClass('selected');
        var font = $(this).css('font-family');
        $('#cyoCustomFont').attr('value', font);
        $('#cyoOverlayText1').css('font-family', font);
        $('#cyoFontFamily').val(font.replace(/'/, ''));
    });

    // Allow font color input to set font color in overlay
    $('#cyoCustomColor').change(function () {
        var hexColor = '#' + document.getElementById('cyoCustomColor').color.toString();
        $('#cyoOverlayText1').css('color', hexColor);
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
        $('#cyoOverlayText1').css('font-size', fontSize);
        $('#cyoFontSize').val(fontSize + 'px');
    }

    // Resize the image to match the dimensions of its container div
    function sizeImageToDiv(divId) {
        var imgElement = $(divId + ' .cyoImgContainer img');
        imgElement.attr('height', $(divId).height());
        imgElement.attr('width', $(divId).width());
    }

    // Create Proof
    $('#btn-create-proof').click(function () {
        alert('Proof is not hooked up yet')
    });

});
