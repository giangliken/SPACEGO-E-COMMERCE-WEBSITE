namespace SPACEGO_E_COMMERCE_WEBSITE.Repository
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
    }
}
