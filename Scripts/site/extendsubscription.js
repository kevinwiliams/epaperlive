// extend range validator method to treat checkboxes differently
var defaultRangeValidator = $.validator.methods.range;
$.validator.methods.range = function (value, element, param) {
    if (element.type === 'checkbox') {
        // if it's a checkbox return true if it is checked
        return element.checked;
    } else {
        // otherwise run the default validation function
        return defaultRangeValidator.call(this, value, element, param);
    }
}

$('#loading').addClass('loading').removeClass('hidden');

let prePlan = $("#rateTypeHidden").val();
let preRate = $("#RateID").val();
let preRateDesc = $("#preloadRateDesc").val();
let startDate = $("#startDateHidden").val();
console.log(startDate);
let currentDate = moment(startDate).format('YYYY-MM-DD');
let today = moment();
let tomorrow = moment().add(1, 'days').format('YYYY-MM-DD');
//let currentDate;

function selectRate() {
    if (preRateDesc) {
        if (prePlan == "Print") {
            $('div[id=' + preRateDesc + ']').addClass('show');
        }
    }

    $('[name=RateID][value=' + preRate + ']').click();
}

function selectPlan() {
    $('[name=RateType][value=' + prePlan + ']').click();
}

$(document).ready(function () {
    $('#loading').addClass('hidden').removeClass('loading');

    if (prePlan) {
        //select preloaded plan
        setTimeout(
            selectPlan
            , 500);
    }

    let rateType;

    $('[name=extendsubdetails]').submit(function (event) {

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
            let formData = $("[name=extendsubdetails]").valid();
            if (formData) {
                $('#loading').addClass('loading').removeClass('hidden');
            }
        }
    });

    $("[name=StartDate]").on('change', function (e) {

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

    $('[name=RateType]').on("click", function () {
        today = moment();
        console.log(today.format('YYYY-MM-DD'), ' day: ', today.isoWeekday(), ' hr: ', today.hours());

        rateType = $(this).val();
        //console.log($(this).val());

        if (rateType != prePlan) {
            if (rateType == "Epaper") {
                currentDate = today;
                $("[name=StartDate]").val(today);

            } else {
                currentDate = tomorrow;
                $("[name=StartDate]").val(tomorrow);
            }
        } else {
            $("[name=StartDate]").val(moment(startDate).format('YYYY-MM-DD'));
        }

        if (rateType != "Epaper") {
            $("#subtypedesc").html(rateType.toLowerCase());
            $("[name=StartDate]").removeClass('hidden');
            $("[for=StartDate]").removeClass('hidden');
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

            $("[name=StartDate]").val(currentDate);

            $("#del-address").show();
            $("#del-instructions").show();
            $("#deliveryAddressForm").show();
            $("#deliveryAddressForm :input:not(#DeliveryAddress_AddressLine2):not([name=SameAsMailing])").attr("required", "required");
        } else {
            $("#subtypedesc").html('ePaper');
            $("[name=StartDate]").addClass('hidden');
            $("[for=StartDate]").addClass('hidden');
            currentDate = today.format('YYYY-MM-DD');

            $("[name=StartDate]").val(currentDate);

            $("#del-address").hide();
            $("#del-instructions").hide();
            $("#deliveryAddressForm").hide();
            $("#deliveryAddressForm :input:not(#DeliveryAddress_AddressLine2):not([name=SameAsMailing])").removeAttr('required');
        }

        $("[name=StartDate]").trigger("change");
    });

    $("[name=RateType]").click(function (e) {
        $("#rates-results").show();
        var rateType = $(this).val();
        let Url = $(this).data("request-url");

        e.preventDefault();
        $.ajax({
            type: "POST",
            url: Url,
            data: { rateType: rateType, isRenewal: true },
            success: function (result) {
                //console.log(result);
                $("#rates-results").html(result);
                $("input[name=RateType][value=" + rateType + "]").prop('checked', true);

                if (preRate) {
                    setTimeout(
                        selectRate
                        , 500);
                }
            }
        });
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

    // Event delegation, waits until the elements are loaded on the page
    $("#rates-results").on("click", "[name=RateID]", function (e) {
        let rateDesc = $(this).parent().children().children().first().text();
        let Url = $(this).data("request-url");

        let plan = e.target.dataset;
        var desc = rateDesc.split("|")[0];
        var freq = rateDesc.split("|")[1];

        var frequency = rateDesc.split("|")[1] ? '<small>' + rateDesc.split("|")[1] + '</small>' : '';
        let rateType = plan.rateType + '|' + desc + '|' + plan.rateDesc + '|' + plan.ratePrice + '|' + freq;
        let rateId = plan.subId;


        $.ajax({
            type: "POST",
            url: Url,
            data: { rateType: rateType, rateId: rateId },
            success: function (result) {
                var html = '<strong>' + plan.rateType.toUpperCase() + ' SUBSCRIPTION</strong><br />' + desc + '<br />' + frequency + '<p class="pt-1 text-success fw-semibold fs-3">' + plan.ratePrice + '</p>';
                $("#subSummary").html(html);
            }
        });
    });


});


let address;
var mailAddress = $('#mailingAddress').val();

if (mailAddress) {
    address = JSON.parse(mailAddress);
}

$("#SameAsMailing").click(() => {
    if ($("#SameAsMailing").is(":checked")) {

        if ($('#mailingAddress').val()) {
            $("#DeliveryAddress_AddressLine1").val(address.AddressLine1);
            $("#DeliveryAddress_AddressLine2").val(address.AddressLine2);
            $("#DeliveryAddress_CityTown").val(address.CityTown);
            $("#DeliveryAddress_CityTown").val(address.CityTown).trigger('change');
            $("#DeliveryAddress_StateParish").val(address.StateParish);
            $("#DeliveryAddress_ZipCode").val(address.ZipCode);
            $("#DeliveryAddress_CountryCode").val(address.CountryCode);
        }
    }
    else {
        $("#DeliveryAddress_AddressLine1").val("");
        $("#DeliveryAddress_AddressLine2").val("");
        $("#DeliveryAddress_CityTown").val("");
        $("#DeliveryAddress_CityTown").val("").trigger('change');
        $("#DeliveryAddress_StateParish").val("");
        $("#DeliveryAddress_ZipCode").val("");
        $("#DeliveryAddress_CountryCode").val("");
    }
})
        //});
