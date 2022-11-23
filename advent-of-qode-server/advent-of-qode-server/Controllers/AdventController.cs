using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using advent_of_qode_server.Logic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace advent_of_qode_server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AdventController : ControllerBase
    {
        private readonly IScoreService _scoreService;

        private AdventContext _context { get; set; }

        public AdventController(AdventContext context, IScoreService scoreService)
        {
            _context = context;
            _scoreService = scoreService;
        }

        [HttpGet]
        public IEnumerable<LeaderBoardViewModel> GetLeaderBoards()
        {
            var groups = _context.ScoreBoard
                    .ToList()
                    .GroupBy(x => x.UserEmail);

            //Remove if user has duplicate entries for that day/question
            var scores = new List<ScoreBoard>();
            foreach (var user in groups)
            {
                var unique = user.GroupBy(x => x.Question).Select(x => x.OrderByDescending(s => s.Score).First());
                scores.AddRange(unique);
            }

            //Add the number of days that the user has completed to the view model
            var highScores = new List<LeaderBoardViewModel>();
            var highScoreGroup = scores.GroupBy(x => x.UserEmail);
            foreach (var user in highScoreGroup)
            {
                var userCompletedDays = user.Where(u => u.Score != -1).ToList(); //Remove the failed days
                highScores.Add(new LeaderBoardViewModel { Email = user.Key, Score = userCompletedDays.Sum(u => u.Score), numberOfDays = userCompletedDays.Count() });
            }

            highScores = highScores.OrderByDescending(x => x.Score).ThenBy(x => x.numberOfDays).ToList();
            return highScores;
        }


        [HttpGet]
        [Route("user")]
        public IEnumerable<ScoreViewModel> GetUserScore(string userId)
        {
            var scoreViewModel = _context.ScoreBoard.Where(x => x.UserId == userId)
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
            var scoreBoard = new ScoreBoard { Created = DateTime.UtcNow, Score = 999, UserEmail = "test@gmail.com" };

            scoreBoard.Year = DateTime.UtcNow.Year;

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
            if (scoreInput.Score < -1) return BadRequest("Score is not valid");
            if (!(scoreInput.Question > 0 && scoreInput.Question < 26)) return BadRequest("Question number is not valid");
            if (string.IsNullOrWhiteSpace(scoreInput.UserId)) return BadRequest("UserId is not valid");
            var scoreRow = await _context.ScoreBoard.SingleOrDefaultAsync(x => x.UserEmail == scoreInput.Email && x.Question == scoreInput.Question);
            if (scoreRow != null)
            {
                return BadRequest("Du har redan gissat");
            }

            var startTime = _context.StartTime.SingleOrDefault(x => x.Question == scoreInput.Question && x.UserEmail == scoreInput.Email);
            if (startTime == null)
                return NotFound("Unable to find StarTime");

            var scoreBoard = await _scoreService.CreateUserScoreAsync(scoreInput.Email, startTime.Started, scoreInput.Question);

            return Created("Ok", scoreBoard);
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
        public int Score { get; set; }
        public string Email { get; set; }
        public int numberOfDays { get; set; }
    }
}
