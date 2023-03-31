
$('#loading').addClass('loading').removeClass('hidden');

let prePlan = $("#preloadPlan").val();
let preRate = $("#preloadRate").val();
let preRateDesc = $("#preloadRateDesc").val();
let today = moment();
let tomorrow = moment().add(1, 'days').format('YYYY-MM-DD');
let currentDate;

var windowWidth = Math.min(window.screen.width, window.outerWidth);
var mobile = windowWidth < 500;
console.log('mobile', mobile);

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
    $('#NewsletterSignUp').prop('checked', true);
    $('#NotificationEmail').prop('checked', true);

    $('#loading').addClass('hidden').removeClass('loading');

    if (prePlan) {
        //select preloaded plan
        setTimeout(
            selectPlan
            , 500);
    }

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
        //scroll on mobile phone
        if (mobile) {
            let anchor = $(".plans").offset();
            window.scrollTo(anchor.left, anchor.top);
        }

        today = moment();
        //console.log(today.format('YYYY-MM-DD'), ' day: ', today.isoWeekday(), ' hr: ', today.hours());

        rateType = $(this).val();
        //console.log($(this).val());
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
        //console.log(Url);
        e.preventDefault();
        $.ajax({
            type: "POST",
            url: Url,
            data: { rateType: rateType },
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
        //console.log(Url);
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


$("#SameAsMailing").click(() => {
    if ($("#SameAsMailing").is(":checked")) {
        $("#DeliveryAddress_AddressLine1").prop("disabled", true);
        $("#DeliveryAddress_AddressLine2").prop("disabled", true);
        $("#DeliveryAddress_CityTown").prop("disabled", true);
        $("#DeliveryAddress_StateParish").prop("disabled", true);

        if (sessionStorage.getItem("delAddress")) {
            let address = JSON.parse(sessionStorage.getItem("delAddress"));
            $("#DeliveryAddress_AddressLine1").val(address.AddressLine1);
            $("#DeliveryAddress_AddressLine2").val(address.AddressLine2);
            $("#DeliveryAddress_StateParish").val(address.StateParish);
            $("#DeliveryAddress_CityTown").val(address.CityTown);
            $("#DeliveryAddress_CityTown").val(address.CityTown).trigger('change');
        }
    }
    else {
        $("#DeliveryAddress_AddressLine1").prop("disabled", false);
        $("#DeliveryAddress_AddressLine2").prop("disabled", false);
        $("#DeliveryAddress_CityTown").prop("disabled", false);
        $("#DeliveryAddress_StateParish").prop("disabled", false);

        $("#DeliveryAddress_AddressLine1").val("");
        $("#DeliveryAddress_AddressLine2").val("");
        $("#DeliveryAddress_CityTown").val("");
        $("#DeliveryAddress_CityTown").val("").trigger('change');
        $("#DeliveryAddress_StateParish").val("");
    }
})
        //});
