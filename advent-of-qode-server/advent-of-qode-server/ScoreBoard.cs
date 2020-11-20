using System;

namespace advent_of_qode_server
{
    public class ScoreBoard
    {
        public int Id { get; set; }
        public int Year { get; set; }
        public int Question { get; set; }
        public int Score { get; set; }
        public string UserEmail { get; set; }
        public string UserId{ get; set; }
        public DateTime Created { get; set; }
    }
}