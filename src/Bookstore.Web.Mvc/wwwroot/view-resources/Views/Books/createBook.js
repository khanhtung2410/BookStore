$('#createBookForm').on('submit', function (e) {
    e.preventDefault();

    var form = $(this);
    $.ajax({
        url: form.attr('action'),
        method: 'POST',
        data: form.serialize(), 
        success: function (response) {
            abp.notify.success("Book created successfully!");
            setTimeout(function () {
                window.location.href = '/Books';
            }, 800);
        },
        error: function (xhr) {
            abp.notify.error("Error creating book");
            console.log(xhr.responseText);
        }
    });
});
