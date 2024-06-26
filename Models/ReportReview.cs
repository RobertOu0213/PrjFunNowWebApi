﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace PrjFunNowWebApi.Models;

public partial class ReportReview
{
    public int ReportId { get; set; }

    public int CommentId { get; set; }

    public int MemberId { get; set; }

    public int ReportTitleId { get; set; }

    public int ReportSubtitleId { get; set; }

    public DateTime ReportedAt { get; set; }

    public string ReportReason { get; set; }

    public string ReviewStatus { get; set; }

    public int? ReviewedBy { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public virtual Comment Comment { get; set; }

    public virtual Member Member { get; set; }

    public virtual ReportSubtitle ReportSubtitle { get; set; }

    public virtual ReportTitle ReportTitle { get; set; }
}