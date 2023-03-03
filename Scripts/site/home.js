
function ReloadPage() {
    location.reload();
}

$(document).ready(function () {
    //$("[name=subscribeNow]").click((e) => e.preventDefault());
    $("#feedbackBtn").click((e) => {
        e.preventDefault();
        var url = $(this).data("request-url");

        let formData = $("[name=feedBackForm]").valid();
        if (formData) {
            var feebBackFormData = $("[name=feedBackForm]").serialize();

            $.ajax({
                type: "POST",
                url: url,
                data: feebBackFormData,
                success: function (result) {
                    //var response = JSON.stringify(result);
                    //console.log((result));
                    var contactSection = $($.parseHTML(result)).filter('section#contact').html();
                    var captchaResult = $($.parseHTML(contactSection)).find('span#captcha_code').html();
                    //console.log(captchaResult);
                    if (captchaResult) {
                        swal({
                            title: "Error!",
                            text: captchaResult,
                            type: "warning",
                            dangerMode: true
                        });
                    } else {

                        $("[name=feedBackForm] :input").val('');

                        swal({
                            title: "Success!",
                            text: "Thanks for your feeback!",
                            type: "success",
                            dangerMode: false
                        });

                        setTimeout(
                            ReloadPage
                            , 2000);
                    }
                    //console.log((contactSection.html()));


                }
            });
        }
    });

    $("[name=subscribeNow]").on("click", function (e) {
        e.preventDefault();
        var freq = $(this).parent().children(".small").html();
        var url = $(this).data("request-url");
        var successUrl = $(this).data("success-url");
        //console.log(url);
        let plan = e.target.dataset;
        let rateText = (plan.isFeature == "true") ? $(this).parent('.btn-wrap').next('input').val() : e.target.innerText;
        freq = (plan.isFeature == "true") ? plan.delFreq : freq;

        let rateType = plan.rateType + '|' + rateText + '|' + plan.rateDesc + '|' + plan.ratePrice + '|' + freq;
        let rateId = plan.subId;
        //console.log(rateType);
        //console.log(rateId);
        //console.log(e);
   
            $.ajax({
                type: "POST",
                url: url,
                    data: {rateType: rateType, rateId: rateId },
                    success: function (result) {
                        //console.log(result['data']);
                        window.location.href = successUrl;
                        }
                });
    });

});
    