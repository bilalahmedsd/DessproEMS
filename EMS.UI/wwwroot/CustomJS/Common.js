"use strict";


let SubmitButton_;
//, Form_, i, fields_, DropDowns_ = [], customfunction_;
const ValidationFunction = async (Form, SubmitButton, Fields, DropDowns, customfunction) => {
    //Form_ = Form;
    //SubmitButton_ = SubmitButton;
    //fields_ = Fields;
    //DropDowns_ = DropDowns;
    //customfunction_ = customfunction

    await FromInit(Form, Fields, SubmitButton, customfunction)
    $.each(DropDowns, function (index, item) {
        $(item).change(function () {
            $(Form).valid()
        })
    });

}

const ajaxPOSTCall = async (url_, data_) => {
    return new Promise(function (Resolve, Reject) {
        if (SubmitButton_ != undefined) {
            SubmitButton_.attr("data-kt-indicator", "on")
            SubmitButton_.attr("disabled", "disabled")
        }
        $.ajax({
            type: "POST",
            url: url_,
            data: data_,
            dataType: "json",
            success: function (res) {
                //console.log("success")
                if (res.isSuccess) {
                   
                    Resolve(res)
                }
                else {
                    if (res.message == "session expired") {
                        location.href = "/Auth/Login"
                    }
                    else {
                        Reject(res.message)
                    }
                }
                if (SubmitButton_ != undefined) {
                    SubmitButton_.removeAttr("data-kt-indicator")
                    SubmitButton_.removeAttr("disabled")
                }

            },
            error: function (e) {

                console.log("error", e)
                Swal.fire({
                    text: "Something Went Wrong!",
                    icon: "error",
                    buttonsStyling: !1,
                    confirmButtonText: "Ok, got it!",
                    customClass: { confirmButton: "btn btn-danger" },
                });
                if (SubmitButton_ != undefined) {
                    SubmitButton_.removeAttr("data-kt-indicator")
                    SubmitButton_.removeAttr("disabled")
                }
            }
        });

    })


}
const ajaxGETCall = async (url_, data_) => {

    return new Promise(function (Resolve, Reject) {
        if (SubmitButton_ != undefined) {
            SubmitButton_.attr("data-kt-indicator", "on")
            SubmitButton_.attr("disabled", "disabled")
        }
        $.ajax({
            type: "GET",
            url: url_,
            data: data_,
            contentType: "application/json",
            success: function (res) {
                if (res.isSuccess) {
                   
                    Resolve(res);
                }
                else {

                    if (res.message == "session expired") {
                        location.href = "/Auth/Login"
                    }
                    else {
                        Reject(res.message);
                    }
                }
                if (SubmitButton_ != undefined) {
                    SubmitButton_.removeAttr("data-kt-indicator")
                    SubmitButton_.removeAttr("disabled")
                }
            },
            error: function (e) {
                Swal.fire({
                    text: "Something Went Wrong!",
                    icon: "error",
                    buttonsStyling: !1,
                    confirmButtonText: "Ok, got it!",
                    customClass: { confirmButton: "btn btn-danger" },
                });
                if (SubmitButton_ != undefined) {
                    SubmitButton_.removeAttr("data-kt-indicator")
                    SubmitButton_.removeAttr("disabled")
                }
                Reject(e);
            }
        });
    });


}
const ajaxCallMultipartFormData = (url, Fromdata) => {
    return new Promise(function (Resolve, Reject) {
        $.ajax({
            type: 'POST',
            url: url,
            data: Fromdata,
            contentType: false,
            processData: false,
            success: function (res) {

                if (res.isSuccess) {
                    if (SubmitButton_ != undefined) {
                        SubmitButton_.removeAttr("data-kt-indicator")
                        SubmitButton_.removeAttr("disabled")
                    }
                    Resolve(res)
                }
                else {
                    if (res.message == "session expired") {
                        location.href = "/Auth/Login"
                    }
                    else {
                        Reject(res.message)
                    }
                }
                SubmitButton_.removeAttr("data-kt-indicator")
                SubmitButton_.removeAttr("disabled")

            },
            error: function (e) {
                console.log("error", e)
                Swal.fire({
                    text: "Something Went Wrong!",
                    icon: "error",
                    buttonsStyling: !1,
                    confirmButtonText: "Ok, got it!",
                    customClass: { confirmButton: "btn btn-danger" },
                });
                if (SubmitButton_ != undefined) {
                    SubmitButton_.removeAttr("data-kt-indicator")
                    SubmitButton_.removeAttr("disabled")
                }
            }
        });
    });

}


const FromInit = (Form__, fields__, SubmitButton__, customfunction__) => {
    $(Form__).validate({
        rules: fields__
    });

    $(Form__).on("submit", (e) => {
        e.preventDefault();
        if ($(Form__).valid()) {

            SubmitButton_ = SubmitButton__;
            customfunction__();

        }
    })

}
const Init = () => {
    FromInit();
}
$(document).on("change", ".check", function () {
    $(this).val($(this).is(":checked"))
})

//$(function () {
//    $('input[type="tel"]').inputmask({ alias: "phone", "clearIncomplete": true });
//});

function deserialize(form, data) {
    Object.keys(data).forEach(function (key, index) {
        if ($(form).find('[name="' + toTitleCase(key) + '"]').prop("type") == 'date') {
            let date = data[key];
            if (date != null) {
                $(form).find('[name="' + toTitleCase(key) + '"]').val(moment(date).format("yyyy-MM-DD"))
            }

        }
        else if ($(form).find('[name="' + toTitleCase(key) + '"]').prop("type") == 'checkbox') {
            let checked = data[key];
            if (checked != null) {
                $(form).find('[name="' + toTitleCase(key) + '"]').prop("checked", checked).trigger("change")
            }

        }
        else {
            $(form).find('[name="' + toTitleCase(key) + '"]').val(data[key])
        }
    });
}
function toTitleCase(str) {
    return str.replace(
        /\w\S*/g,
        function (txt) {
            return txt.charAt(0).toUpperCase() + txt.substr(1);
        }
    );
}
function Datefilter(date) {
    var retrunDate;
    var valueDate = date == null ? null : new Date(date);
    if (valueDate != null) {
        retrunDate = `${valueDate.getFullYear()}-${(valueDate.getMonth() + 1).toString().padStart(2, 0)}-${valueDate.getDate().toString().padStart(2, 0)}`;
    }
    else {
        retrunDate = " ";
    }
    return retrunDate;
}