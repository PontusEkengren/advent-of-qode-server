using System.Collections.Generic;

namespace advent_of_qode_server.Domain
{
    public class Question
    {
        public int Id { get; set; }
        public string Query { get; set; }
        public List<Option> Options { get; set; }
        public int Day { get; set; }
        public int Year { get; set; }
    }
}