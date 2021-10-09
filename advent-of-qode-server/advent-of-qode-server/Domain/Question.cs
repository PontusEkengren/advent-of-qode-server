using System.Collections.Generic;

namespace advent_of_qode_server.Domain
{
    public class Question
    {
        public int Id { get; set; }
        public string Query { get; set; }
        public string Answer { get; set; }
        public IEnumerable<Option> Options { get; set; }
        public int Day { get; set; }
        public int Year { get; set; }
    }
}