document.addEventListener('DOMContentLoaded', function () {
    const importModal = new bootstrap.Modal(document.getElementById('importModal'));
    const importTemplateButton = document.querySelector('#getFormatExcel');

    document.querySelector('.btn-import').addEventListener('click', function () {
        importModal.show();
    });

    if (importTemplateButton) {
        importTemplateButton.addEventListener('click', function () {
            window.location.href = '/template';
            console.log("Import template download initiated");
        });
    }
    $("#importForm").validate({
        ignore: [],
        rules: {
            ExcelFile: {
                required: true,
                extension: "xls|xlsx"
            }
        },
        messages: {
            ExcelFile: {
                required: "Please select an Excel file",
                extension: "Only .xls or .xlsx files are allowed"
            }
        },
        errorClass: 'text-danger small',
        submitHandler: function (form) {
            var formData = new FormData(form);
            $.ajax({
                url: $(form).attr('action'),
                method: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                success: function () {
                    abp.notify.success("Books imported successfully!");
                    importModal.hide();
                    setTimeout(function () {
                        window.location.href = '/Books';
                    }, 500);
                },
                error: function (xhr) {
                    abp.notify.error("Error importing books");
                    console.log(xhr.responseText);
                }
            });
        }
    });
});