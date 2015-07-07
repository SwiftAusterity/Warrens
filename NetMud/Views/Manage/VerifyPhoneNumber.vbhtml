@ModelType VerifyPhoneNumberViewModel
@Code
    ViewBag.Title = "Verify Phone Number"
End Code

<h2>@ViewBag.Title.</h2>

@Using Html.BeginForm("VerifyPhoneNumber", "Manage", FormMethod.Post, New With { .class = "form-horizontal", .role = "form" })
    @Html.AntiForgeryToken()
    @Html.Hidden("phoneNumber", Model.PhoneNumber)
    @<text>
    <h4>Enter verification code</h4>
    <h5>@ViewBag.Status</h5>
    <hr />
    @Html.ValidationSummary("", New With { .class = "text-danger" })
    <div class="form-group">
        @Html.LabelFor(Function(m) m.Code, New With { .class = "col-md-2 control-label" })
        <div class="col-md-10">
            @Html.TextBoxFor(Function(m) m.Code, New With { .class = "form-control" })
        </div>
    </div>
    <div class="form-group">
        <div class="col-md-offset-2 col-md-10">
            <input type="submit" class="btn btn-default" value="Submit" />
        </div>
    </div>
    </text>
End Using

@Section Scripts
    @Scripts.Render("~/bundles/jqueryval")
End Section
