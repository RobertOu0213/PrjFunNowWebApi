﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace PrjFunNowWebApi.Models;

public partial class Coupon
{
    public int CouponId { get; set; }

    public string CouponName { get; set; }

    public string CouponDescription { get; set; }

    public decimal Discount { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<PaymentStatus> PaymentStatuses { get; set; } = new List<PaymentStatus>();
}