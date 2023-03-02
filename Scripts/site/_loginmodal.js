let _passwordInput = $("#PasswordModal");
let _eye = $("#eyeModal");

$(_eye).click(function () {
    $(this).toggleClass("fa-eye-slash");
    const type = $(_passwordInput).attr("type") === "password" ? "text" : "password";
    $(_passwordInput).attr("type", type);
});