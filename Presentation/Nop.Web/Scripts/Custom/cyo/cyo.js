$(function () {

    var DELETE = 46;
    var DEFAULT_ZOOM_SLIDER_POSITION = "50%";
    var activeTextContainer = 1;

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

    // Remove the uploaded image from the overlay and clear
    // the settings display to the right of the button. The
    // uploaded image is still available in the upload dialog.
    function clearUploadedImage() {
        var imgContainer = $('#cyoOverlayUploadedImage .cyoImgContainer');
        imgContainer.html('');
        $('#cyoOverlayUploadedImage').hide();
        clearSettings('#cyoUploadImageContainer');
        return false;
    }

    // Clear the text overlay and the settings that appear
    // to the right of the Text button.
    function clearText() {
        $('#cyoTextContent' + activeTextContainer).html('');
        $('#cyoCustomText').val('');
        $('#cyoText' + activeTextContainer).val('');
        clearSettings('#cyoAddTextContainer' + activeTextContainer);
    }

    // Load custom text settings from the hidden vars back into the UI controls.
    // User may be switching back and forth between text1 and text2, so we need
    // to load the correct values for whatever they're working with.
    function loadTextSettings() {
        var text = $('#cyoText' + activeTextContainer).val();
        $('#cyoCustomText').val(text);

        var color = $('#cyoFontColor' + activeTextContainer).val();
        $("#cyoTextColor").spectrum("set", color);

        var font = $('#cyoFontFamily' + activeTextContainer).val();
        var quotedFont = "'" + font + "'";
        $('#cyoFontMenu li').each(function (index, element) {
            var liFont = $(element).css('font-family');
            if (liFont == quotedFont || liFont == font) {
                $(element).trigger("click");
            }
        });

        setSliderFromFontSize();
    }

    // Set the binky's background image. This fills the entire shield.
    function setBinkyBackground(imageUrl) {
        $('#cyoSample').css('background-image', 'url("' + imageUrl + '")');
        $('#cyoImage').val(imageUrl);
        $('#cyoSample').css('background-color', 'transparent');
        $('#cyoBackgroundColor').val('');

        // Reset zoom when setting new image
        $('#cyoSample').css('background-size', '100%')
        $('#uploadSizeSlider a').css('left', DEFAULT_ZOOM_SLIDER_POSITION);

    }

    function setTextColor() {
        var hexColor = $('#cyoTextColor').spectrum("get").toHexString();
        $('#cyoTextContent' + activeTextContainer).css('color', hexColor);
        $('#cyoFontColor' + activeTextContainer).val(hexColor);
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
        $('#cyoTextContent' + activeTextContainer).css('font-size', fontSize);
        $('#cyoFontSize' + activeTextContainer).val(fontSize + 'px');
    }

    // Set the zoom on the uploaded background image when the user
    // changes the zoom slider. The slider is in the upload dialog.
    function setUploadImageZoom(sliderControl) {
        var binkyBackground = $('#cyoSample').css('background-image');
        var uploadedImage = $('#cyoUploadedImage').val();
        if (binkyBackground.indexOf(uploadedImage) > -1) {
            var zoom = parseInt(sliderControl.css('left'), 10) / 2;        
            $('#cyoSample').css('background-size', zoom + '%')
            $('#cyoBgImageZoom').val(zoom);
        }
    }


    // Set the position of the font-size slider based on font size in the text overlay.
    function setSliderFromFontSize() {
        var fontSize = parseInt($('#cyoFontSize' + activeTextContainer).val(), 10);
        $('#font-size-slider a').css('left', fontSize * 1.5);
    }

    // Resize the inner text container to match the dimensions of its container div
    function sizeTextContainerToDiv(divId) {
        var textContainer = $(divId).find('div');
        textContainer.attr('height', $(divId).height());
        textContainer.attr('width', $(divId).width());
    }

    // Resize the image to match the dimensions of its container div
    function sizeImageToDiv(divId) {
        var imgElement = $(divId + ' .cyoImgContainer img');
        imgElement.attr('height', $(divId).height());
        imgElement.attr('width', $(divId).width());
    }

    // Display the uploaded image on the binky
    function showUploadAsBackground(imgUrl) {
        setBinkyBackground(imgUrl);
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
                    showUploadAsBackground(imgUrl);
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

        // Remove this div. If the user drags anywhere in the upload dialog,
        // that will cause the uploader to display a div that says 
        // "Drop files here to upload". By the time the user is dragging, 
        // he/she has already uploaded an image and is trying to work with
        // the slider. The appearance of this new div is disconcerting in 
        // that context.
        $('.qq-upload-drop-area').remove();

        // Clear uploaded image when user clicks X
        $('#cyoUploadImageContainer .ui-icon-closethick').click(function () {
            clearUploadedImage();
        });

        // If user clicks on the uploaded image thumbnail, 
        // load the image back into the overlay. This is 
        // useful when user deletes uploaded image then
        // wants it back
        $('#cyoUploadedImageThumbnail').click(function () {
            var imgUrl = $('#cyoUploadedImageThumbnail').attr('src');
            showUploadAsBackground(imgUrl);
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

        $('#btnShowModalText1').click(function () {
            activeTextContainer = 1;
            loadTextSettings();
            $("#cyoOverlayText1").show();
            $("#cyoModalText").dialog("open");
        });
        $('#btnShowModalText2').click(function () {
            activeTextContainer = 2;
            loadTextSettings();
            $("#cyoOverlayText2").show();
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
            },
            handles: "n, e, s, w"
        });
        $("#cyoOverlayUploadedImage").draggable().resizable({
            resize: function (e, ui) {
                sizeImageToDiv("#cyoOverlayUploadedImage");
            },
            handles: "n, e, s, w"
        });
        $("#cyoOverlayText1").draggable().resizable({
            resize: function (e, ui) {
                sizeTextContainerToDiv("#cyoOverlayText1");
            },
            handles: "n, e, s, w"
        });
        $("#cyoOverlayText2").draggable().resizable({
            resize: function (e, ui) {
                sizeTextContainerToDiv("#cyoOverlayText2");
            },
            handles: "n, e, s, w"
        });

        $("#font-size-slider").slider();
        $("#uploadSizeSlider").slider();
        $("#uploadSizeSlider a").css("left", DEFAULT_ZOOM_SLIDER_POSITION);
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
            $('#cyoTextContent' + activeTextContainer).css('font-family', font);
            $('#cyoFontFamily' + activeTextContainer).val(font.replace(/'/g, ''));
        });

        // Allow font color input to set font color in overlay
        $('#cyoTextColor').spectrum({
            allowEmpty: true,
            color: "#000",
            move: setTextColor,
            showPalette: true
        });

        // Allow slider to set font-size in the overlay
        $('#font-size-slider a').mouseup(function () {
            setFontSize($(this));
        });

        $('#uploadSizeSlider a').mouseup(function () {
            setUploadImageZoom($(this));
        });

        // Set custom text as user types
        $('#cyoCustomText').keyup(function () {
            var text = $(this).val().replace(/\n/g, "<br/>");
            $('#cyoTextContent' + activeTextContainer).html(text);
            $('#cyoText' + activeTextContainer).val(text);
            showSettings('#cyoAddTextContainer' + activeTextContainer, text);
        });

        // Initialize font size slider position and font size
        $('#font-size-slider a').css('left', '80px');
        setFontSize($('#font-size-slider a'));

        // Clear text when user clicks X
        $('#cyoAddTextContainer1 .ui-icon-closethick, #cyoAddTextContainer2 .ui-icon-closethick').click(function () {
            clearText();
            return false;
        });
    }



    function initRadios() {
        // Copy product size to hidden form when user changes it
        $('input[name=size]').change(function () {
            $('#cyoProductSize').val($(this).val());
        });

        // Select & display binky brand
        $('input[name=brand]').change(function () {
            var brand = $(this).val();
            $('#cyoBrand').val($(this).val());
            if (brand == 'booginhead') {
                $('#binkiesNuk').hide();
                $('#binkiesBooginhead').show();
            }
            else {
                $('#binkiesBooginhead').hide();
                $('#binkiesNuk').show();
            }
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
        // If the user clicks on an element, make that element
        // the "selected" element. Make sure any previously selected
        // elements are no longer selected.
        $('#cyoOverlayStockImage, #cyoOverlayUploadedImage, #cyoOverlayText1, #cyoOverlayText2').click(function () {
            $('.selected-overlay').removeClass('selected-overlay');
            var elementId = $(this).attr('id');
            if (elementId == 'cyoOverlayText1') {
                activeTextContainer = 1;
                loadTextSettings();
                $("#cyoModalText").dialog("open");
            }
            else if (elementId == 'cyoOverlayText2') {
                activeTextContainer = 2;
                loadTextSettings();
                $("#cyoModalText").dialog("open");
            }
            $(this).addClass('selected-overlay');
            return false;
        });
    }

    // Initialize document-wide click and key behaviors
    function initDocumentBehaviors() {

        // When clicking on the body, outside of one of our overlays,
        // de-select any overlay that might be selected.
        $('body').click(function () {
            $('.selected-overlay').removeClass('selected-overlay');
        });

        // Catch the keyup event for the delete key. If one of the overlays
        // is selected when user hits delete, empty that element and hide it.
        $(document).keyup(function (event) {
            var selectedOverlay = $('.selected-overlay')[0];
            if (selectedOverlay != null && event.which == DELETE) {
                if ($(selectedOverlay).attr('id') == 'cyoOverlayStockImage') {
                    clearStockImage();
                    $('#cyoOverlayStockImage').removeClass('selected-overlay');
                }
                else if ($(selectedOverlay).attr('id') == 'cyoOverlayUploadedImage') {
                    clearUploadedImage();
                    $('#cyoOverlayUploadedImage').removeClass('selected-overlay');
                }
                else if ($(selectedOverlay).attr('id') == 'cyoOverlayText1') {
                    clearText();
                    $('#cyoOverlayText1').removeClass('selected-overlay');
                    $('#cyoOverlayText1').hide();
                }
                else if ($(selectedOverlay).attr('id') == 'cyoOverlayText2') {
                    clearText();
                    $('#cyoOverlayText2').removeClass('selected-overlay');
                    $('#cyoOverlayText2').hide();
                }
            }
        });
    }

    // Adapted from https://github.com/kentor/jquery-draggable-background
    function initDraggableBackground() {
        $('#cyoSample').on('mousedown touchstart', function (e) {
            e.preventDefault()
            if (e.originalEvent.touches) {
                e.clientX = e.originalEvent.touches[0].clientX
                e.clientY = e.originalEvent.touches[0].clientY
            }
            else if (e.which !== 1) {
                return
            }
            var x0 = e.clientX
              , y0 = e.clientY
              , pos = $(this).css('background-position').match(/(-?\d+).*?\s(-?\d+)/) || []
              , xPos = parseInt(pos[1]) || 0
              , yPos = parseInt(pos[2]) || 0
            $(window).on('mousemove touchmove', function (e) {
                e.preventDefault()
                if (e.originalEvent.touches) {
                    e.clientX = e.originalEvent.touches[0].clientX
                    e.clientY = e.originalEvent.touches[0].clientY
                }
                var x = e.clientX
                  , y = e.clientY
                xPos = xPos + x - x0
                yPos = yPos + y - y0
                x0 = x
                y0 = y
                $('#cyoSample').css('background-position', xPos + 'px ' + yPos + 'px')
            })
        })
        $(window).on('mouseup touchend', function () { $(window).off('mousemove touchmove') })
    }

    function initUI() {
        initUploader();
        initModals();
        initDraggablesAndSliders();
        initBackgroundImageBehaviors();
        initStockImageBehaviors();
        initTextDialogBehaviors();
        initRadios();
        initBinkySelector();
        initCreateProof();
        initOverlays();
        initDraggableBackground();
        initDocumentBehaviors();
    }
});
