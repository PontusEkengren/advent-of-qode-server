#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using advent_of_qode_server.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace advent_of_qode_server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class QueryController : ControllerBase
    {
        private AdventContext _context { get; set; }


        public QueryController(AdventContext context)
        {
            _context = context;
        }

        [HttpGet]
        public QuestionViewModel GetQuestion(int day)
        {
            var questionViewModel = new QuestionViewModel
            {
                Question = "Careful... You and your eagerness might end up on Santa's naught list!",
                Day = day
            };

            if (day > DateTime.Now.Day) return questionViewModel;
            var question = _context.Questions.SingleOrDefault(x => x.Day == day && x.Year == DateTime.Now.Year);
            questionViewModel = new QuestionViewModel
            {
                Question = question != null
                    ? question.Query
                    : "Seems like santa is missing a riddle for this day. Please let the elves know!",
                Options = question?.Options.Select(x => x.Text).ToArray(),
                Day = day
            };

            return questionViewModel;
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

        [HttpPut("edit")]
        public async Task<IActionResult> EditExistingQuestion(QuestionInputModel queryInput)
        {
            var badRequestMessage = ValidateInput(queryInput);
            if (string.IsNullOrEmpty(badRequestMessage) is false)
                return BadRequest(badRequestMessage);

            var questions = _context.Questions.Include(x => x.Options);
            var existingQuestion = questions.SingleOrDefault(x => x.Day == queryInput.Day && x.Year == DateTime.Now.Year);
            if (existingQuestion == null)
                return NotFound("Could not find a question for this day");

            try
            {
                existingQuestion.Options = queryInput.Options
                    .Select(x => new Option{ Text = x.Text, IsCorrectAnswer = x.IsCorrectAnswer })
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

        [HttpPost]
        public async Task<IActionResult> CreateQuestion(QuestionInputModel queryInput)
        {
            var badRequestMessage = ValidateInput(queryInput);
            if (string.IsNullOrEmpty(badRequestMessage) is false)
                return BadRequest(badRequestMessage);
            
            var questions = _context.Questions.Include(x => x.Options);
            
            if (questions.SingleOrDefault(x => x.Day == queryInput.Day && x.Year == DateTime.Now.Year) != null)
                return BadRequest($"That day already has a question in the database for day {queryInput.Day}");

            var question = new Question
            {
                Day = queryInput.Day,
                Query = queryInput.Question,
                Options = queryInput.Options
                    .Select(x => new Option
                    {
                        Text = x.Text,
                        IsCorrectAnswer = x.IsCorrectAnswer
                    })
                    .ToList(),
                Year = DateTime.Now.Year,
            };

            try
            {
                await _context.Questions.AddAsync(question);
                await _context.SaveChangesAsync();

                return Created("Ok", question);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static string? ValidateInput(QuestionInputModel queryInput)
        {
            string? badRequestMessage = null;
            if (!(queryInput.Day > 0 && queryInput.Day < 26)) badRequestMessage = "Day is not valid";
            if (string.IsNullOrWhiteSpace(queryInput.Question)) badRequestMessage = "Question cannot be empty";
            if (!queryInput.Options.Any()) badRequestMessage = "Question cannot be empty";
            if (queryInput.Options.Count(x => x.IsCorrectAnswer) != 1)
                badRequestMessage = "One and only one option has to match the answer";

            return badRequestMessage;
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
