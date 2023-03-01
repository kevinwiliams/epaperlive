

$('#CountryCode').val('JAM');
$('#SendMail').prop('checked', true);
var sendMail = $('#SendMail').val();
$('#mailSend').val(sendMail.toString());

//console.log(sendMail);

$(document).ready(function () {
    console.log($('#mailSend').val())

    $("#eye").click();
    var parish = $("#StateParishList").val();
    $("#StateParish").val(parish).trigger('change');

    var UserRole = $("#UserRole").val();

    console.log(UserRole);
    if (UserRole != "Admin") {
        $('[name=Currency]').attr("readonly", "readonly");
        $('[name=CardAmount]').attr("readonly", "readonly");
        $('[name=SubType]').attr("readonly", "readonly");
        $('[name=PaymentType]').attr("readonly", "readonly");
        $('[name=RateDescription]').attr("readonly", "readonly");

        $('[name=ConfirmationNumber]').attr("required", "required");
        $('[name=ConfirmationNumber]').attr("maxlength", "7");
        $('[name=ConfirmationNumber]').attr("minlength", "5");


        $('[name=RateType]').each(function () {
            if ($(this).val() != 'Epaper') {
                $(this).attr("disabled", "disabled");
            }
        });

        $('#PaymentList option').each(function () {
            if ($(this).val() != 'HC-COMP' && $(this).val() != '') {
                $(this).remove();
            }
        });

        //$('#PaymentList').val('HC-COMP').trigger('change');
    }
});

$('#SendMail').on('click', (e) => {
    var mail = e.currentTarget.checked;
    $('#mailSend').val(mail.toString());
    console.log(e.currentTarget.checked);
});

$(".btn-coupon-code").on('click', function (e) {
    let Url = $(this).data("request-url");
    e.preventDefault();
    var couponCode = $("[name=Password]").val();
    // if (couponCode != "") {
    $.ajax({
        type: "POST",
        url: Url,
        data: { couponCode: couponCode },
        success: function (result) {
            console.log(result.data);
            $("[name=Password]").val(result.data);
        }
    });
    //}
});

let passwordInput = $("#Password");
let eye = $("#eye");

$(eye).click(function () {
    $(this).toggleClass("fa-eye-slash");
    const type = $(passwordInput).attr("type") === "password" ? "text" : "password";
    $(passwordInput).attr("type", type);
});

let countryCode = $('#CountryCode').val();

if (countryCode == "JAM") {
    $('#StateParishList').show();
    $('#StateParishList').attr('required', true);
    $('#StateParish').hide();
    $('#ZipCode').attr('disabled', true);
}
else {
    $('#StateParishList').hide();
    $('#StateParishList').removeAttr('required');
    $('#StateParish').show();
    $('#ZipCode').attr('disabled', false);
}

$('#CountryCode').on('change', (e) => {
    console.log($('#CountryCode').val());
    countryCode = $('#CountryCode').val();

    if (countryCode == "JAM") {
        $('#StateParishList').show();
        $('#StateParishList').attr('required', true);
        $('#StateParish').hide();
        $('#ZipCode').attr('disabled', true);
    } else {

        $('#StateParishList').hide();
        $('#StateParishList').removeAttr('required');

        $('#StateParish').show();
        $('#ZipCode').attr('disabled', false);
    }
});

$('#StateParishList').on('change', (e) => {
    $('#StateParish').val($('#StateParishList option:selected').text());
});


$("[name='GetPaymentList']").on('change', function (e) {
    let paymentType = $(this).val();
    console.log(paymentType);
    if (paymentType == "CHECK" || paymentType == "BANK" || paymentType == "HC-COMP") {
        if (paymentType == "HC-COMP") {
            $("#ConfirmationNumber").attr("placeholder", "Circulationpro Subscription Acct");
        } else {
            $("#ConfirmationNumber").attr("placeholder", "Transaction Number");
        }

        $("#ConfirmationNumber").removeClass("hidden");
    } else {
        $("#ConfirmationNumber").addClass("hidden");
    }

    if (paymentType == "COMP" || paymentType == "STAFF" || paymentType == "HC-COMP") {
        $("[name='CardAmount']").val("0.00");
    }

    $("[name='PaymentType']").val(paymentType);
});


let rateType;

$('[name=subdetails]').submit(function (event) {

    let rateType = $('[name=RateType]:checked').val();
    let deliveryAddress = $("#AddressAdded").val();

    if (!$('[name=RateType]').is(':checked')) {

        swal({
            title: "Error!",
            text: "Please select a subscription plan to proceed",
            type: "warning",
            dangerMode: true
        });

        return false;
    }

    if (rateType != "Epaper" && !deliveryAddress) {
        swal({
            title: "Error!",
            text: "Please enter a delivery address to proceed",
            type: "warning",
            dangerMode: true
        });

        return false;
    }

    if ($('[name=RateType]').is(':checked')) {
        let formData = $("[name=subdetails]").valid();
        if (formData) {
            $('#loading').addClass('loading').removeClass('hidden');
        }
    }
});

