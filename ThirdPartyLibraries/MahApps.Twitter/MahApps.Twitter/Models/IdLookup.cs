namespace MahApps.Twitter.Models
{
    public class IdLookup : ITwitterResponse
    {
        public int previous_cursor { get; set; }
        public int[] ids { get; set; }
        public string previous_cursor_str { get; set; }
        public int next_cursor { get; set; }
        public string next_cursor_str { get; set; }
    }
}