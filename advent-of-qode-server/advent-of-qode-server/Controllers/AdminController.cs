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
        private readonly IGoogleService _googleService;

        private AdventContext _context { get; set; }

        public AdminController(AdventContext context, IConfiguration configuration, IGoogleService googleService)
        {
            _configuration = configuration;
            _context = context;
            _googleService = googleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetQuestion(int day)
        {
            try
            {
                var googleId = _configuration.GetSection("Authentication:Google:ClientId").Value;
                var auth = HttpContext?.Request?.Headers["Authorization"] ?? "";
                var adminEmail = await _googleService.GetEmailByGmailToken(auth, googleId);
                if (!_configuration.GetSection("Uniqode:Admins").Value.Contains(adminEmail))
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
                .SingleOrDefault(x => x.Day == day && x.Year == DateTime.UtcNow.Year);
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
