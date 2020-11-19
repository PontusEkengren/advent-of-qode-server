using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace advent_of_qode_server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AdventController : ControllerBase
    {
        private AdventContext _context { get; set; }
        private readonly ILogger<AdventController> _logger;


        public AdventController(ILogger<AdventController> logger, AdventContext context)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<LeaderBoardViewModel> GetLeaderBoards()
        {
            var groups = _context.ScoreBoard
                .ToList()
                .GroupBy(x =>
                {
                    var shortDate = x.Created.ToShortDateString();
                    return new { shortDateString = shortDate, x.UserEmail };
                });

            var leaderBoardViewModels = groups.Select(userGroups => userGroups
                    .OrderBy(x => x.Score)
                    .FirstOrDefault())
                .Select(scoreBoard => new LeaderBoardViewModel
                {
                    Created = scoreBoard?.Created.ToString("yyyy-MM-dd"),
                    Email = scoreBoard?.UserEmail,
                    Score = scoreBoard.Score.ToString()
                }).ToList();

            return leaderBoardViewModels;
        }


        [HttpGet]
        [Route("user")]
        public IEnumerable<ScoreViewModel> GetUserScore(string userId)
        {
            var scoreViewModel= _context.ScoreBoard.Where(x => x.UserId == userId)
                .Select(scoreBoard => new ScoreViewModel
                {
                    Question = scoreBoard.Question,
                    Email = scoreBoard.UserEmail,
                    Score = scoreBoard.Score.ToString()
                }).ToList();

            return scoreViewModel;
        }

        [HttpPost]
        [Route("debug")]
        public async Task<IActionResult> DebugPost()
        {
            var scoreBoard = new ScoreBoard { Created = DateTime.Now, Score = 999, UserEmail = "test@gmail.com" };

            scoreBoard.Year = DateTime.Now.Year;

            try
            {
                await _context.ScoreBoard.AddAsync(scoreBoard);
                await _context.SaveChangesAsync();

                return Accepted("Ok");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateUserScore(ScoreInputModel scoreInput)
        {
            if (!Helper.IsValidEmail(scoreInput.Email)) return BadRequest("Email is not valid");
            if (scoreInput.Score < 1) return BadRequest("Score is not valid");
            if (!(scoreInput.Question > 0 && scoreInput.Question < 26)) return BadRequest("Question number is not valid");
            if (string.IsNullOrWhiteSpace(scoreInput.UserId)) return BadRequest("UserId is not valid");

            var scoreBoard = new ScoreBoard
            {
                UserId = scoreInput.UserId,
                Question = scoreInput.Question,
                Created = DateTime.Now,
                Score = scoreInput.Score,
                UserEmail = scoreInput.Email,
                Year = DateTime.Now.Year
            };

            try
            {
                await _context.ScoreBoard.AddAsync(scoreBoard);
                await _context.SaveChangesAsync();

                return Created("Ok", scoreBoard);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }

    public class ScoreViewModel
    {
        public string Score { get; set; }
        public string Email { get; set; }
        public int Question { get; set; }
    }


    public class ScoreInputModel
    {
        public string UserId { get; set; }
        public int Score { get; set; }
        public string Email { get; set; }
        public int Question { get; set; }
    }

    public class LeaderBoardViewModel
    {
        public string Score { get; set; }
        public string Email { get; set; }
        public string Created { get; set; }
    }
}
