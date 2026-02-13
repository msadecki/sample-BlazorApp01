using BlazorApp01.Domain.Models;
using Microsoft.AspNetCore.Identity;

namespace BlazorApp01.Web.Components.Account;

// Remove the "else if (EmailSender is IdentityNoOpEmailSender)" block from RegisterConfirmation.razor after updating with a real implementation.
internal sealed class IdentityNoOpEmailSender(ILogger<IdentityNoOpEmailSender> logger) : IEmailSender<ApplicationUser>
{
    /*
    private readonly IEmailSender emailSender = new NoOpEmailSender();

    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
        emailSender.SendEmailAsync(email, "Confirm your email", $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.");

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
        emailSender.SendEmailAsync(email, "Reset your password", $"Please reset your password by <a href='{resetLink}'>clicking here</a>.");

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
        emailSender.SendEmailAsync(email, "Reset your password", $"Please reset your password using the following code: {resetCode}");
    */

    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
    {
        logger.LogInformation("Confirmation link for {Email}: {ConfirmationLink}", email, confirmationLink);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        logger.LogInformation("Password reset link for {Email}: {ResetLink}", email, resetLink);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {
        logger.LogInformation("Password reset code for {Email}: {ResetCode}", email, resetCode);
        return Task.CompletedTask;
    }
}
