﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MisFinanzas.Domain.Entities;
using MisFinanzas.Domain.Enums;

namespace MisFinanzas.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets para nuestras entidades
        public DbSet<Category> Categories { get; set; }
        public DbSet<ExpenseIncome> ExpensesIncomes { get; set; }
        public DbSet<FinancialGoal> FinancialGoals { get; set; }
        public DbSet<Budget> Budgets { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ====== RENOMBRAR TABLAS DE IDENTITY ======
            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<IdentityRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");

            // ====== CONFIGURAR APPLICATIONUSER ======
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(u => u.FullName)
                    .HasMaxLength(200);

                entity.Property(u => u.UserRole)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasDefaultValue("User");

                entity.Property(u => u.IsActive)         
                    .IsRequired()
                    .HasDefaultValue(true);

                // Índice para búsquedas por rol
                entity.HasIndex(u => u.UserRole);

                // Índice para búsquedas por estado
                entity.HasIndex(u => u.IsActive);

                // Ignorar campos de Identity que no usamos
                entity.Ignore(u => u.PhoneNumber);
                entity.Ignore(u => u.PhoneNumberConfirmed);
                entity.Ignore(u => u.TwoFactorEnabled);
                entity.Ignore(u => u.LockoutEnd);
                entity.Ignore(u => u.LockoutEnabled);
                entity.Ignore(u => u.AccessFailedCount);
            });

            // ====== CONFIGURACIÓN DE CATEGORIES ======
            builder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.CategoryId);

                entity.Property(c => c.Title)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(c => c.Icon)
                    .HasMaxLength(10)
                    .HasDefaultValue("📁");

                // Relación con ApplicationUser
                entity.HasOne(c => c.User)
                    .WithMany(u => u.Categories)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Índices
                entity.HasIndex(c => c.UserId);
                entity.HasIndex(c => new { c.UserId, c.Title });
            });

            // ====== CONFIGURACIÓN DE EXPENSESINCOME ======
            builder.Entity<ExpenseIncome>(entity =>
            {
                entity.HasKey(ei => ei.Id);

                entity.Property(ei => ei.Amount)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(ei => ei.Description)
                    .HasMaxLength(500);

                entity.Property(ei => ei.Date)
                    .HasDefaultValueSql("datetime('now')");

                // Relación con ApplicationUser
                entity.HasOne(ei => ei.User)
                    .WithMany(u => u.ExpensesIncomes)
                    .HasForeignKey(ei => ei.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Relación con Category
                entity.HasOne(ei => ei.Category)
                    .WithMany(c => c.ExpensesIncomes)
                    .HasForeignKey(ei => ei.CategoryId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Índices
                entity.HasIndex(ei => ei.UserId);
                entity.HasIndex(ei => ei.Date);
                entity.HasIndex(ei => new { ei.UserId, ei.Date });
                entity.HasIndex(ei => new { ei.UserId, ei.Type });

                // Validación: Amount debe ser positivo
                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_ExpenseIncome_Amount", "Amount > 0"));
            });

            // ====== CONFIGURACIÓN DE FINANCIAL GOALS ======
            builder.Entity<FinancialGoal>(entity =>
            {
                entity.HasKey(g => g.GoalId);

                entity.Property(g => g.Title)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(g => g.Description)
                    .HasMaxLength(500);

                entity.Property(g => g.Icon)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasDefaultValue("🎯");

                entity.Property(g => g.TargetAmount)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(g => g.CurrentAmount)
                    .HasColumnType("decimal(18,2)")
                    .HasDefaultValue(0);

                entity.Property(g => g.StartDate)
                    .HasDefaultValueSql("datetime('now')");

                entity.Property(g => g.TargetDate)
                    .IsRequired();

                entity.Property(g => g.Status)
                    .IsRequired();
                   

                entity.Property(g => g.CompletedAt)
                    .IsRequired(false);

                // Relación con ApplicationUser
                entity.HasOne(g => g.User)
                    .WithMany(u => u.FinancialGoals)
                    .HasForeignKey(g => g.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Índices
                entity.HasIndex(g => g.UserId);
                entity.HasIndex(g => g.TargetDate);
                entity.HasIndex(g => new { g.UserId, g.Status });

                // Validaciones
                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_Goal_TargetAmount", "TargetAmount > 0"));
                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_Goal_CurrentAmount", "CurrentAmount >= 0"));
                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_Goal_Dates", "TargetDate >= StartDate"));
            });

            // ====== CONFIGURACIÓN DE BUDGETS (NUEVO) ======
            builder.Entity<Budget>(entity =>
            {
                entity.HasKey(b => b.Id);

                entity.Property(b => b.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(b => b.AssignedAmount)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(b => b.SpentAmount)
                    .HasColumnType("decimal(18,2)")
                    .HasDefaultValue(0);

                entity.Property(b => b.IsActive)
                    .HasDefaultValue(true);

                // Relación con ApplicationUser
                entity.HasOne(b => b.User)
                    .WithMany(u => u.Budgets)
                    .HasForeignKey(b => b.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Relación REQUERIDA con Category
                entity.HasOne(b => b.Category)
                    .WithMany(c => c.Budgets)
                    .HasForeignKey(b => b.CategoryId)
                    .OnDelete(DeleteBehavior.Cascade); // Si se elimina categoría, elimina presupuestos

                // Índices
                entity.HasIndex(b => b.UserId);
                entity.HasIndex(b => new { b.UserId, b.Month, b.Year });
                entity.HasIndex(b => new { b.UserId, b.IsActive });
                entity.HasIndex(b => new { b.UserId, b.CategoryId, b.Month, b.Year });

                // Validaciones
                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_Budget_AssignedAmount", "AssignedAmount > 0"));
                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_Budget_SpentAmount", "SpentAmount >= 0"));
                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_Budget_Month", "Month BETWEEN 1 AND 12"));
                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_Budget_Year", "Year >= 2020"));

                // Ignorar propiedades calculadas (no se mapean a BD)
                entity.Ignore(b => b.AvailableAmount);
                entity.Ignore(b => b.UsedPercentage);
                entity.Ignore(b => b.IsOverBudget);
                entity.Ignore(b => b.IsNearLimit);
            });

            // ====== SEED DATA: ROLES Y USUARIO ADMIN ======

            // 1. Crear rol Admin
            var adminRoleId = "550e8400-e29b-41d4-a716-446655440000";
            builder.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Id = adminRoleId,
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    ConcurrencyStamp = adminRoleId
                }
            );

            // 2. Crear rol User
            var userRoleId = "550e8400-e29b-41d4-a716-446655440001";
            builder.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Id = userRoleId,
                    Name = "User",
                    NormalizedName = "USER",
                    ConcurrencyStamp = userRoleId
                }
            );

            // 3. Crear usuario Admin
            var adminUserId = "admin-550e8400-e29b-41d4-a716-446655440000";
            builder.Entity<ApplicationUser>().HasData(
                new ApplicationUser
                {
                    Id = adminUserId,
                    UserName = "admin",
                    NormalizedUserName = "ADMIN",
                    Email = "admin@misfinanzas.com",
                    NormalizedEmail = "ADMIN@MISFINANZAS.COM",
                    EmailConfirmed = true,
                    PasswordHash = "Admin123", // ⚠️ Texto plano (con custom hasher)
                    SecurityStamp = Guid.NewGuid().ToString(),
                    FullName = "Administrador del Sistema",
                    UserRole = "Admin",
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // 4. Asignar rol Admin al usuario Admin
            builder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>
                {
                    RoleId = adminRoleId,
                    UserId = adminUserId
                }
            );
        }
    }
}