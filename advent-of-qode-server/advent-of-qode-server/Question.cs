namespace advent_of_qode_server
{
    public class Question
    {
        public int Id { get; set; }
        public string Query{ get; set; }
        public string Answers{ get; set; }//Comma separated cuz lazy af?
        public int Day { get; set; }
        public int Year { get; set; }
    }
}