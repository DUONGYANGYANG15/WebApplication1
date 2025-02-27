using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace WebApplication1.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email,string subject,string message);
    };
    
}
