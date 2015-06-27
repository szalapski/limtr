function foo(data){
    $('#fooMessage').text("invoked foo " + Date.now());

}

$('#fooButton').click(function (eventObject) {
    $.ajax({
        type: 'POST',
        url: 'http://limtr.azurewebsites.net/api/limit?appKey=free&limitKey=foo',
        success: foo,
        error: function () {
            $('#fooMessage').text("foo error");

        }
    });
});


$('#barMessage').text("further implementation");