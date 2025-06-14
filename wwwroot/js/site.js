﻿// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


//Image Preview///////////////////////////////////////////////////////////////////////////////////////////////
function readURL(input) {
    if (input.files && input.files[0]) {
        let reader = new FileReader();

        reader.onload = function (e) {
            $("img#imgpreview").attr("src", e.target.result).width(200).height(200);
        };

        reader.readAsDataURL(input.files[0]);
    }
};

//Data Tables/////////////////////////////////////////////////////////////////////////////////////////////////
$(document).ready(function () {
    // Define common options for both DataTables instances
    var dataTableOptions = {
        dom: '<"row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>><"row"<"col-sm-12"tr>><"row"<"col-sm-12 col-md-5"i><"col-sm-12 col-md-7"p>>B',
        buttons: [
            {
                extend: 'copy',
                exportOptions: {
                    columns: ':visible:not(:last-child)' // Export only visible columns except the last
                }
            },
            {
                extend: 'csv',
                exportOptions: {
                    columns: ':visible:not(:last-child)' // Export only visible columns except the last
                }
            },
            {
                extend: 'excel',
                exportOptions: {
                    columns: ':visible:not(:last-child)' // Export only visible columns except the last
                }
            },
            {
                extend: 'pdf',
                exportOptions: {
                    columns: ':visible:not(:last-child)' // Export only visible columns except the last
                }
            },
            {
                extend: 'print',
                exportOptions: {
                    columns: ':visible:not(:last-child)' // Export only visible columns except the last
                }
            }
        ],
        pageLength: 10,
        lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]],
        language: {
            lengthMenu: "_MENU_ records per page"
        }
    };

    //Initialising Datatables on groups index page
    $('#groupTable').DataTable();
    //Initialising Datatables on maintenance index page
    $('#maintenanceTable').DataTable();
    //Initialising Datatables on models index page
    var modelsTable = $('#allModelsTable').DataTable(dataTableOptions);
     $('.column-toggle').change(function () {
         var columnIndex = $(this).data('column');
         modelsTable.column(columnIndex).visible(this.checked);
     });
    //Initialising Datatables on models index page
    $('#allModelsTable').DataTable();
    //Initialising Datatables on brands index page
    $('#brandsTable').DataTable();
    //Initialising Datatables on locations index page
    $('#locationsTable').DataTable();
    //Initialising Datatables on conditions index page
    $('#conditionsTable').DataTable();
    //Initialising Datatables on myLogs index page
    $('#myLogsTable').DataTable();
    //Initialising Datatables on departments index page
    $('#departmentsTable').DataTable();
    //Initialising Datatables on categories index page
    $('#categoriesTable').DataTable();



    // Handle page size changes
    $('#pageSize').change(function () {
        table.page.len($(this).val()).draw();
    });

    // Initialize #userTable with filtering and column visibility toggle
    var table = $('#userTable').DataTable(dataTableOptions);
    $('.column-toggle').change(function () {
        var columnIndex = $(this).data('column');
        table.column(columnIndex).visible(this.checked);
    });
    // Initialize #logsTable with filtering and column visibility toggle
    var table = $('#logsTable').DataTable(dataTableOptions);
    $('.column-toggle').change(function () {
        var columnIndex = $(this).data('column');
        table.column(columnIndex).visible(this.checked);
    });

    function isColumnVisible(columnIndex) {
        return $('.column-toggle[data-column="' + columnIndex + '"]').is(':checked');
    }

    // Initialize #allUsersTable with export options (excluding last column)
    $('#allUsersTable').DataTable($.extend({}, dataTableOptions, {
        columnDefs: [
            {
                targets: ':last-child',
                exportOptions: {
                    display: false
                }
            }
        ]
    }));

    // Initialize #allDevicesTable with export options (excluding last column)
    $('#allDevicesTable').DataTable($.extend({}, dataTableOptions, {
        columnDefs: [
            {
                targets: ':last-child',
                exportOptions: {
                    display: false
                }
            }
        ]
    }));

    // Handle page size changes
    $('#pageSize').change(function () {
        table.page.len($(this).val()).draw();
    });
});

