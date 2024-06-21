﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace PrjFunNowWebApi.Models;

public partial class OrderDetail
{
    public int OrderDetailId { get; set; }

    public int MemberId { get; set; }

    public int RoomId { get; set; }

    public DateTime CheckInDate { get; set; }

    public DateTime CheckOutDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool IsOrdered { get; set; }

    public int? OrderId { get; set; }

    public int? GuestNumber { get; set; }

    public virtual Member Member { get; set; }

    public virtual Order Order { get; set; }

    public virtual Room Room { get; set; }
}