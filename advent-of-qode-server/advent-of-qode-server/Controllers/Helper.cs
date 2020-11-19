using System;
using System.Net.Mail;

namespace advent_of_qode_server.Controllers
{
    public class Helper
    {
        public static bool IsValidEmail(string emailaddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailaddress);

                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
        public static bool QuestionMatcher(string answers, string input)
        {
            if (answers == input)
                return true;

            return false;
        }
    }
}