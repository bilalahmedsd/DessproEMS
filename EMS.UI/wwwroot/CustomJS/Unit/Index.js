$(document).ready(async () => {

    const Frm_fields = {};
    const Frm = $("#kt_UnitForm");
    const Frm_Button = $("#kt_UnitForm #kt_kt_UnitForm_Save");
    const Frm_Drop = ['#FkProjectManagement'];
    await ValidationFunction(Frm, Frm_Button, Frm_fields, Frm_Drop, SaveUnit);
    await Init();

    await GetProject();
})
const SaveUnit = async () => {
    let url = $('#kt_UnitForm [name="Id"]').val() == '' ? "/Unit/Add" : "/Unit/Edit"
    console.log($("#Frm_fiscalYear").serialize());
    await ajaxPOSTCall(url, $("#kt_UnitForm").serialize()).then(function (resp) {
        console.log("Saved ...", resp)
        Swal.fire({
            text: resp.msg,
            icon: "success",
            buttonsStyling: !1,
            confirmButtonText: "Ok, got it!",
            customClass: { confirmButton: "btn btn-primary" },
        });

        $('#kt_UnitForm').trigger("reset");
        $('#kt_UnitForm [name="Id"]').val('');
        
    }).catch(function (e) {
        debugger
        Swal.fire({
            title: e,
            icon: "error",
            buttonsStyling: !1,
            confirmButtonText: "Ok, got it!",
            customClass: { confirmButton: "btn btn-danger" },
        });
    })


}

const GetProject = async () => {
    await ajaxGETCall("/Project/Get", null).then(function (resp) {
        console.log("Get ...", resp);
        $.map(resp.data, function (item) {
            $('[name="FkProjectManagement"]').append(`<option value=${item.id}>${item.projectName}</option>`);
        })
       
      
    });
}