﻿// <auto-generated />
using System;
using HelixAPI.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace HelixAPI.Migrations
{
    [DbContext(typeof(HelixDBContext))]
    [Migration("20231210182130_initialsetup")]
    partial class initialsetup
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("HelixAPI.Model.Creator", b =>
                {
                    b.Property<Guid>("ID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("CreatorID")
                        .HasColumnType("int");

                    b.Property<string>("Firstname")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Lastname")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SortName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ID", "CreatorID");

                    b.ToTable("Creators");
                });

            modelBuilder.Entity("HelixAPI.Model.Entity", b =>
                {
                    b.Property<Guid>("ID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("EntityID")
                        .HasColumnType("int");

                    b.Property<int>("Category")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ID", "EntityID");

                    b.ToTable("Entities");
                });

            modelBuilder.Entity("HelixAPI.Model.EntityRelationship", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("Entity1ID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("Entity2ID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Relationship")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.ToTable("EntityRelationships");
                });

            modelBuilder.Entity("HelixAPI.Model.Index", b =>
                {
                    b.Property<Guid>("ID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("IndexID")
                        .HasColumnType("int");

                    b.Property<Guid>("EntityID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("IndexedByID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Location")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("SourceID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Subject")
                        .HasColumnType("int");

                    b.HasKey("ID", "IndexID");

                    b.ToTable("Indexs");
                });

            modelBuilder.Entity("HelixAPI.Model.Source", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Branch")
                        .HasColumnType("int");

                    b.Property<int>("ContentType")
                        .HasColumnType("int");

                    b.Property<Guid>("CreatorID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Flags")
                        .HasColumnType("int");

                    b.Property<int>("Format")
                        .HasColumnType("int");

                    b.Property<DateOnly>("PublicationDate")
                        .HasColumnType("date");

                    b.Property<string>("Publisher")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("URL")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ID");

                    b.ToTable("Sources");
                });

            modelBuilder.Entity("HelixAPI.Model.User", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("Active")
                        .HasColumnType("bit");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ID");

                    b.ToTable("Users");
                });
#pragma warning restore 612, 618
        }
    }
}