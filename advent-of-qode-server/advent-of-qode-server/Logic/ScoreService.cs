using System;
using System.Threading.Tasks;

namespace advent_of_qode_server.Logic
{
    public interface IScoreService
    {
        Task<ScoreBoard> CreateUserScoreAsync(string email, DateTime started, int day, bool success = true);
    }

    public class ScoreService : IScoreService
    {
        private AdventContext _context { get; set; }

        public ScoreService(AdventContext context)
        {
            _context = context;
        }

        public async Task<ScoreBoard> CreateUserScoreAsync(string email, DateTime started, int day, bool success = true)
        {
            var now = DateTime.UtcNow;
            var timeBasedScore = 0;
            if (success)
                timeBasedScore = ScoreCalulator.Calculate(started, now);

            var scoreBoard = new ScoreBoard
            {
                UserId = email,
                Question = day,
                Created = DateTime.UtcNow,
                Score = timeBasedScore,
                UserEmail = email,
                Year = DateTime.UtcNow.Year,
            };

            await _context.ScoreBoard.AddAsync(scoreBoard);
            await _context.SaveChangesAsync();

            return scoreBoard;
        }
    }
}
