﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace PrjFunNowWebApi.Models;

public partial class City
{
    public int CityId { get; set; }

    public string CityName { get; set; }

    public int CountryId { get; set; }

    public virtual Country Country { get; set; }

    public virtual ICollection<Hotel> Hotels { get; set; } = new List<Hotel>();
}