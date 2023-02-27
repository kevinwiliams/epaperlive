
//remove labels
$(".usercard-cc").html("");
let address;
var mailAddress = $('#mailingAddress').val();

if (mailAddress) {
    address = JSON.parse(mailAddress);
}

let subType = $('#SubType').val();

$("#SameAsMailing").click(() => {
    if ($("#SameAsMailing").is(":checked")) {

        if ($('#mailingAddress').val()) {
            $("#BillingAddress_AddressLine1").val(address.AddressLine1);
            $("#BillingAddress_AddressLine2").val(address.AddressLine2);
            $("#BillingAddress_CityTown").val(address.CityTown).change();
            $("#BillingAddress_StateParish").val(address.StateParish);
            $("#BillingAddress_ZipCode").val(address.ZipCode);
            $("#BillingAddress_CountryCode").val(address.CountryCode);
        }
    }
    else {
        $("#BillingAddress_AddressLine1").val("");
        $("#BillingAddress_AddressLine2").val("");
        $("#BillingAddress_CityTown").val("");
        $("#BillingAddress_StateParish").val("");
        $("#BillingAddress_ZipCode").val("");
        $("#BillingAddress_CountryCode").val("");
    }
})

$(document).ready(function () {

    console.log($('#TransactionSummary').val());
    console.log($('#ErrorMessages').val());

    let printDate = $('#printStartDate').val();
    let ePaperDate = $('#ePaperStartDate').val();
    console.log('printDate', printDate);
    console.log('ePaperDate', ePaperDate);



    printDate = moment(printDate, 'MM/DD/yyyy');
    ePaperDate = moment(ePaperDate, 'MM/DD/yyyy');

    console.log('mPrint', printDate);

    let summaryPrintDateText = ': ' + printDate.format('DD/MM/yyyy');
    let summaryEpaperDateText = ': ' + ePaperDate.format('DD/MM/yyyy');

    $("#subPStartDate").html(summaryPrintDateText);

    $("#subStartDate").html(summaryEpaperDateText);

    switch (subType) {
        case "Print":
            $("#sub-dt-p").removeClass('hidden');
            $("#sub-dt-e").addClass('hidden');
            break;
        case "Epaper":
            $("#subtypedesc").html('ePaper');
            break;
        case "Bundle":
            $("#sub-dt-p").removeClass('hidden');
            $("#subtypedesc").html('ePaper');
            break;
        default:
            break;
    }

    if (subType == "Epaper") {
    }
    if (typeof printDate !== "undefined") {

    }




    $("input[name='CardType']").click((e) => e.preventDefault());


    $(".btn-promo-code").on('click', function (e) {
        e.preventDefault();
        let Url = $(this).data("request-url");
        console.log(Url);
        $('#originalAmount').val($("[name=CardAmount]").val());
        var promoCode = $("[name=PromoCode]").val();
        if (promoCode != "") {
            $('#loading').addClass('loading').removeClass('hidden');
            $.ajax({
                type: "POST",
                url: Url,
                data: { promoCode: promoCode },
                beforeSend: function () {
                    $('#loading').addClass('loading').removeClass('hidden');
                },
                success: function (result) {
                    $('#loading').addClass('hidden').removeClass('loading');
                    console.log(result);
                    if (result.applied) {
                        let originalAmount = $('#originalAmount').val();
                        $("[name=CardAmount]").val(result.data.CardAmount);
                        let cardAmount = result.data.CardAmount;
                        originalAmount = parseFloat(originalAmount, 10).toFixed(2).replace(/(\d)(?=(\d{3})+\.)/g, "$1,").toString();
                        cardAmount = parseFloat(cardAmount, 10).toFixed(2).replace(/(\d)(?=(\d{3})+\.)/g, "$1,").toString();

                        $('span.na').html('$' + originalAmount);
                        $('span#cardAmount').html('$' + cardAmount);
                        $('#promoAlert').html(result.msg);
                        let currency = $("[name=Currency]").val();
                        $('p#subRate').html(currency + ' $' + cardAmount);


                    }
                },
                complete: function () {
                    $('#loading').addClass('hidden').removeClass('loading');
                },
            });
        }


    });

    $("[name=PaymentDetails]").on('submit', function (e) {
        e.preventDefault();
        let formData = $("[name=PaymentDetails]").valid();
        let Url = $("[name=PaymentDetails]").data("request-url");
        if (formData) {

            //Serialize the form datas.   
            var paymentData = $("[name=PaymentDetails] :input").serialize();
            paymentData += '&nextBtn=Pay';

            $.ajax({
                type: "POST",
                url: Url,
                dataType: 'json',
                data: paymentData,
                contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
                beforeSend: function () {
                    $('#loading').addClass('loading').removeClass('hidden');
                },
                success: function (result) {
                    if (result) {
                        //console.log(result);
                        let routeData = result["TransactionSummary"]["Merchant3DSResponseHtml"];

                        if (routeData) {
                            //routeData = $.parseHTML(routeData);
                            $('#paymentGatway').html(routeData);

                            setTimeout(
                                submitForm, 100
                            );
                        }
                    }
                }
            });
        }
    });
});

function submitForm() {
    $('#paymentGatway').find('form').submit();
    $('#paymentGatway').find('form input[name=submit]').click();
}

// Needed for KeyCard
$("#CardNumber").on("input", (e) => {
    if ($("#CardNumber").val().startsWith("7")) {
        $("input[value='KeyCard']").prop("checked", true);
    }
    else if ($("#CardNumber").val().length == 0) {
        $("input[name='CardType']").prop("checked", false);
    }
});

//$("#CardCVV").on("input", (e) => {
//    $("#CardCVV").val($("#CardCVV").val().replace(/\D/g, ''));
//});

let cardNumber = new Cleave('#CardNumber', {
    creditCard: true,
    onCreditCardTypeChanged: (type) => {
        /*$("input[name='CardType']").prop("checked", false);*/
        if (type == 'visa') {
            $("input[value='Visa']").prop("checked", true);
        }
        else if (type == 'mastercard') {
            $("input[value='Mastercard']").prop("checked", true);
        }
        //console.log(type);
    }
});

let cardCVV = new Cleave('#CardCVV', {
    blocks: [3],
    numericOnly: true
})

let expirationDate = new Cleave('#CardExp', {
    date: true,
    datePattern: ['m', 'y']
});

function addCommas(nStr) {
    nStr += '';
    x = nStr.split('.');
    x1 = x[0];
    x2 = x.length > 1 ? '.' + x[1] : '';
    var rgx = /(\d+)(\d{3})/;
    while (rgx.test(x1)) {
        x1 = x1.replace(rgx, '$1' + ',' + '$2');
    }
    return x1 + x2;
}
