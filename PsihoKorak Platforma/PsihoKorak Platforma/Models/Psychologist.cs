﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace PsihoKorak_Platforma.Models;

public partial class Psychologist
{
    public int PsychologistId { get; set; }

    public string Email { get; set; }

    public string HashedPassword { get; set; }

    public string PasswordSalt { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }
}