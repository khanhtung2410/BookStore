document.addEventListener('DOMContentLoaded', function () {
    const importModal = new bootstrap.Modal(document.getElementById('importModal'));
    const importTemplateButton = document.querySelector('#getFormatExcel');

    document.querySelector('.btn-import').addEventListener('click', function () {
        importModal.show();
    });

    if (importTemplateButton) {
        importTemplateButton.addEventListener('click', function () {
            window.location.href = '/template';
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
        submitHandler: function (form, event) {
            event.preventDefault();
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
                    console.log("success");
                    setTimeout(function () {
                        window.location.href = '/Books';
                    }, 1000);   
                },
                error: function (xhr) {
                    abp.notify.error("Error importing books");
                    console.log(xhr.responseText);
                }
            });
            return false;
        }
    });

    document.getElementById('ExcelFile').addEventListener('change', function (event) {
        const file = event.target.files[0]; // get the first selected file
        if (!file) return;

        console.log("Selected file name:", file.name);
        console.log("MIME type:", file.type); // browser-reported MIME type
        console.log("File size (bytes):", file.size);
    });

});