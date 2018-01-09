$(function () {

    //eventHandlers
    $('#password').keyup(validatePassword);

    $('#confirm').keyup(validatePassword);

    //functions
    function validatePassword() {
        var password = $('#password').val();
        var confirmPassword = $('#confirm').val();

        if (password != confirmPassword) {

            $('#submit').prop('disabled', true);
            $('#passwodLabel').text("Passwords do not match");
        }

        else {
          
            $('#submit').prop('disabled', false);
            $('#passwodLabel').text("");
        }
    }

});