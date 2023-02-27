const passwordInput = $("#Password");
const passwordConfirmInput = $("#ConfirmPassword");
const eye = $("#eye");
const eyeC = $("#eyeC");

$(eye).click(function () {
    $(this).toggleClass("fa-eye-slash");
    const type = $(passwordInput).attr("type") === "password" ? "text" : "password";
    $(passwordInput).attr("type", type);
});

$(eyeC).click(function () {
    $(this).toggleClass("fa-eye-slash");
    const typeC = $(passwordConfirmInput).attr("type") === "password" ? "text" : "password";
    $(passwordConfirmInput).attr("type", typeC);
});


$('[name=LoginDetails]').on('submit', (e) => {
    let formData = $("[name=LoginDetails]").valid();
    if (formData) {
        $('#loading').addClass('loading').removeClass('hidden');

    }
});