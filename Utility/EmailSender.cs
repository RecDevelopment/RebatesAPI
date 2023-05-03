using SMTPEmailSender;


namespace RebatesAPI.Utilities
{
    public class Emails
    {
        public bool SendEmails(SMTPEmailSender.EmailDetails body, string profile, IConfiguration configuration)
        {
            try
            {
                var config = new SMTPEmailSender.EmailConfiguration();
                var emailSender = new SMTPEmailSender.EmailSender();

                config.smptIP = configuration["EmailSettings:" + profile + ":smtpIP"].ToString();
                config.smptPort = Convert.ToInt32(configuration["EmailSettings:" + profile + ":smtpPort"].ToString());
                config.userName = configuration["EmailSettings:" + profile + ":smtpuserName"].ToString();
                config.password = configuration["EmailSettings:" + profile + ":smtppassword"].ToString();

                return emailSender.SendEmails(body, config);
            }
            catch (Exception ex) { }

            return false;
        }
    }
}
