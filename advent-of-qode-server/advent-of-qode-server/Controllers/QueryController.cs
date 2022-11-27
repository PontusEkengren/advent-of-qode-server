#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using advent_of_qode_server.Domain;
using advent_of_qode_server.Logic;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using static IdentityServer4.Models.IdentityResources;

namespace advent_of_qode_server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class QueryController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IGoogleService _googleService;
        private readonly IScoreService _scoreService;

        private AdventContext _context { get; set; }

        public QueryController(AdventContext context, IConfiguration configuration, IGoogleService googleService, IScoreService scoreService)
        {
            _configuration = configuration;
            _googleService = googleService;
            _scoreService = scoreService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetQuestion(int day, string email)//Use token validation instead //https://developers.google.com/identity/sign-in/web/backend-auth
        {
            var questionViewModel = new QuestionViewModel
            {
                Question = "Careful... You and your eagerness might end up on Santa's naught list!",
                Day = day
            };

            try
            {
                if (email == null)
                {
                    return StatusCode(401, "Kunde inte hitta epost");
                }

                var startTime = await _context.StartTime.SingleOrDefaultAsync(x => x.UserEmail == email && x.Question == day);
                if (startTime == null)
                {
                    await _context.StartTime
                        .AddAsync(new StartTime { Started = DateTime.UtcNow, Question = day, UserEmail = email });
                    await _context.SaveChangesAsync();
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
            questionViewModel = new QuestionViewModel
            {
                Question = question != null
                    ? question.Query
                    : "Seems like santa is missing a riddle for this day. Please let the elves know!",
                Options = question?.Options.Select(x => x.Text).ToArray(),
                Day = day
            };

            return Ok(questionViewModel);
        }

        [HttpPost]
        [Route("answer")]
        public async Task<IActionResult> Answer(AnswerInputModel answerInput, string email) //Use token validation instead //https://developers.google.com/identity/sign-in/web/backend-auth
        {
            if (string.IsNullOrWhiteSpace(answerInput.Answer)) return BadRequest("Answer cannot be empty");
            //if (answerInput.Day != DateTime.UtcNow.Day) return BadRequest();
            //if (answerInput.Time > 10) return Ok("slow");

            try
            {
                if (email == null)
                {
                    return StatusCode(401, "Kunde inte hitta epost");
                }

                var scoreRow = await _context.ScoreBoard.SingleOrDefaultAsync(x => x.UserEmail == email && x.Question == answerInput.Day);
                if (scoreRow != null)
                {
                    return BadRequest("Du har redan gissat");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(401);
            }

            try
            {
                var question =
                    await _context.Questions
                        .Include(x => x.Options)
                        .SingleOrDefaultAsync(x =>
                        x.Day == answerInput.Day && x.Year == DateTime.UtcNow.Year);
                if (question == null)
                    throw new Exception(
                        "Seems like santa is missing a riddle for this day. Please let the elves know!");

                var success = Helper.QuestionMatcher(question.Options.Single(x => x.IsCorrectAnswer).Text, answerInput.Answer);
                var startTime = _context.StartTime.SingleOrDefault(x => x.Question == answerInput.Day && x.UserEmail == email);
                if (startTime == null)
                    return NotFound("Unable to find StarTime");
                var scoreBoard = await _scoreService.CreateUserScoreAsync(email, startTime.Started, answerInput.Day, success);

                return Ok(success ? "correct" : "wrong");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }


        [HttpPut]
        public async Task<IActionResult> AddOrUpdateQuestion(QuestionInputModel queryInput)
        {
            try
            {
                var googleId = _configuration.GetSection("Authentication:Google:ClientId");
                var auth = HttpContext?.Request?.Headers["Authorization"] ?? "";
                var adminEmail = await _googleService.GetEmailByGmailToken(auth, googleId.Value);
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


            var badRequestMessage = ValidateInput(queryInput);
            if (string.IsNullOrEmpty(badRequestMessage) is false)
                return BadRequest(badRequestMessage);

            var questions = _context.Questions.Include(x => x.Options);
            var existingQuestion = questions.SingleOrDefault(x => x.Day == queryInput.Day && x.Year == DateTime.UtcNow.Year);
            if (existingQuestion == null)
            {
                existingQuestion = new Question
                {
                    Day = queryInput.Day,
                    Year = DateTime.UtcNow.Year,
                };
                _context.Questions.Add(existingQuestion);
            }

            try
            {
                existingQuestion.Options = queryInput.Options
                    .Select(x => new Option { Text = x.Text, IsCorrectAnswer = x.IsCorrectAnswer })
                    .ToList();
                existingQuestion.Query = queryInput.Question;

                await _context.SaveChangesAsync();

                return Ok(existingQuestion.Day);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static string? ValidateInput(QuestionInputModel queryInput)
        {
            if (!(queryInput.Day is > 0 and < 26)) return "Day is not valid";
            if (string.IsNullOrWhiteSpace(queryInput.Question)) return "Question cannot be empty";
            if (queryInput?.Options is null || !queryInput.Options.Any()) return "Answer options cannot be empty";
            if (queryInput.Options.Count(x => x.IsCorrectAnswer) < 1)
                return "Must have at least one correct answer";
            if (queryInput.Options.Count(x => x.IsCorrectAnswer) > 1)
                return "Only one correct answer is allowed";

            return null;
        }
    }

    public class QuestionViewModel
    {
        public string Question { get; set; }
        public int Day { get; set; }
        public string[] Options { get; set; }
    }


    public class QuestionInputModel
    {
        public int Day { get; set; }
        public string Question { get; set; }
        public IEnumerable<OptionInputModel> Options { get; set; }
    }

    public class OptionInputModel
    {
        public string Text { get; set; }
        public bool IsCorrectAnswer { get; set; }
    }

    public class AnswerInputModel
    {
        public int Day { get; set; }
        public string Answer { get; set; }
    }
}
