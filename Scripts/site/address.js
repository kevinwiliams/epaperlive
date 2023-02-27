$(document).ready(function () {
    var parish = $("#StateParishList").val();
    $("#StateParish").val(parish).trigger('change');
});

let countryCode = $('#CountryCode').val();

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



$('[name=AddressDetails]').on('submit', (e) => {
    let formData = $("[name=AddressDetails]").valid();
    if (formData) {
        $('#loading').addClass('loading').removeClass('hidden');
        return true;
    }
});

let phoneNumber = new Cleave('#Phone', {
    phone: true,
    phoneRegionCode: 'JM'
});