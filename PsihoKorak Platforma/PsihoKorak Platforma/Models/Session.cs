﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace PsihoKorak_Platforma.Models;

public partial class Session
{
    public int SessionId { get; set; }

    public DateTime DateTime { get; set; }

    public TimeSpan Duration { get; set; }

    public int SessionTypeId { get; set; }

    public virtual ICollection<Help> Helps { get; set; } = new List<Help>();

    public virtual SessionType SessionType { get; set; }
}