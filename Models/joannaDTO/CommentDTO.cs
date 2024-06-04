﻿namespace PrjFunNowWebApi.Models.joannaDTO
{
    public class CommentDTO
    {
        //public int Page { get; set; } = 1;
        //public int PageSize { get; set; } = 10;
        //public string Search { get; set; } = null;
        //public int? RatingFilter { get; set; } = null;
        //public string DateFilter { get; set; } = null;

        public int CommentId { get; set; }
        public int HotelId { get; set; }
        public string CommentTitle { get; set; }
        public string CommentText { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<RatingScoreDTO> RatingScores { get; set; }
    }

    public class RatingScoreDTO
    {
        public int RatingId { get; set; }
        public int CommentId { get; set; }
        public decimal ComfortScore { get; set; }
        public decimal CleanlinessScore { get; set; }
        public decimal StaffScore { get; set; }
        public decimal FacilitiesScore { get; set; }
        public decimal ValueScore { get; set; }
        public decimal LocationScore { get; set; }
        public decimal FreeWifiScore { get; set; }
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

    public class CommentStatisticsDTO
    {
        public int TotalItems { get; set; }
        public List<CommentDTO> Comments { get; set; }
        public decimal TotalAverageScore { get; set; }
        public decimal AverageScore { get; set; }
        public string HotelName { get; set; }
        public int SuperPositiveCount { get; set; }
        public int VeryPositiveCount { get; set; }
        public int GoodCount { get; set; }
        public int AverageCount { get; set; }
        public int BelowExpectationCount { get; set; }
    }
}
