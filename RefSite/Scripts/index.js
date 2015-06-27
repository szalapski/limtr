function foo() {
    // do stuff
    setFooMessage("Foo invoked!");
}


$('#fooButton').click(function (eventObject) {
    $.ajax({
        type: 'POST',
        url: 'http://limtr.azurewebsites.net/api/limit?appKey=free&limitKey=foo',
        success: function (isAllowed) {
            if (isAllowed) foo();
            else setFooMessage("Sorry, you have made too many attempts. Please try again later.");
        },
        error: function () { 
            setFooMessage("Limtr Error");
        }
    });
});

function setFooMessage(value) {
    $('#fooMessage').text(value + " " + Date.now());
}



$('#myButton').click(function (eventObject) {
    $.ajax({
        type: 'POST',
        url: 'http://limtr.azurewebsites.net/api/limit?appKey=free&limitKey=myOperation1',
        success: function(isAllowed) {
            if (isAllowed) { /* invoke the full functionality here */ }
            else alert("Sorry, you have made too many attempts. Please try again later.");
        }
    });
});