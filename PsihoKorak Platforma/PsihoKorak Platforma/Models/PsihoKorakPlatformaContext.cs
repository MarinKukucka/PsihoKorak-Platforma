﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PsihoKorak_Platforma.Models;

public partial class PsihoKorakPlatformaContext : DbContext
{
    public PsihoKorakPlatformaContext()
    {

    }

    public PsihoKorakPlatformaContext(DbContextOptions<PsihoKorakPlatformaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Help> Helps { get; set; }

    public virtual DbSet<Medication> Medications { get; set; }

    public virtual DbSet<MedicationType> MedicationTypes { get; set; }

    public virtual DbSet<Patient> Patients { get; set; }

    public virtual DbSet<Psychologist> Psychologists { get; set; }

    public virtual DbSet<Record> Records { get; set; }

    public virtual DbSet<Session> Sessions { get; set; }

    public virtual DbSet<SessionType> SessionTypes { get; set; }

    public virtual DbSet<Use> Uses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Help>(entity =>
        {
            entity.HasKey(e => e.HelpsId);

            entity.Property(e => e.Note).IsUnicode(false);

            entity.HasOne(d => d.Patient).WithMany(p => p.Helps)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Helps_Patient");

            entity.HasOne(d => d.Psychologist).WithMany(p => p.Helps)
                .HasForeignKey(d => d.PsychologistId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Helps_Psychologist");

            entity.HasOne(d => d.Session).WithMany(p => p.Helps)
                .HasForeignKey(d => d.SessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Helps_Session");
        });

        modelBuilder.Entity<Medication>(entity =>
        {
            entity.ToTable("Medication");

            entity.Property(e => e.MedicationName)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.MedicationType).WithMany(p => p.Medications)
                .HasForeignKey(d => d.MedicationTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Medication_MedicationType");
        });

        modelBuilder.Entity<MedicationType>(entity =>
        {
            entity.ToTable("MedicationType");

            entity.Property(e => e.MedicationTypeName)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.ToTable("Patient");

            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PhoneNumber)
                .IsRequired()
                .HasMaxLength(15)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Psychologist>(entity =>
        {
            entity.ToTable("Psychologist");

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.HashedPassword)
                .IsRequired()
                .IsUnicode(false);
            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PasswordSalt)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Record>(entity =>
        {
            entity.ToTable("Record");

            entity.Property(e => e.DateTime).HasColumnType("datetime");
            entity.Property(e => e.RecordText)
                .IsRequired()
                .IsUnicode(false);

            entity.HasOne(d => d.Patient).WithMany(p => p.Records)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Record_Patient");
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.ToTable("Session");

            entity.Property(e => e.DateTime).HasColumnType("datetime");

            entity.HasOne(d => d.SessionType).WithMany(p => p.Sessions)
                .HasForeignKey(d => d.SessionTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Session_SessionType");
        });

        modelBuilder.Entity<SessionType>(entity =>
        {
            entity.ToTable("SessionType");

            entity.Property(e => e.SessionTypeName)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Use>(entity =>
        {
            entity.HasKey(e => e.UsesId);

            entity.Property(e => e.Dose)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Medication).WithMany(p => p.Uses)
                .HasForeignKey(d => d.MedicationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Uses_Medication");

            entity.HasOne(d => d.Patient).WithMany(p => p.Uses)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Uses_Patient");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}