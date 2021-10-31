#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using advent_of_qode_server.Domain;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace advent_of_qode_server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class QueryController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private AdventContext _context { get; set; }

        public QueryController(AdventContext context, IConfiguration configuration)
        {
            _configuration = configuration;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetQuestion(int day)
        {
            var questionViewModel = new QuestionViewModel
            {
                Question = "Careful... You and your eagerness might end up on Santa's naught list!",
                Day = day
            };

            var question = _context.Questions
                .Include(x => x.Options)
                .SingleOrDefault(x => x.Day == day && x.Year == DateTime.Now.Year);
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
        public async Task<IActionResult> Answer(AnswerInputModel answerInput)
        {
            if (string.IsNullOrWhiteSpace(answerInput.Answer)) return BadRequest("Answer cannot be empty");
            if (answerInput.Day != DateTime.Now.Day) return BadRequest();

            try
            {
                var question =
                    await _context.Questions.SingleOrDefaultAsync(x =>
                        x.Day == answerInput.Day && x.Year == DateTime.Now.Year);
                if (question == null)
                    throw new Exception(
                        "Seems like santa is missing a riddle for this day. Please let the elves know!");

                var success = Helper.QuestionMatcher(question.Options.Single(x => x.IsCorrectAnswer).Text, answerInput.Answer);
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


            var badRequestMessage = ValidateInput(queryInput);
            if (string.IsNullOrEmpty(badRequestMessage) is false)
                return BadRequest(badRequestMessage);

            var questions = _context.Questions.Include(x => x.Options);
            var existingQuestion = questions.SingleOrDefault(x => x.Day == queryInput.Day && x.Year == DateTime.Now.Year);
            if (existingQuestion == null)
            {
                existingQuestion = new Question
                {
                    Day = queryInput.Day,
                    Year = DateTime.Now.Year,
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
