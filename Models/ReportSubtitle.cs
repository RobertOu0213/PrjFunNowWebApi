﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace PrjFunNowWebApi.Models;

public partial class ReportSubtitle
{
    public int ReportSubtitleId { get; set; }

    public string ReportSubtitle1 { get; set; }

    public int ReportTitleId { get; set; }

    public virtual ICollection<ReportReview> ReportReviews { get; set; } = new List<ReportReview>();

    public virtual ReportTitle ReportTitle { get; set; }
}