//Select2///////////////////////////////////////////////////////////////////////////////////////////////
$(document).ready(function () {
    $('#aduser-search').select2({
        width: '100%'
    });

    $('#user-search').select2({
        width: '100%'
    });
    $('#userFilter').select2({
        width: '100%'
    });
    $('#brand-search').select2({
        width: '100%'
    });
    $('#brand-select').select2({
        width: '100%'
    });
    $('#model-select').select2({
        width: '100%'
    });
    $('#condition-search').select2({
        width: '100%'
    });
    $('#location-search').select2({
        width: '100%'
    });
    $('#category-search').select2({
        width: '100%'
    });
    $('#department-search').select2({
        width: '100%'
    });
    $('#location-search').select2({
        width: '100%'
    });
    $('#serial-number-select').select2({
        width: '100%'
    });
    $('#sysUser-search').select2({
        width: '100%'
    });
    $('#serial-number-select').select2({
        width: '100%'
    });
});



//Modal Popup////////////////////////////////////////////////////////////////////////////////////////////////////

function loadCreatePartial(controllerName) {
    $.ajax({
        url: '/' + controllerName + '/Create',
        type: 'GET',
        success: function (result) {
            $('#modalContainer').html(result);
            $('#createModal').modal('show');
        },
        error: function (xhr, status, error) {
            console.error('Error loading partial view:', error);
        }
    });
}
function loadCreateByCPartial(controllerName, id) {
    $.ajax({
        url: '/' + controllerName + '/CreateByCategory',
        type: 'GET',
        data: { id: id },
        success: function (result) {
            $('#modalContainer').html(result);
            $('#createModal').modal('show');
        },
        error: function (xhr, status, error) {
            console.error('Error loading partial view:', error);
        }
    });
}
function loadCreateByBPartial(controllerName, id) {
    $.ajax({
        url: '/' + controllerName + '/CreateByBrand',
        type: 'GET',
        data: { id: id },
        success: function (result) {
            $('#modalContainer').html(result);
            $('#createModal').modal('show');
        },
        error: function (xhr, status, error) {
            console.error('Error loading partial view:', error);
        }
    });
}
function loadEditPartial(controllerName, id) {
    $.ajax({
        url: '/' + controllerName + '/Edit',
        type: 'GET',
        data: { id: id },
        success: function (result) {
            $('#modalContainer').html(result);
            $('#editModal').modal('show');
        },
        error: function (xhr, status, error) {
            console.error('Error loading partial view:', error);
        }
    });
}
function loadDeletePartial(controllerName, id) {
    $.ajax({
        url: '/' + controllerName + '/Delete',
        type: 'GET',
        data: { id: id },
        success: function (result) {
            $('#modalContainer').html(result);
            $('#deleteModal').modal('show');
            console.log("success");
        },
        error: function (xhr, status, error) {

            console.log("failure");
            console.error('Error loading partial view:', error);
        }
    });
}
function loadDeleteAlloPartial(controllerName, id) {
    $.ajax({
        url: '/' + controllerName + '/DeleteFromAllocation',
        type: 'GET',
        data: { id: id },
        success: function (result) {
            $('#modalContainer').html(result);
            $('#deleteModal').modal('show');
            console.log(controllerName);
        },
        error: function (xhr, status, error) {

            console.log("failure");
            console.error('Error loading partial view:', error);
        }
    });
}

//Modal Popup////////////////////////////////////////////////////////////////////////////////////////////////////


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

///////////////////////////////////////////////////////////////////////////////Loader

//$(document).ready(function () {
//    var loaderTimeout;

//    $(document).ajaxStart(function () {
//        loaderTimeout = setTimeout(function () {
//            $("#loader").show();
//        }, 500); // Show loader after 500ms
//    }).ajaxStop(function () {
//        clearTimeout(loaderTimeout);
//        $("#loader").hide();
//    });

//    $("form").submit(function () {
//        loaderTimeout = setTimeout(function () {
//            $("#loader").show();
//        }, 500); // Show loader after 500ms
//    });

//    $(window).on('load', function () {
//        clearTimeout(loaderTimeout);
//        $("#loader").hide();
//    });

