﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace PrjFunNowWebApi.Models;

public partial class RoomEquipment
{
    public int RoomEquipmentId { get; set; }

    public string RoomEquipmentName { get; set; }

    public virtual ICollection<RoomEquipmentReference> RoomEquipmentReferences { get; set; } = new List<RoomEquipmentReference>();
}