namespace PrjFunNowWebApi.Models.joannaDTO
{
    public class CommentResponseDTO
    {
        public class CommentResponse
        {
            public int CommentId { get; set; }
            public int HotelId { get; set; }
            public string CommentTitle { get; set; }
            public string CommentText { get; set; }
            public DateTime CreatedAt { get; set; }
            public List<RatingScoreDTO> RatingScores { get; set; }
        }

        public class CommentStatisticsDTO
        {
            public int TotalItems { get; set; }
            public decimal TotalAverageScore { get; set; }
            public AverageScoresDTO AverageScores { get; set; }
            public string HotelName { get; set; }
            public int SuperPositiveCount { get; set; }
            public int VeryPositiveCount { get; set; }
            public int GoodCount { get; set; }
            public int AverageCount { get; set; }
            public int BelowExpectationCount { get; set; }
        }

        public class AverageScoresDTO
        {
            public decimal ComfortScore { get; set; }
            public decimal CleanlinessScore { get; set; }
            public decimal StaffScore { get; set; }
            public decimal FacilitiesScore { get; set; }
            public decimal ValueScore { get; set; }
            public decimal LocationScore { get; set; }
            public decimal FreeWifiScore { get; set; }
        }
    }
}
