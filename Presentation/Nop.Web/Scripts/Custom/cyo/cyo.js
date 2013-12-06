$(function () {

    var DELETE = 46;

    // Call all of the UI initializers & wire up all behaviors
    initUI();


    // --------------------------------------------------------------------
    // BEGIN UTILITY FUNCTIONS
    // --------------------------------------------------------------------

    // Remove the stock image & related settings
    function clearStockImage() {
        if ($('#cyoGraphicIsBackground').val() == 'false') {
            $('#cyoOverlayStockImage .cyoImgContainer').html('');
            $('#cyoOverlayStockImage').hide();
        }
        else {
            $('#cyoSample').css('background-image', 'none');
        }
        clearSettings('#cyoAddGraphicContainer');
        $('#cyoGraphic').val('');
        $('#cyoGraphicIsBackground').val('false');
        return false;
    }

    // Set the binky's background image. This fills the entire shield.
    function setBinkyBackground(imageUrl) {
        $('#cyoSample').css('background-image', 'url("' + imageUrl + '")');
        $('#cyoImage').val(imageUrl);
        $('#cyoSample').css('background-color', 'transparent');
        $('#cyoBackgroundColor').val('');
    }

    function setText1Color() {
        var hexColor = $('#cyoText1Color').spectrum("get").toHexString();
        $('#cyoOverlayText1').css('color', hexColor);
        $('#cyoFontColor').val(hexColor);
    }

    // Show the selected image/text next to the buttons on the
    // right side of the screen.
    function showSettings(divId, setting) {
        var settingDiv = $(divId).find('.cyoDisplaySetting');
        settingDiv.html(setting);
        settingDiv.show();
        $(divId).find('.ui-icon-closethick').removeClass('hidden').addClass('inline-block');
        return false;
    }

    // Clear the settings displayed next to the buttons on the right
    // side of the screen, and hide the little clickable X.
    function clearSettings(divId) {
        var settingDiv = $(divId).find('.cyoDisplaySetting');
        settingDiv.html('');
        settingDiv.hide();
        $(divId).find('.ui-icon-closethick').removeClass('inline-block').addClass('hidden');
        return false;
    }

    // Set the font size in the text overlay based on the position
    // of the font-size slider.
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

    // Display the uploaded image on the binky
    function showUploadInOverlay(imgUrl) {
        var img = '<img src=\"' + imgUrl + '\">';
        var imgContainer = $('#cyoOverlayUploadedImage .cyoImgContainer');
        imgContainer.html(img);
        sizeImageToDiv('#cyoOverlayUploadedImage');
        $('#cyoOverlayUploadedImage').show();
        removeOldUploadsFromDisplay();

        // Show the name of the uploaded image next to the button.
        showSettings('#cyoUploadImageContainer', $('.qq-upload-file').text());
    }

    // Remove older photos from the upload dialog, 
    // since we don't have a way of getting back to them.
    function removeOldUploadsFromDisplay() {        
        var imageCount = $('.qq-upload-success').length;
        if (imageCount > 1) {
            $('.qq-upload-success').each(function (index, element) {
                if (index == 0) {
                    $(element).remove();
                }
            });
        }
    }

    // Set the binky background color to match what's selected
    // in the background dialog.
    function setBinkyBackgroundColor() {
        $('#cyoSample').css('background-image', 'none');
        $('#cyoImage').val('');
        var hexColor = $('#cyoBackgroundColorControl').spectrum("get").toHexString();
        showSettings('#cyoSelectBackgroundContainer', hexColor);
        $('#cyoSample').css('background-image', 'none');
        $('#cyoSample').css('background-color', hexColor);
        $('#cyoBackgroundColor').val(hexColor);
        return false;
    }

    // --------------------------------------------------------------------
    // BEGIN INITIALIZERS
    // --------------------------------------------------------------------

    // Initialize the file upload controller.
    // The #cyoUploadControlSettings div contains some settings 
    // in data-* attributes that were set by the server.
    function initUploader() {        
        var settings = $('#cyoUploadControlSettings');
        var previewUrl = settings.attr('data-preview-url');
        var cyoUploader = new qq.FileUploader({
            element: document.getElementById('cyoUploader'),
            action: settings.attr('data-form-action'),
            onComplete: function (id, fileName, responseJSON) {
                $("#cyoUploadFile").val(responseJSON.downloadGuid);
                if (responseJSON.downloadGuid) {
                    // Show a thumbnail of the uploaded image in the upload dialog.
                    var imgUrl = previewUrl.replace('00000000-0000-0000-0000-000000000000', responseJSON.downloadGuid);
                    $('#cyoUploadedImageThumbnail').attr('src', imgUrl);
                    $('#cyoUploadedImageDiv').show();
                    showUploadInOverlay(imgUrl);
                }
                else if (responseJSON.message) {
                    alert(responseJSON.message);
                }
            },
            allowedExtensions: ['jpg', 'jpeg', 'png', 'gif'],
            strings: {
                upload: settings.attr('data-upload-string'),
                drop: settings.attr('data-drop-string'),
                cancel: settings.attr('data-cancel-string'),
                failed: settings.attr('data-failed-string')
            }
        });

        // Clear uploaded image when user clicks X
        $('#cyoUploadImageContainer .ui-icon-closethick').click(function () {
            var imgContainer = $('#cyoOverlayUploadedImage .cyoImgContainer');
            imgContainer.html('');
            $('#cyoOverlayUploadedImage').hide();
            clearSettings('#cyoUploadImageContainer');
            return false;
        });

        // If user clicks on the uploaded image thumbnail, 
        // load the image back into the overlay. This is 
        // useful when user deletes uploaded image then
        // wants it back
        $('#cyoUploadedImageThumbnail').click(function () {
            var imgUrl = $('#cyoUploadedImageThumbnail').attr('src');
            showUploadInOverlay(imgUrl);
        });
    }


    // Initalize the modal dialogs and hook up the buttons that launch them.
    function initModals() {
        var modalProperties = {
            height: 400,
            width: 500,
            modal: false,
            autoOpen: false,
            position: { my: "left", at: "center", of: window },
            show: { effect: "blind", duration: 600 }
        };

        $("#cyoModalUpload").dialog(modalProperties);
        $("#cyoModalBackground").dialog(modalProperties);
        $("#cyoModalText").dialog(modalProperties);
        $("#cyoModalGraphic").dialog(modalProperties);

        $('#btnShowModalUpload').click(function () {
            $("#cyoModalUpload").dialog("open");
        });
        $('#btnHideModalUpload').click(function () {
            $("#cyoModalUpload").dialog("close");
        });


        $('#btnShowModalBackground').click(function () {
            $("#cyoModalBackground").dialog("open");
        });
        $('#btnHideModalBackground').click(function () {
            $("#cyoModalBackground").dialog("close");
        });

        $('#btnShowModalText').click(function () {
            $("#cyoModalText").dialog("open");
        });
        $('#btnHideModalText').click(function () {
            $("#cyoModalText").dialog("close");
        });

        $('#btnShowModalGraphic').click(function () {
            $("#cyoModalGraphic").dialog("open");
        });
        $('#btnHideModalGraphic').click(function () {
            $("#cyoModalGraphic").dialog("close");
        });
    }

    function initDraggablesAndSliders() {
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
    }

    function initBackgroundImageBehaviors() {
        // Load clicked background image into pacifier
        $('#cyoModalBackground .chooser img').click(function () {
            var url = $(this).attr('src');
            setBinkyBackground(url.replace(/_thumb/, ''));
            showSettings('#cyoSelectBackgroundContainer', $(this).attr('title'));
            return false;
        });

        // Clear the background image from the pacifier when user clicks X
        $('#cyoSelectBackgroundContainer .ui-icon-closethick').click(function () {
            $('#cyoSample').css('background-image', 'none');
            $('#cyoImage').val('');
            clearSettings('#cyoSelectBackgroundContainer');
            return false;
        });

        // Set up the colorpicker.
        // The move event here is not mousemove. It's fired
        // when you move the black dot on the color picker
        // by clicking in a new location or by dragging the dot.
        $('#cyoBackgroundColorControl').spectrum({
            allowEmpty: true,
            color: "#fff",
            move: setBinkyBackgroundColor,
            showPalette: true
        });

        $('#cyoBackgroundColorControl').change(function () {
            setBinkyBackgroundColor();
        });

        // When user clicks color wheel, focus color input so pop-up colorpicker appears.
        $('#cyoBgColorWheel').click(function () {
            $('#cyoBackgroundColorControl').trigger('focus');
            return false;
        });

    }

    function initStockImageBehaviors() {
        // Load clicked stock image into div on top of pacifier
        $('#cyoModalGraphic .chooser img').click(function () {
            var url = $(this).attr('src');
            // Full background image...
            if ($(this).attr('data-fill-background') == 'true') {
                setBinkyBackground(url.replace(/_thumb/, ''));
                $('#cyoOverlayStockImage .cyoImgContainer').empty();
                $('#cyoGraphicIsBackground').val('true');
            }
            else {  // Smaller image goes on overlay
                var img = '<img src=\"' + url + '\">';
                $('#cyoOverlayStockImage .cyoImgContainer').html(img);
                sizeImageToDiv("#cyoOverlayStockImage");
                $('#cyoOverlayStockImage').show();
                if ($('#cyoGraphicIsBackground').val() == 'true')
                    $('#cyoSample').css('background-image', 'none');
                $('#cyoGraphicIsBackground').val('false');
            }
            $('#cyoGraphic').val(url);
            showSettings('#cyoAddGraphicContainer', $(this).attr('title'));
            return false;
        });

        // Clear stock image when user clicks X
        $('#cyoAddGraphicContainer .ui-icon-closethick').click(function () {
            clearStockImage();
        });
    }


    function initTextDialogBehaviors() {
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
        $('#cyoText1Color').spectrum({
            allowEmpty: true,
            color: "#fff",
            move: setText1Color,
            showPalette: true
        });

        // Allow slider to set font-size in the overlay
        $('#font-size-slider a').mouseup(function () {
            setFontSize($(this));
        });

        // Set custom text as user types
        $('#cyoCustomText').keyup(function () {
            var text = $(this).val().replace(/\n/, "<br/>");
            $('#cyoOverlayText1').html(text);
            $('#cyoText').val(text);
            showSettings('#cyoAddTextContainer', text);
        });

        // Initialize font size slider position and font size
        $('#font-size-slider a').css('left', '80px');
        setFontSize($('#font-size-slider a'));

        // Clear text when user clicks X
        $('#cyoAddTextContainer .ui-icon-closethick').click(function () {
            $('#cyoOverlayText1').html('');
            $('#cyoCustomText').val('');
            $('#cyoText').val('');
            clearSettings('#cyoAddTextContainer');
            return false;
        });
    }



    function initSizeRadios() {
        // Copy product size to hidden form when user changes it
        $('input[name=size]').change(function () {
            $('#cyoProductSize').val($(this).val());
        });
    }


    function initBinkySelector() {
        // When user clicks white/pink/blue binky, load that image into the customizer on the left
        $('.shields img').click(function () {
            var bgImage = $('#cyoSample').css('background-image');
            $('#cyoSample img').attr('src', $(this).attr('data-large-image'));
            $('#cyoSample').css('background-image', bgImage);
            // Copy product color to hidden form
            $('#cyoProductColor').val($(this).attr('data-shield'));
        });
    }


    function initCreateProof() {
        // Create Proof button
        $('#btn-create-proof').click(function () {
            alert('Proof is not hooked up yet')
        });
    }

    function initOverlays() {
        $('#cyoOverlayStockImage').click(function () {
            $(this).addClass('selected-overlay');
            return false;
        });
    }

    function initDocumentBehaviors() {
        $('body').click(function () {
            $('.selected-overlay').removeClass('selected-overlay');
            return false;
        });
        $(document).keyup(function (event) {
            var selectedOverlay = $('.selected-overlay')[0];
            if (selectedOverlay != null && event.which == DELETE) {
                if ($(selectedOverlay).attr('id') == 'cyoOverlayStockImage') {
                    clearStockImage();
                    $('#cyoOverlayStockImage').removeClass('selected-overlay');
                }
            }
        });
    }

    function initUI() {
        initUploader();
        initModals();
        initDraggablesAndSliders();
        initBackgroundImageBehaviors();
        initStockImageBehaviors();
        initTextDialogBehaviors();
        initSizeRadios();
        initBinkySelector();
        initCreateProof();
        initOverlays();
        initDocumentBehaviors();
    }
});
