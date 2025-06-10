namespace simple_rank_backend.DTOs.Ranking.Responses
{
    public class ShareableLinkResponse
    {
        public string ShareableId { get; set; }
        
        public ShareableLinkResponse(string link) 
        {
            ShareableId = link;        
        }
    }
}
