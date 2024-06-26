﻿namespace PrjFunNowWebApi.Models.joannaDTO
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
            public int MemberId { get; set; } // 新增 MemberId
            public string FirstName { get; set; } // 添加新屬性
            public string TravelerType { get; set; } // 添加新屬性
            public string RoomTypeName { get; set; } // 添加新屬性

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
        public class ReportReviewFilter
        {
            public int? ReportTitleId { get; set; }
            public int? ReportSubtitleId { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public string SearchText { get; set; }
            public List<int> ReviewStatuses { get; set; }
        }
        public class MemberCommentDTO
        {
            public int hotelId { get; set; }
            public string hotelAddress { get; set; }
            public string roomtypename { get; set; }
            public DateTime? cheeckInDate { get; set; }
            public DateTime? cheeckOutDate { get; set; }
           
        }

        public class CommentRequest
        {
            public string CommentTitle { get; set; }
            public string CommentText { get; set; }
            public int RoomID { get; set; }
            public int ComfortScore { get; set; }
            public int CleankinessScore { get; set; }
            public int StaffScore { get; set; }
            public int FacilitiesScore { get; set; }
            public int ValueScore { get; set; }
            public int LocationScore { get; set; }
            public int FreeWifiScore { get; set; }
            public string TravelerType { get; set; }
        }

        public class AddComment
        {
            public int CommentID { get; set; }
            public string CommentTitle { get; set; }
            public string CommentText { get; set; }
            public DateTime UpdatedAt { get; set; }
            public int CommentStatus { get; set; }
        }


       













    }

}
