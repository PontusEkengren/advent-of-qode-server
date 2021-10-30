#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace advent_of_qode_server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private AdventContext _context { get; set; }

        public AdminController(AdventContext context, IConfiguration configuration)
        {
            _configuration = configuration;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetQuestion(int day)
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"];
                var admin = await GoogleJsonWebSignature.ValidateAsync(token,
                    new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { _configuration.GetSection("Authentication:Google:ClientId").Value }
                    });

                if (!_configuration.GetSection("Uniqode:Admins").Value.Contains(admin.Email))
                {
                    return StatusCode(401);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(401);
            }

            var question = _context.Questions
                .Include(x => x.Options)
                .SingleOrDefault(x => x.Day == day && x.Year == DateTime.Now.Year);
            var questionViewModel = new QuestionWithAnswersViewModel
            {
                Question = question != null
                    ? question.Query
                    : "Seems like santa is missing a riddle for this day. Please let the elves know!",
                Options = question?.Options?.Select(x => new OptionsViewModel
                {
                    Text = x.Text,
                    IsCorrectAnswer = x.IsCorrectAnswer
                }).ToList(),
                Day = day
            };

            return Ok(questionViewModel);
        }
    }

    public class QuestionWithAnswersViewModel
    {
        public string Question { get; set; }
        public int Day { get; set; }
        public List<OptionsViewModel>? Options { get; set; }
    }

    public class OptionsViewModel
    {
        public string Text { get; set; }
        public bool IsCorrectAnswer { get; set; }
    }
}
