﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace PsihoKorak_Platforma.Models;

public partial class MedicationType
{
    public int MedicationTypeId { get; set; }

    public string MedicationTypeName { get; set; }

    public virtual ICollection<Medication> Medications { get; set; } = new List<Medication>();
}