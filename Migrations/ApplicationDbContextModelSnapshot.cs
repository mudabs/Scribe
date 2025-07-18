﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Scribe.Data;

#nullable disable

namespace Scribe.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Scribe.Models.ADUsers", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("ADUsers");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Name = "No User"
                        });
                });

            modelBuilder.Entity("Scribe.Models.AllocationHistory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("ADUsersId")
                        .HasColumnType("int");

                    b.Property<string>("AllocatedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("AllocationDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("DeallocatedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("DeallocationDate")
                        .HasColumnType("datetime2");

                    b.Property<int?>("GroupId")
                        .HasColumnType("int");

                    b.Property<int>("SerialNumberId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ADUsersId");

                    b.HasIndex("GroupId");

                    b.HasIndex("SerialNumberId");

                    b.ToTable("AllocationHistory");
                });

            modelBuilder.Entity("Scribe.Models.Brand", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ImageName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Brands");
                });

            modelBuilder.Entity("Scribe.Models.Category", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Icon")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("Scribe.Models.Condition", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ColorCode")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Condition");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            ColorCode = "green",
                            Name = "New"
                        },
                        new
                        {
                            Id = 2,
                            ColorCode = "yellow",
                            Name = "Needs Repairs"
                        },
                        new
                        {
                            Id = 3,
                            ColorCode = "blue",
                            Name = "In Use"
                        },
                        new
                        {
                            Id = 4,
                            ColorCode = "red",
                            Name = "Out Of Order"
                        },
                        new
                        {
                            Id = 5,
                            ColorCode = "grey",
                            Name = "Awaiting User"
                        });
                });

            modelBuilder.Entity("Scribe.Models.Department", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Department");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Name = "IT"
                        },
                        new
                        {
                            Id = 2,
                            Name = "No Department"
                        });
                });

            modelBuilder.Entity("Scribe.Models.Group", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Group");
                });

            modelBuilder.Entity("Scribe.Models.IndividualAssignment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("ADUsersId")
                        .HasColumnType("int");

                    b.Property<int>("SerialNumberId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ADUsersId");

                    b.HasIndex("SerialNumberId");

                    b.ToTable("IndividualAssignment");
                });

            modelBuilder.Entity("Scribe.Models.Location", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Locations");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Description = "IT Storage room",
                            Name = "New Storage"
                        },
                        new
                        {
                            Id = 2,
                            Description = "IT Storage room",
                            Name = "Old Storage"
                        },
                        new
                        {
                            Id = 3,
                            Description = "User Office or Station",
                            Name = "User Station"
                        });
                });

            modelBuilder.Entity("Scribe.Models.Log", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<string>("Details")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("User")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Log");
                });

            modelBuilder.Entity("Scribe.Models.Maintenance", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("ConditionId")
                        .HasColumnType("int");

                    b.Property<DateTime>("NextServiceDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("SerialNumberId")
                        .HasColumnType("int");

                    b.Property<DateTime>("ServiceDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("ServiceDescription")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SystemUserId")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("ConditionId");

                    b.HasIndex("SerialNumberId");

                    b.ToTable("Maintenances");
                });

            modelBuilder.Entity("Scribe.Models.Model", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("BrandId")
                        .HasColumnType("int");

                    b.Property<int?>("CategoryId")
                        .HasColumnType("int");

                    b.Property<string>("Image")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("BrandId");

                    b.HasIndex("CategoryId");

                    b.ToTable("Models");
                });

            modelBuilder.Entity("Scribe.Models.SerialNumber", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("ADUsersId")
                        .HasColumnType("int");

                    b.Property<string>("AllocatedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("Allocation")
                        .HasColumnType("datetime2");

                    b.Property<int?>("ConditionId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("Creation")
                        .HasColumnType("datetime2");

                    b.Property<bool>("CurrentlyAllocated")
                        .HasColumnType("bit");

                    b.Property<string>("DeallocatedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("DepartmentId")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("GroupId")
                        .HasColumnType("int");

                    b.Property<int?>("LocationId")
                        .HasColumnType("int");

                    b.Property<string>("MacAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("ModelId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("SerialNumberId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ADUsersId");

                    b.HasIndex("ConditionId");

                    b.HasIndex("DepartmentId");

                    b.HasIndex("GroupId");

                    b.HasIndex("LocationId");

                    b.HasIndex("ModelId");

                    b.HasIndex("SerialNumberId");

                    b.ToTable("SerialNumbers");
                });

            modelBuilder.Entity("Scribe.Models.SerialNumberGroup", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("ADUsersId")
                        .HasColumnType("int");

                    b.Property<int?>("GroupId")
                        .HasColumnType("int");

                    b.Property<int>("SerialNumberId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ADUsersId");

                    b.HasIndex("GroupId");

                    b.HasIndex("SerialNumberId");

                    b.ToTable("SerialNumberGroup");
                });

            modelBuilder.Entity("Scribe.Models.SystemUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("DisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FirstName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LastName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SamAccountName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserPrincipalName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("SystemUsers");
                });

            modelBuilder.Entity("Scribe.Models.UserGroup", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("GroupId")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("GroupId");

                    b.HasIndex("UserId");

                    b.ToTable("UserGroup");
                });

            modelBuilder.Entity("Scribe.Models.Warranty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("ModelId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("PurchaseDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("WarrantyDurationYears")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ModelId");

                    b.ToTable("Warranties");
                });

            modelBuilder.Entity("Scribe.Models.AllocationHistory", b =>
                {
                    b.HasOne("Scribe.Models.ADUsers", "ADUsers")
                        .WithMany()
                        .HasForeignKey("ADUsersId");

                    b.HasOne("Scribe.Models.Group", "Group")
                        .WithMany()
                        .HasForeignKey("GroupId");

                    b.HasOne("Scribe.Models.SerialNumber", "SerialNumber")
                        .WithMany()
                        .HasForeignKey("SerialNumberId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ADUsers");

                    b.Navigation("Group");

                    b.Navigation("SerialNumber");
                });

            modelBuilder.Entity("Scribe.Models.IndividualAssignment", b =>
                {
                    b.HasOne("Scribe.Models.ADUsers", "ADUsers")
                        .WithMany()
                        .HasForeignKey("ADUsersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Scribe.Models.SerialNumber", "SerialNumber")
                        .WithMany()
                        .HasForeignKey("SerialNumberId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ADUsers");

                    b.Navigation("SerialNumber");
                });

            modelBuilder.Entity("Scribe.Models.Maintenance", b =>
                {
                    b.HasOne("Scribe.Models.Condition", "Condition")
                        .WithMany()
                        .HasForeignKey("ConditionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Scribe.Models.SerialNumber", "SerialNumber")
                        .WithMany()
                        .HasForeignKey("SerialNumberId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Condition");

                    b.Navigation("SerialNumber");
                });

            modelBuilder.Entity("Scribe.Models.Model", b =>
                {
                    b.HasOne("Scribe.Models.Brand", "Brand")
                        .WithMany()
                        .HasForeignKey("BrandId");

                    b.HasOne("Scribe.Models.Category", "Category")
                        .WithMany()
                        .HasForeignKey("CategoryId");

                    b.Navigation("Brand");

                    b.Navigation("Category");
                });

            modelBuilder.Entity("Scribe.Models.SerialNumber", b =>
                {
                    b.HasOne("Scribe.Models.ADUsers", "ADUsers")
                        .WithMany("SerialNumbers")
                        .HasForeignKey("ADUsersId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("Scribe.Models.Condition", "Condition")
                        .WithMany()
                        .HasForeignKey("ConditionId");

                    b.HasOne("Scribe.Models.Department", "Department")
                        .WithMany()
                        .HasForeignKey("DepartmentId");

                    b.HasOne("Scribe.Models.Group", "Group")
                        .WithMany()
                        .HasForeignKey("GroupId");

                    b.HasOne("Scribe.Models.Location", "Location")
                        .WithMany()
                        .HasForeignKey("LocationId");

                    b.HasOne("Scribe.Models.Model", "Model")
                        .WithMany("SerialNumbers")
                        .HasForeignKey("ModelId");

                    b.HasOne("Scribe.Models.SerialNumber", null)
                        .WithMany("SerialNumbers")
                        .HasForeignKey("SerialNumberId");

                    b.Navigation("ADUsers");

                    b.Navigation("Condition");

                    b.Navigation("Department");

                    b.Navigation("Group");

                    b.Navigation("Location");

                    b.Navigation("Model");
                });

            modelBuilder.Entity("Scribe.Models.SerialNumberGroup", b =>
                {
                    b.HasOne("Scribe.Models.ADUsers", "ADUsers")
                        .WithMany()
                        .HasForeignKey("ADUsersId");

                    b.HasOne("Scribe.Models.Group", "Group")
                        .WithMany("SerialNumberGroups")
                        .HasForeignKey("GroupId");

                    b.HasOne("Scribe.Models.SerialNumber", "SerialNumber")
                        .WithMany()
                        .HasForeignKey("SerialNumberId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ADUsers");

                    b.Navigation("Group");

                    b.Navigation("SerialNumber");
                });

            modelBuilder.Entity("Scribe.Models.UserGroup", b =>
                {
                    b.HasOne("Scribe.Models.Group", "Group")
                        .WithMany("UserGroups")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Scribe.Models.ADUsers", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Group");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Scribe.Models.Warranty", b =>
                {
                    b.HasOne("Scribe.Models.Model", "Model")
                        .WithMany()
                        .HasForeignKey("ModelId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Model");
                });

            modelBuilder.Entity("Scribe.Models.ADUsers", b =>
                {
                    b.Navigation("SerialNumbers");
                });

            modelBuilder.Entity("Scribe.Models.Group", b =>
                {
                    b.Navigation("SerialNumberGroups");

                    b.Navigation("UserGroups");
                });

            modelBuilder.Entity("Scribe.Models.Model", b =>
                {
                    b.Navigation("SerialNumbers");
                });

            modelBuilder.Entity("Scribe.Models.SerialNumber", b =>
                {
                    b.Navigation("SerialNumbers");
                });
#pragma warning restore 612, 618
        }
    }
}
