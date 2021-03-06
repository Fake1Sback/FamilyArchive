﻿$(function () {
    var path = "";
    var current_loads = 0;
    var max_posible_loads = 0;
    var loading_now = false;
    var active_request = null;
    var selected_image = "";
    var selectedPhotoName = "";
    var selected_description = "";
    var selected_header = "";

    $('#options_container').hide();
    $('#create_folder_container').hide();
    $('#Gradient_container').hide();
    $('#load_more_button_container').hide();

    GalleryResize();
    HidePopUp();
    GetMaxAmountOfLoads();

    $('#menu_button').click(function () {
        $('#options_container').slideToggle(1000);
    });

    $('#Edit_button').on('click', function () {
        EditPhoto();
    });

    $('#Delete_button').on('click', function () {
        DeletePhoto();
    });

    $('#Download_button').on('click', function () {
        DownloadPhoto();
    });

    $('#create_folder_submit').on('click', function () {
        CreateFolder(path, $('#new_folder_name_field').val());
    });

    $('#Gallery_Container').on('click', 'img', function () {
        selected_image = $(this).attr('src');
        selected_header = $(this).siblings('h2').text();
        selected_description = $(this).siblings('p').text();
        var selectedFileType = $(this).siblings('input').first().val();
        selectedPhotoName = $(this).siblings('input').last().val();

        $('#DetailedPhoto').attr('src', selected_image);
        $('#EditInput').val(selected_header);
        $('#EditArea').val(selected_description);


        $('#photo_container').empty();
        if (selectedFileType === "photo") {
            $('#photo_container').append('<img id="DetailedPhoto" src="" />');
            $('#DetailedPhoto').attr('src', selected_image);
        }
        else if (selectedFileType === "video") {
            $('#photo_container').append('<video id="DetailedVideo" src="" controls preload="none"/>');
            var url = '/api/values/GetVideo?path=' + path + '\\' + selectedPhotoName;
            $('#DetailedVideo').attr('poster', selected_image);
            $('#DetailedVideo').attr('src', url);
        }

        $('#Gradient_container').show();
        $(window).scrollTop($('#photo_container').offset().top);
    });

    $('#file_input').on('change', function (e) {
        $('#upload_photos_form').submit();
    });

    $('#upload_photos_form').on('submit', function (e) {
        e.preventDefault();
        UploadFiles();
    });

    $('.options_item').on('click', '.fs_item_container img', function () {
        if ($(this).attr('src') === '/images/folder.svg' || $(this).attr('src') === '/images/empty_folder.svg') {

            path = path + '\\' + $(this).siblings('div').text();
            $('#fs_cur_path_container p').text('Поточний шлях:' + path);

            $('#photo_container').empty();
            $('#Gradient_container').hide();

            $('.Gallery-SubContainer').each(function (_, el) {
                $(this).empty();
            });

            $('.options_item').empty();

            $(window).scrollTop($('#Gallery_Container').offset().top);

            if (active_request !== null) {
                active_request.abort();
                active_request = null;
            }

            current_loads = 0;
            GetMaxAmountOfLoads();
        }
        else {
            var full_fs_item_name = $(this).siblings('div').text();
            ShowPopUp(full_fs_item_name, false, true);
        }
    });

    function GalleryResize() {
        var width = $(this).width();
        var subcontainers = $('.Gallery-SubContainer');

        if (width >= 768) {
            if (subcontainers.length !== 3) {
                if (subcontainers.length === 0) {
                    $('#Gallery_Container').append("<div class='Gallery-SubContainer'></div>");
                    $('#Gallery_Container').append("<div class='Gallery-SubContainer'></div>");
                    $('#Gallery_Container').append("<div class='Gallery-SubContainer'></div>");
                }
                else if (subcontainers.length === 2) {
                    $('#Gallery_Container').append("<div class='Gallery-SubContainer'></div>");
                    FixDivs(3);
                }
            }
        }
        else if (width >= 480 && width < 768) {
            if (subcontainers.length !== 2) {
                if (subcontainers.length === 0) {
                    $('#Gallery_Container').append("<div class='Gallery-SubContainer'></div>");
                    $('#Gallery_Container').append("<div class='Gallery-SubContainer'></div>");
                }
                else if (subcontainers.length === 1) {
                    FixDivs(2, false);
                }
                else if (subcontainers.length === 3) {
                    FixDivs(2, true);
                }
            }
        }
        else if (width < 480) {
            if (subcontainers.length !== 1) {
                if (subcontainers.length === 0) {
                    $('#Gallery_Container').append("<div class='Gallery-SubContainer'></div>");
                }
                else if (subcontainers.length === 2) {
                    FixDivs(1);
                }
            }
        }
    }

    function FixDivs(desiredAmout, removing) {
       
        if (desiredAmout === 3) {
            var newSubcontainers = $('.Gallery-SubContainer');
            var firstDiv = newSubcontainers.eq(0);
            var secondDiv = newSubcontainers.eq(1);
            var thirdDiv = newSubcontainers.eq(2);

            while (thirdDiv.height() < firstDiv.height() && thirdDiv.height() < secondDiv.height()) {
                if (firstDiv.height() > secondDiv.height()) {
                    var detachedElement = firstDiv.children().first().detach();
                    thirdDiv.append(detachedElement);
                }
                else if (firstDiv.height() <= secondDiv.height()) {
                    var detachedElement = secondDiv.children().first().detach();
                    thirdDiv.append(detachedElement);
                }
            }
        }
        else if (desiredAmout === 2 && removing === false) {
            $('#Gallery_Container').append("<div class='Gallery-SubContainer'></div>");
            var newSubcontainers = $('.Gallery-SubContainer');
            var firstDiv = newSubcontainers.eq(0);
            var secondDiv = newSubcontainers.eq(1);

            while (parseInt(firstDiv.height()) > parseInt(secondDiv.height())) {
                var detachedElement = firstDiv.children().first().detach();
                secondDiv.append(detachedElement);
            }
        }
        else if (desiredAmout === 2 && removing === true) {
            var firstDiv = $('.Gallery-SubContainer').eq(0);
            var secondDiv = $('.Gallery-SubContainer').eq(1);
            var thirdDiv = $('.Gallery-SubContainer').eq(2);

            while (thirdDiv.height() !== 0) {
                var detachedElement = thirdDiv.children().last().detach();
                if (firstDiv.height() < secondDiv.height()) {
                    firstDiv.append(detachedElement);
                }
                else {
                    secondDiv.append(detachedElement);
                }
            }

            thirdDiv.remove();
        }
        else if (desiredAmout === 1) {
            var newSubcontainers = $('.Gallery-SubContainer');
            var firstDiv = newSubcontainers.eq(0);
            var secondDiv = newSubcontainers.eq(1);

            while (secondDiv.height() !== 0) {             
                var detachedElement = secondDiv.children().first().detach();
                firstDiv.append(detachedElement);
            }

            secondDiv.remove();
        }
    }

    $(window).resize(GalleryResize); 

    $(window).scroll(function () {
        if ($(window).scrollTop() + $(window).height() >= $(document).height() - 80) {
            if (current_loads < max_posible_loads) {
                if (loading_now === false) {
                    GetFolderContent();
                }
            }         
        }
    });

    function GetMaxAmountOfLoads() {
        if (loading_now === false) {
            loading_now = true;
            $.ajax({
                url: 'api/values/GetMaxLoads?path=' + path,
                type: 'GET',
                dataType: 'text',
                success: function (max_amount_of_loads) {
                    max_posible_loads = max_amount_of_loads;
                },
                error: function (xhr) {
                    ShowError(xhr);
                },
                complete: function (xhr) {
                    loading_now = false;
                    if (xhr.status === 200) {
                        GetFolderContent();
                    }                 
                }
            });
        }     
    }

    function GetFolderContent() {
        if (loading_now === false) {
            loading_now = true;
            $.ajax({
                url: '/api/values/GetFolderContent?path=' + path + '&skip=' + current_loads,
                type: 'GET',
                contentType: "application/json",
                dataType: 'json',
                success: function (content) {
                    Fill_fs_container(content);
                },
                error: function (xhr) {
                    ShowError(xhr);
                },
                complete: function (xhr) {
                    loading_now = false;
                    if (xhr.status === 200) {
                        GetPhotos();
                    }                  
                }
            });
        }   
    }

    function GetPhotos() {
        if (loading_now === false) {
            loading_now = true;
            ShowPopUp('Завантаження', true, false);
            active_request = $.ajax({
                url: '/api/values/GetPhotos?path=' + path + '&skip=' + current_loads,
                type: 'GET',
                contentType: "application/json",
                dataType: 'json',
                success: function (photos) {
                    PlacePhotos(photos);
                    HidePopUp();
                },
                error: function (xhr) {
                    ShowError(xhr);
                },
                complete: function (xhr) {
                    active_request = null;
                    loading_now = false;
                    if (xhr.status === 200) {
                        current_loads += 1;
                    }              
                }
            });
        }   
    }

    var photosArr = [];
    var MyIntervals = [];
   
    function PlacePhotos(photos) {

        if (current_loads === 0) {
            $('.Gallery-SubContainer').each((_, el) => {
                el.innerHTML = '';
            });
        }

        for (var i = 0; i < photos.length; i++) {
            photosArr.push(photos[i]);
        }

        MyIntervals.push(setInterval(PutPhotoToContainer, 100));     
    }

    function PutPhotoToContainer() {
        if (photosArr.length > 0) {
            var subcontainers = $('.Gallery-SubContainer');
            var SmallestContainer = subcontainers.eq(0);

            for (var j = 0; j < subcontainers.length; j++) {
                if (SmallestContainer.height() > subcontainers.eq(j).height()) {
                    SmallestContainer = subcontainers.eq(j);
                }
            }

            var photo = photosArr.shift();

            if (photo.photoHeader === null)
                photo.photoHeader = "";
            if (photo.photoDescription === null)
                photo.photoDescription = "";
            if (!photo.isVideo) {
                SmallestContainer.append("<div class='Gallery_Item'><img src='" + photo.photoBase64 + "'/><input type='hidden' value='photo'/><input type='hidden' value='" + photo.photoName + "' /><h2>" + photo.photoHeader + "</h2><p>" + photo.photoDescription + "</p></div>");
            }
            else {
                if (photos[i].photoBase64) {
                    SmallestContainer.append("<div class='Gallery_Item'><img src='" + photo.photoBase64 + "'/><input type='hidden' value='video'/><input type='hidden' value='" + photo.photoName + "' /><h2>" + photo.photoHeader + "</h2><p>" + photo.photoDescription + "</p></div>");
                }
                else {
                    SmallestContainer.append("<div class='Gallery_Item'><img src='/images/video_black.png'/><input type='hidden' value='video'/><input type='hidden' value='" + photo.photoName + "' /><h2>" + photo.photoHeader + "</h2><p>" + photo.photoDescription + "</p></div>");
                }
            }
        }
        else {
            clearInterval(MyIntervals.shift());
        }
    }

    function UploadFiles() {
        if (loading_now === false) {
            loading_now = true;

            var input = document.getElementById('file_input');
            var files = input.files;
            var formData = new FormData();

            for (var i = 0; i < files.length; i++) {
                formData.append('photos', files[i]);
            }

            ShowPopUp('Завантаження на сервер', true, false);
            $.ajax({
                url: '/api/values/UploadPhotos?path=' + path,
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                success: function () {

                    $('.Gallery-SubContainer').each((_, el) => {
                        el.innerHTML = '';
                    });

                    $('#file_input').remove();
                    $('#upload_button').append('<input id="file_input" name="photos" type="file" multiple>');
                    $('#file_input').on('change', function (e) {
                        $('#upload_photos_form').submit();
                    });

                    current_loads = 0;
                },
                error: function (xhr) {
                    ShowError(xhr);
                },
                complete: function (xhr) {
                    loading_now = false;
                    HidePopUp();
                    GetMaxAmountOfLoads();
                }
            });
        }
    }

    function CreateFolder(_path, _folderName) {
        if (loading_now === false) {
            loading_now = true;

            $.ajax({
                url: '/api/values/CreateFolder',
                type: 'POST',
                contentType: "application/json",
                data: JSON.stringify({
                    CurrentPath: _path,
                    FolderName: _folderName
                }),
                success: function (content) {
                    var selector = $('.fs_item_container').filter(function (index) {
                        if ($(this).children('button').length === 0) {
                            return true;
                        }
                        else {
                            return false;
                        }
                    });

                    if (selector.length !== 0) {
                        $("<div class='fs_item_container'><img src='/images/folder.svg' /><div>" + _folderName + "</div></div>").insertBefore(selector.first());
                    }
                    else {
                        $('.options_item').first().append("<div class='fs_item_container'><img src='/images/folder.svg' /><div>" + _folderName + "</div></div>");
                    }
                   
                    $('#create_folder_container').slideUp(1000);
                },
                error: function (xhr) {
                    $('#new_folder_name_field').val("Folder Name");
                    ShowError(xhr);
                },
                complete: function (xhr) {
                    loading_now = false;
                }
            });
        }    
    }

    function DeletePhoto() {
        if (loading_now === false) {
            loading_now = true;

            $.ajax({
                url: '/api/values/DeletePhoto',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(
                    {
                        Path: path,
                        PhotoName:selectedPhotoName
                    }),
                success: function () {
                    $('.Gallery_Item').filter(function () {
                        var name = $(this).children('input').last().val();
                        if (name === selectedPhotoName) {
                            $(this).remove();
                        }
                    });
                    $('.fs_item_container').filter(function (index) {
                        if ($(this).children('div').length !== 0) {
                            if ($(this).children('div').first().text() === selectedPhotoName) {
                                return true;
                            }
                            else {
                                return false;
                            }
                        }
                        else {
                            return false;
                        }
                    }).first().remove();
                    $('#Gradient_container').slideUp(1000);
                    ShowPopUp('Файл був видалений', false, true);
                },
                error: function (xhr) {
                    ShowError(xhr);
                },
                complete: function (xhr) {
                    current_loads = 0;
                    $.ajax({
                        url: 'api/values/GetMaxLoads?path=' + path,
                        type: 'GET',
                        dataType: 'text',
                        success: function (max_amount_of_loads) {
                            max_posible_loads = max_amount_of_loads;
                        },
                        error: function (xhr) {
                            ShowError(xhr);
                        },
                        complete: function (xhr) {
                            loading_now = false;
                        }
                    });
                }
            });
        }     
    }

    function EditPhoto() {
        if (loading_now === false) {
            loading_now = true;

            $.ajax({
                url: '/api/values/EditPhoto',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    Path: path,
                    FileName: selectedPhotoName,
                    NewHeader: $('#EditInput').val(),
                    NewDescription: $('#EditArea').val()
                }),
                success: function () {
                    $('.Gallery_Item').filter(function () {
                        var name = $(this).children('input').last().val();
                        if (name === selectedPhotoName) {
                            $(this).children('h2').text($('#EditInput').val());
                            $(this).children('p').text($('#EditArea').val());
                        }
                        ShowPopUp('Опис був змінений', false, true);
                    });
                },
                error: function (xhr) {
                    ShowError(xhr);
                },
                complete: function (xhr) {
                    loading_now = false;
                }
            });
        }
    }

    function DownloadPhoto() {
        ShowPopUp('Скачування почато',false,true);
        window.location.href = '/api/values/DownloadPhoto?path=' + path + '\\' + selectedPhotoName;
    }

    function Fill_fs_container(content) {

        if (current_loads === 0) {
            $('.options_item').empty();
        }

        if (path !== '' && current_loads === 0) {
            $('.options_item').append("<div class='fs_item_container'><button id='path_back_button'></button><div>Назад</div></div>");
            $('#path_back_button').on('click', function () {
                PathBack();
            });
        }

        if (current_loads === 0) {
            $('.options_item').append("<div class='fs_item_container'><button id='create_folder_button'></button><div>Нова папка</div></div>");
            $('#create_folder_button').on('click', function () {
                $('#create_folder_container').slideToggle(700);
            });
        }

        for (var i = 0; i < content.length; i++) {
            if (content[i].isFolder && content[i].folderEmpty) {
                $('.options_item').append("<div class='fs_item_container'><img src='/images/folder.svg' /><div>" + content[i].name + "</div></div>");
            }
            else if (content[i].isFolder && !content[i].folderEmpty) {
                $('.options_item').append("<div class='fs_item_container'><img src='/images/empty_folder.svg' /><div>" + content[i].name + "</div></div>");
            }
            else if (!content[i].isFolder && !content[i].isVideo) {
                $('.options_item').append("<div class='fs_item_container'><img src='/images/picture.svg' /><div>" + content[i].name + "</div></div>");
            }
            else if (!content[i].isFolder && content[i].isVideo) {
                $('.options_item').append("<div class='fs_item_container'><img src='/images/video.png' /><div>" + content[i].name + "</div></div>");
            }
        }
    }

    function PathBack() {
        var arr = path.split('\\');
        path = "";
        for (var i = 0; i < arr.length - 1; i++) {
            if (i < arr.length - 2)
                path += arr[i] + '\\';
            else
                path += arr[i];
        }
        $('#fs_cur_path_container p').text('Поточний шлях:' + path);

        //$('#Gallery_Container').empty();
        $('.Gallery-SubContainer').each(function (_, el) {
            $(this).empty();
        });
 
        $('.options_item').empty();

        $('#photo_container').empty();
        $('#Gradient_container').hide();

        if (active_request !== null) {
            active_request.abort();
            active_request = null;
        }

        current_loads = 0;
        GetMaxAmountOfLoads();

        $(window).scrollTop($('.options_item').first().offset().top);
    }

    function ShowPopUp(text, showimage,timeout) {
        $('#loading_container').css('display', 'flex');
        $('#loading_container').children().first().text(text);
        if (showimage === true) {
            $('#loading_container').children().last().css('display', 'block');
        } else {
            $('#loading_container').children().last().css('display', 'none');
        }
        if (timeout === true) {
            setTimeout(HidePopUp, 3000);
        }     
    }

    function HidePopUp() {
        $('#loading_container').css('display', 'none');
    }

    function ShowError(xhr) {
        if (xhr.status !== 0) {
            alert(xhr.responseText);
        }     
    }
});

