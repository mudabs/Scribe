// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


function readURL(input) {
    if (input.files && input.files[0]) {
        let reader = new FileReader();

        reader.onload = function (e) {
            $("img#imgpreview").attr("src", e.target.result).width(200).height(200);
        };

        reader.readAsDataURL(input.files[0]);
    }
};


//Modal Popup
//$(function () {
//    var PlaceHolderElement = $('#editModal');
//    console.log("Hello");
//    $('button[data-toggle="ajax-modal"]').click(function (event) {
//        console.log("Hello2");
//        var url = $(this).data('url');
//        $.get(url).done(function (data) {
//            PlaceHolderElement.html(data);
//            PlaceHolderElement.find('.modal').modal('show');
//        })
//    })
//});

/////////////////////////////////////////////////////////////////////////
//multilevel dropdown
//document.addEventListener("DOMContentLoaded", function () {
//    // make it as accordion for smaller screens
//    if (window.innerWidth < 992) {

//        // close all inner dropdowns when parent is closed
//        document.querySelectorAll('.navbar .dropdown').forEach(function (everydropdown) {
//            everydropdown.addEventListener('hidden.bs.dropdown', function () {
//                // after dropdown is hidden, then find all submenus
//                this.querySelectorAll('.submenu').forEach(function (everysubmenu) {
//                    // hide every submenu as well
//                    everysubmenu.style.display = 'none';
//                });
//            })
//        });

//        document.querySelectorAll('.dropdown-menu a').forEach(function (element) {
//            element.addEventListener('click', function (e) {
//                let nextEl = this.nextElementSibling;
//                if (nextEl && nextEl.classList.contains('submenu')) {
//                    // prevent opening link if link needs to open dropdown
//                    e.preventDefault();
//                    if (nextEl.style.display == 'block') {
//                        nextEl.style.display = 'none';
//                    } else {
//                        nextEl.style.display = 'block';
//                    }

//                }
//            });
//        })
//    }
//    // end if innerWidth
//});
//// DOMContentLoaded  end


//Custom Notification

if ($("div.alert.notification").length) {
    setTimeout(() => {
        $("div.alert.notification").fadeOut();
    }, 2000);
}