$("[name='StartDate']").on('change', function (e) {

    let givenDate = $(this).val();
    givenDate = moment(givenDate);

    if (givenDate.isBefore(currentDate)) {
        swal({
            title: "Error!",
            text: "Please select a subscription date in the future or today",
            type: "warning",
            dangerMode: true
        });

        $(this).val(currentDate);

        return false;
    } else {
        let splitDate = $(this).val().split('-');
        let summaryDateText = ': ' + givenDate.format('DD/MM/yyyy');
        $("#subStartDate").html(summaryDateText);
    }

});

$('[name="RateType"]').on("click", function () {

    today = moment();
    console.log(today.format('YYYY-MM-DD'), ' day: ', today.isoWeekday(), ' hr: ', today.hours());

    rateType = $(this).val();
    //console.log($(this).val());
    if (rateType != "Epaper") {
        $("#subtypedesc").html(rateType.toLowerCase());
        $("[name='StartDate']").removeClass('hidden');
        $("[for='StartDate']").removeClass('hidden');
        // if (today is fri) and time of day <=  10am
        today = moment();
        if (today.isoWeekday() === 5 && today.hours() <= 10) {
            currentDate = today.add(1, 'days').format('YYYY-MM-DD');
        } else {
            currentDate = today.add(2, 'days').format('YYYY-MM-DD');
        }
        //If (today is sat or sun)
        today = moment();
        if (today.isoWeekday() >= 6) {
            currentDate = moment().isoWeekday(2).format('YYYY-MM-DD');
        }
        //if (today is a mon, tues, wed or thur) and time of day <= 11am
        today = moment();
        if (today.isoWeekday() <= 4 && today.hours() <= 11) {
            currentDate = today.add(1, 'days').format('YYYY-MM-DD');
        } else {
            currentDate = today.add(2, 'days').format('YYYY-MM-DD');
        }

        $("[name='StartDate']").val(currentDate);

        $("#del-address").show();
        $("#del-instructions").show();
        $("#deliveryAddressForm").show();
        $("#deliveryAddressForm :input:not(#DeliveryAddress_AddressLine2):not([name=SameAsMailing])").attr("required", "required");
    } else {
        $("#subtypedesc").html('ePaper');
        $("[name='StartDate']").addClass('hidden');
        $("[for='StartDate']").addClass('hidden');
        currentDate = today.format('YYYY-MM-DD');

        $("[name='StartDate']").val(currentDate);

        $("#del-address").hide();
        $("#del-instructions").hide();
        $("#deliveryAddressForm").hide();
        $("#deliveryAddressForm :input:not(#DeliveryAddress_AddressLine2):not([name=SameAsMailing])").removeAttr('required');
    }

    $("[name=StartDate]").trigger("change");

});

$("[name='RateType']").click(function (e) {
    $("#rates-results").show();
    var rateType = $(this).val();
    let Url = $(this).data("request-url");
    e.preventDefault();
    $.ajax({
        type: "POST",
        url: Url,
                    data: { rateType: rateType },
        success: function (result) {
            //console.log(result);
            $("#rates-results").html(result);
            $("input[name='RateType'][value=" + rateType + "]").prop('checked', true);

            //if (preRate) {
            //    setTimeout(
            //        selectRate
            //        , 500);
            //}
        }
    });
});
// Event delegation, waits until the elements are loaded on the page
$("#rates-results").on("click", "[name=RateID]", function (e) {
    let rateDesc = $(this).parent().children().children().first().text();
    let plan = e.target.dataset;
    var desc = rateDesc.split("|")[0];
    var freq = rateDesc.split("|")[1];

    var frequency = rateDesc.split("|")[1] ? '<small>' + rateDesc.split("|")[1] + '</small>' : '';
    let rateType = plan.rateType + '|' + desc + '|' + plan.rateDesc + '|' + plan.ratePrice + '|' + freq;
    let rateId = plan.subId;

    console.log('rateDesc', rateDesc);
    $('#RateDescription').val(rateDesc);
    console.log(plan.rateType);
    $('#SubType').val(plan.rateType);
    console.log(desc);
    console.log(rateId);
    $('#RateID').val(rateId);
    console.log(plan.subId);
    console.log(plan.rateDesc);
    console.log(plan.ratePrice);
    $('#Currency').val(plan.ratePrice.split(" ")[0]);
    $('#CardAmount').val(plan.ratePrice.split("$")[1]);



});


$("[name=deliveryAddressForm]").on('submit', function (e) {
    e.preventDefault();
    let formData = $("[name=deliveryAddressForm]").valid();
    let Url = $("[name=deliveryAddressForm]").data("request-url");

    if (formData) {
        //Serialize the form datas.   
        var addressData = $("[name=deliveryAddressForm] :input").serialize();
        $.ajax({
            type: "POST",
            url: Url,
            dataType: 'json',
            data: addressData,
            success: function (result) {
                sessionStorage.setItem("delAddress", JSON.stringify(result));
                var addressLine2 = result.AddressLine2 ? `${result.AddressLine2}<br>` : '';
                var html = `<p class="p-3">${result.AddressLine1}<br>
            ${addressLine2}
            ${result.CityTown}<br>
                ${result.StateParish}<br>
                                        </p>`;
                $("#AddressAdded").val("added");
                $("#delAddressDetails").html(html);
            }
        });
    }

});

$("#rates-results").on("click", "[name=RateID]", function (e) {
    $('#PaymentList').trigger('change');
});
