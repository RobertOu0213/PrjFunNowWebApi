namespace PrjFunNowWebApi.Models.joannaDTO
{
    public class CommentRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string Search { get; set; } = null;
        public int? RatingFilter { get; set; } = null;
        public string DateFilter { get; set; } = null;
        public int HotelId { get; set; }
    }

    public class RatingScoreDTO
    {
        public int CommentId { get; set; }
        public int RatingId { get; set; }
        public decimal ComfortScore { get; set; }
        public decimal CleanlinessScore { get; set; }
        public decimal StaffScore { get; set; }
        public decimal FacilitiesScore { get; set; }
        public decimal ValueScore { get; set; }
        public decimal LocationScore { get; set; }
        public decimal FreeWifiScore { get; set; }
        public string TravelerType { get; set; }
    }


   

}
