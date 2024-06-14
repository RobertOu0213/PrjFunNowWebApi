﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace PrjFunNowWebApi.Models;

public partial class Comment
{
    public int CommentId { get; set; }

    public int HotelId { get; set; }

    public int MemberId { get; set; }

    public DateTime CreatedAt { get; set; }

    public string CommentTitle { get; set; }

    public string CommentText { get; set; }

    public string CommentStatus { get; set; }

    public bool IsReported { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? OrderId { get; set; }

    public int? RoomId { get; set; }

    public int? OrderId { get; set; }

    public int? RoomId { get; set; }

    public int? RoomId { get; set; }

    public virtual ICollection<CommentTravelerType> CommentTravelerTypes { get; set; } = new List<CommentTravelerType>();

    public virtual Hotel Hotel { get; set; }

    public virtual Member Member { get; set; }

    public virtual ICollection<RatingScore> RatingScores { get; set; } = new List<RatingScore>();

    public virtual ICollection<ReportReview> ReportReviews { get; set; } = new List<ReportReview>();
}