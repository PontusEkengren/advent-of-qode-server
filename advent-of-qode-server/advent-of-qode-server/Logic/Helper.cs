using System;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Internal;

namespace advent_of_qode_server.Logic
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
        public static bool QuestionMatcher(string answer, string input)
        {
            var rgx = new Regex("[^a-zA-Z0-9,]");
            var inputWashed = rgx.Replace(input, "");
            var answersWashed = rgx.Replace(answer, "");

            return string.Equals(answersWashed.ToUpperInvariant(), inputWashed.ToUpperInvariant());
        }
    }
}