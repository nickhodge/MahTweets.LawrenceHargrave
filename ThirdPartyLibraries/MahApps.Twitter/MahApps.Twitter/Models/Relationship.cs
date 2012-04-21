namespace MahApps.Twitter.Models
{
    public class RelationshipWrapper : ITwitterResponse
    {
        public Relationship Relationship { get; set; }
    }

    public class Relationship : ITwitterResponse
    {
        public RelationShipEntity Target { get; set; }
        public RelationShipEntity Source { get; set; }
    }
}