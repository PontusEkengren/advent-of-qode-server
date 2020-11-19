using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace advent_of_qode_server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class QueryController : ControllerBase
    {
        private AdventContext _context { get; set; }
        private readonly ILogger<QueryController> _logger;


        public QueryController(ILogger<QueryController> logger, AdventContext context)
        {
            _context = context;
            _logger = logger;
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
                Question = question != null ? question.Query : "Seems like santa is missing a riddle for this day. Please let the elves know!",
                Day = day
            };

            return questionViewModel;
        }


        [HttpPost]
        [Route("answer")]
        public async Task<IActionResult> Answer(AnswerInputModel answerInput)
        {
            if (string.IsNullOrWhiteSpace(answerInput.Answer)) return BadRequest("Answer cannot be empty");
            if (answerInput.Day > DateTime.Now.Day) return BadRequest();

            try
            {
                var question = await _context.Questions.SingleOrDefaultAsync(x => x.Day == answerInput.Day && x.Year == DateTime.Now.Year);
                if (question == null) throw new Exception("Seems like santa is missing a riddle for this day. Please let the elves know!");

                var success = Helper.QuestionMatcher(question.Answers,answerInput.Answer);
                return Ok(success ? "correct" : "wrong");
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
            if (!(queryInput.Day > 0 && queryInput.Day < 26)) return BadRequest("Day is not valid");
            if (string.IsNullOrWhiteSpace(queryInput.Question)) return BadRequest("Question cannot be empty");
            if (string.IsNullOrWhiteSpace(queryInput.Answers)) return BadRequest("Answers cannot be empty");

            var question = new Question
            {
                Day = queryInput.Day,
                Query = queryInput.Question,
                Answers = queryInput.Answers,
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
    }

    public class QuestionViewModel
    {
        public string Question { get; set; }
        public int Day { get; set; }
    }


    public class QuestionInputModel
    {
        public int Day { get; set; }
        public string Question { get; set; }
        public string Answers { get; set; }
    }

    public class AnswerInputModel
    {
        public int Day { get; set; }
        public string Answer { get; set; }
    }
}
