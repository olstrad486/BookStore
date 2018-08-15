using Domain.Abstract;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Domain.Concrete
{
    public class EmailSetting
    {
        public string MailToAdress = "order@example.com";
        public string MailFromAdress = "bookstore@example.com";
        public bool UseSsl = true;
        public string Username = "MySmtpUsername";
        public string Password = "MySmtpPassword";
        public string ServerName = "smtp.example.com";
        public int ServerPort = 587;
        public bool WriteAsFile = true;
        public string FileLication = @"c:\book_store_emails";
    }

    public class EmailOrderProcessor : IOrderProcessor
    {
        private EmailSetting emailSetting;
        public EmailOrderProcessor(EmailSetting setting)
        {
            emailSetting = setting;
        }

        public void ProcessOrder(Cart cart, ShoppingDetails shoppingDetails)
        {
            throw new NotImplementedException();
        }

        public void ProcessorOrder(Entities.Cart cart, Entities.ShoppingDetails shoppingDetails)
        {
            using (var smtpClient = new SmtpClient())
            {
                smtpClient.EnableSsl = emailSetting.UseSsl;
                smtpClient.Host = emailSetting.ServerName;
                smtpClient.Port = emailSetting.ServerPort;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(emailSetting.Username, emailSetting.Password);

                if (emailSetting.WriteAsFile)
                {
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                    smtpClient.PickupDirectoryLocation = emailSetting.FileLication;
                    smtpClient.EnableSsl = false;
                }

                StringBuilder body = new StringBuilder()
                    .AppendLine("New order compete")
                    .AppendLine("---")
                    .AppendLine("Goods");

                foreach (var line in cart.Lines)
                {
                    var subtotal = line.Book.Price * line.Quantity;
                    body.AppendFormat("{0} x {1} (Total: {2:c}", line.Quantity, line.Book.Name, subtotal);
                }

                body.AppendFormat("Total cost: {0:c}", cart.ComputeTotalValue())
                    .AppendLine("---")
                    .AppendLine("Delivery:")
                    .AppendLine(shoppingDetails.Name)
                    .AppendLine(shoppingDetails.Line1)
                    .AppendLine(shoppingDetails.Line2 ?? "")
                    .AppendLine(shoppingDetails.Line3 ?? "")
                    .AppendLine(shoppingDetails.City)
                    .AppendLine(shoppingDetails.Country)
                    .AppendLine("---")
                    .AppendFormat("Gift wrap: {0}", shoppingDetails.GiftWrap ? "Yes":"No");

                MailMessage mailMessage = new MailMessage(
                    emailSetting.MailFromAdress,
                    emailSetting.MailToAdress,
                    "New order complete!",
                    body.ToString()
                    );

                if (emailSetting.WriteAsFile)
                {
                    mailMessage.BodyEncoding = Encoding.UTF8;
                }

                smtpClient.Send(mailMessage);
            }
        }
    }
}
