function GetSiteURL() {
    var str = window.location.href;
    var res = str.split("/");
    var URL = '';
    if (res[2].toLowerCase() === 'localhost'.toLowerCase() || res[2].toLowerCase() === '127.0.0.1'.toLowerCase()) {
        URL = window.location.protocol + "//" + res[2].toLowerCase() + "/" + res[3].toLowerCase();
    }
    else {
        URL = window.location.protocol + "//" + res[2].toLowerCase() + "/";
    }
    SiteURL = URL;
    return URL;
}

function ShowMessage(message, messagetype, onClose) {
    //var cssclass;
    //$("#alert_container").show();
    //switch (messagetype) {
    //    case 'Success':
    //        cssclass = 'alert-success'
    //        break;
    //    case 'Error':
    //        cssclass = 'alert-danger'
    //        break;
    //    case 'Warning':
    //        cssclass = 'alert-warning'
    //        break;
    //    default:
    //        cssclass = 'alert-info'
    //}
    //$('#alert_container').html('');
    //$('#alert_container').append('<div id="alert_div" style="margin: 0 0.5%; -webkit-box-shadow: 3px 4px 6px #999;" class="alert fade in ' + cssclass + '"><a href="#" class="close" data-dismiss="alert" aria-label="close">&times;</a><strong>' + messagetype + '!</strong> <span>' + message + '</span></div>');
    //setTimeout(function () { $("#alert_container").hide(); }, 40000);

    //var alertPlaceholder = document.getElementById('alert_container')

    $("#alert_container").show();
    $('#alert_container').html('');
    var wrapper = document.createElement('div')
    wrapper.innerHTML = '<div class="alert alert-' + messagetype + ' alert-dismissible" role="alert">' + message + '<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button></div>'

    $("#alert_container").append(wrapper)
    setTimeout(function () { $("#alert_container").hide(); }, 5000);

}

function PasswordVerification(thisPassword) {
    
    var Issuccess = true;
    var passwordregex6digits = new RegExp("^(?=.{8,12})");
    var passwordregexLowercase = new RegExp("^(?=.*[a-z])");
    var passwordregexUppercase = new RegExp("^(?=.*[A-Z])");
    var passwordregexNumber = new RegExp("^(?=.*[0-9])");
    var passwordRegexSpecial = new RegExp("^(?=.*[!@#$%^&*])");
    var passwordRegexAll = new RegExp("^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*])(?=.{8,12})");

    if (!passwordregex6digits.test(thisPassword)) {
        //ShowMessage('Required Character Length:8-12', "Error");
        return false;
    }

    if (!passwordregexLowercase.test(thisPassword) && Issuccess) {
        //ShowMessage('Required Minimum 1 Lowercase Character', "Error");
        return false;
    }

    if (!passwordregexUppercase.test(thisPassword) && Issuccess) {
        //ShowMessage('Required Minimum 1 Uppercase Character', "Error");
        return false;
    }

    if (!passwordregexNumber.test(thisPassword) && Issuccess) {
        //ShowMessage('Required Minimum 1 Digit Character', "Error");
        return false;
    }

    if (!passwordRegexSpecial.test(thisPassword) && Issuccess) {
        //ShowMessage('Required Minimum 1 Special Character', "Error");
        return false;
    }
    if (!passwordRegexAll.test(thisPassword) && Issuccess) {
        //ShowMessage('Invalid Password ', "Error");
        return false;

    }
    else {
        return true;
    }

}
// This script is for show hide password(Toggle)
//function showHidePassword() {
//    var x = document.getElementById("Password");
//    if (x.type === "password") {
//        x.type = "text";
//    } else {
//        x.type = "password";
//    }
//}