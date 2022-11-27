using System;

namespace advent_of_qode_server
{
    public class StartTime
    {
        public int Id { get; set; }
        public int Question { get; set; }
        public string UserEmail { get; set; }
        public DateTime Started { get; set; }
        public int QuestionSeen { get; set; }
    }
}