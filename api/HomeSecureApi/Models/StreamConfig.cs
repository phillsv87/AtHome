namespace HomeSecureApi.Models
{
    public class StreamConfig
    {
        public int Id{get;set;}

        public string Name{get;set;}

        public string Uri{get;set;}

        public string Tag{get;set;}


        public StreamInfo GetInfo()
        {
            return new StreamInfo()
            {
                Id=Id,
                Name=Name,
                Tag=Tag
            };
        }
    }
}