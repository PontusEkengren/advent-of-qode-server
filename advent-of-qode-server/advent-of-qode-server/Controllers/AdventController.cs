using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        public IEnumerable<LeaderBoardViewModel> Get()
        {
            var groups = _context.ScoreBoard
                .ToList()
                .GroupBy(x =>
                {
                    var shortDate = x.Created.ToShortDateString();
                    return new { shortDateString = shortDate, x.UserEmail };
                });

            var leaderBoardViewModels = groups.Select(userGroups => userGroups
                    .OrderByDescending(x => x.Score)
                    .FirstOrDefault())
                .Select(scoreBoard => new LeaderBoardViewModel
                {
                    Created = scoreBoard?.Created.ToString("yyyy-MM-dd"),
                    Email = scoreBoard?.UserEmail,
                    Score = scoreBoard.Score.ToString()
                }).ToList();

            return leaderBoardViewModels;
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            var scoreBoard = new ScoreBoard { Created = DateTime.Now, Score = 999 , UserEmail = "test@gmail.com" };

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
    }

    public class LeaderBoardViewModel
    {
        public string Score { get; set; }
        public string Email { get; set; }
        public string Created { get; set; }
    }
}
