using Nest;

namespace Training.Models
{
    [ElasticsearchType(RelationName = "training--logging")]
    public class Log
    {
        [Text(Name = "level")]
        public string Level { get; set; } 
        [Text(Name = "message")]
        public string Message { get; set; }
    }
}