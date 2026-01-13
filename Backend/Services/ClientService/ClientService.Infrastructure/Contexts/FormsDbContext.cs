using BaseService.Domain.Entities;
using BaseService.Infrastructure.Contexts;
using ClientService.Domain.Entities;
using ClientService.Domain.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore.Models;

namespace ClientService.Infrastructure.Contexts;

public class FormsDbContext : AppDbContext
{
    public FormsDbContext(DbContextOptions<FormsDbContext> options) : base(options)
    {
    }

    // Base entities
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;

    // OpenIddict entities
    public DbSet<OpenIddictEntityFrameworkCoreApplication<Guid>> OpenIddictApplications { get; set; } = null!;
    public DbSet<OpenIddictEntityFrameworkCoreAuthorization<Guid>> OpenIddictAuthorizations { get; set; } = null!;
    public DbSet<OpenIddictEntityFrameworkCoreScope<Guid>> OpenIddictScopes { get; set; } = null!;
    public DbSet<OpenIddictEntityFrameworkCoreToken<Guid>> OpenIddictTokens { get; set; } = null!;

    // Forms entities
    public DbSet<Form> Forms { get; set; } = null!;
    public DbSet<Field> Fields { get; set; } = null!;
    public DbSet<Logic> Logic { get; set; } = null!;
    public DbSet<Submission> Submissions { get; set; } = null!;
    public DbSet<Answer> Answers { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // IMPORTANT:
        // Keep table names consistent with existing schema/migrations ("User", "Role").
        // Without this, EF may infer "Users"/"Roles" from DbSet names and generate rename migrations.
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");
            entity.HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId);
            entity.HasData(
                new User
                {
                    Id = Guid.Parse("3538ed8c-b05d-4b58-80ff-2868aba45701"),
                    RoleId = Guid.Parse("019923ff-5bd5-7622-b805-c53c2525ba21"),
                    Email = "user@example.com",
                    Name = "Nguyen Van A",
                    Password = "$2a$12$hgon6fKIh8fKF9NgjefgX.rrDTSyigpvPQ9ZD5hExZJjqXrKbkqAW",
                    CreatedAt = new DateTime(2026, 1, 10, 18, 46, 7, 462, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2026, 1, 10, 18, 46, 7, 463, DateTimeKind.Utc),
                    CreatedBy = "System",
                    UpdatedBy = "System",
                    IsActive = true
                }
            );
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Role");
            entity.HasData(
                new Role
                {
                    Id = Guid.Parse("019923ff-5bd5-7622-b805-c53c2525ba21"),
                    Name = "user",
                    NormalizedName = "USER",
                    IsActive = true
                }
            );
        });

        // Configure OpenIddict entities - OpenIddict will configure these automatically
        // when UseOpenIddict<Guid>() is called in DbContextOptions configuration

        // Configure Form entity
        modelBuilder.Entity<Form>(entity =>
        {
            entity.ToTable("forms");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasColumnType("uuid");
            entity.Property(e => e.UserId).HasColumnName("user_id").HasColumnType("uuid").IsRequired();
            entity.Property(e => e.Title).HasColumnName("title").HasColumnType("varchar").IsRequired();
            entity.Property(e => e.Slug).HasColumnName("slug").HasColumnType("varchar");
            entity.Property(e => e.ThemeConfig).HasColumnName("theme_config").HasColumnType("jsonb");
            entity.Property(e => e.Settings).HasColumnName("settings").HasColumnType("jsonb");
            entity.Property(e => e.IsPublished).HasColumnName("is_published");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasColumnType("varchar");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasColumnType("varchar");
            entity.Property(e => e.IsActive).HasColumnName("is_active");

            // Relationship: Form -> User (from BaseService)
            entity.HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed data for Form
            var sampleFormId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var sampleUserId = Guid.Parse("3538ed8c-b05d-4b58-80ff-2868aba45701");
            var seedDateTime = new DateTime(2026, 1, 11, 1, 46, 7);
            entity.HasData(
                new Form
                {
                    Id = sampleFormId,
                    UserId = sampleUserId,
                    Title = "Sample Form",
                    Slug = "sample-form",
                    IsPublished = true,
                    CreatedAt = seedDateTime,
                    UpdatedAt = seedDateTime,
                    CreatedBy = "System",
                    UpdatedBy = "System",
                    IsActive = true
                }
            );
        });

        // Configure Field entity
        modelBuilder.Entity<Field>(entity =>
        {
            entity.ToTable("fields");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasColumnType("uuid");
            entity.Property(e => e.FormId).HasColumnName("form_id").HasColumnType("uuid").IsRequired();
            entity.Property(e => e.Title).HasColumnName("title").HasColumnType("varchar").IsRequired();
            entity.Property(e => e.Description).HasColumnName("description").HasColumnType("varchar");
            entity.Property(e => e.Type).HasColumnName("type").HasConversion<string>().IsRequired();
            entity.Property(e => e.Properties).HasColumnName("properties").HasColumnType("jsonb");
            entity.Property(e => e.IsRequired).HasColumnName("is_required");
            entity.Property(e => e.Order).HasColumnName("\"order\""); // Escape order as it's a reserved keyword
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasColumnType("varchar");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasColumnType("varchar");
            entity.Property(e => e.IsActive).HasColumnName("is_active");

            // Relationship: Field -> Form
            entity.HasOne(f => f.Form)
                .WithMany(form => form.Fields)
                .HasForeignKey(f => f.FormId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed data for Field
            var sampleFormId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var sampleFieldId1 = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var sampleFieldId2 = Guid.Parse("22222222-2222-2222-2222-222222222223");
            var seedDateTime = new DateTime(2026, 1, 11, 1, 46, 7);
            entity.HasData(
                new Field
                {
                    Id = sampleFieldId1,
                    FormId = sampleFormId,
                    Title = "Full Name",
                    Description = "Please enter your full name",
                    Type = FieldType.Text,
                    IsRequired = true,
                    Order = 1,
                    CreatedAt = seedDateTime,
                    UpdatedAt = seedDateTime,
                    CreatedBy = "System",
                    UpdatedBy = "System",
                    IsActive = true
                },
                new Field
                {
                    Id = sampleFieldId2,
                    FormId = sampleFormId,
                    Title = "Email Address",
                    Description = "Please enter your email",
                    Type = FieldType.Email,
                    IsRequired = true,
                    Order = 2,
                    CreatedAt = seedDateTime,
                    UpdatedAt = seedDateTime,
                    CreatedBy = "System",
                    UpdatedBy = "System",
                    IsActive = true
                }
            );
        });

        // Configure Logic entity
        modelBuilder.Entity<Logic>(entity =>
        {
            entity.ToTable("logic");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasColumnType("uuid");
            entity.Property(e => e.FieldId).HasColumnName("field_id").HasColumnType("uuid").IsRequired();
            entity.Property(e => e.Condition).HasColumnName("condition").HasColumnType("varchar").IsRequired();
            entity.Property(e => e.Value).HasColumnName("value").HasColumnType("varchar");
            entity.Property(e => e.DestinationFieldId).HasColumnName("destination_field_id").HasColumnType("uuid");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasColumnType("varchar");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasColumnType("varchar");
            entity.Property(e => e.IsActive).HasColumnName("is_active");

            // Relationship: Logic -> Field (source)
            entity.HasOne(l => l.Field)
                .WithMany(f => f.LogicRules)
                .HasForeignKey(l => l.FieldId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship: Logic -> Field (destination)
            entity.HasOne(l => l.DestinationField)
                .WithMany(f => f.DestinationLogicRules)
                .HasForeignKey(l => l.DestinationFieldId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed data for Logic
            var sampleFieldId1 = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var sampleFieldId2 = Guid.Parse("22222222-2222-2222-2222-222222222223");
            var seedDateTime = new DateTime(2026, 1, 11, 1, 46, 7);
            entity.HasData(
                new Logic
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    FieldId = sampleFieldId1,
                    Condition = LogicCondition.Is,
                    Value = "John Doe",
                    DestinationFieldId = sampleFieldId2,
                    Order = 0,
                    LogicGroupId = null,
                    CreatedAt = seedDateTime,
                    UpdatedAt = seedDateTime,
                    CreatedBy = "System",
                    UpdatedBy = "System",
                    IsActive = true
                }
            );
        });

        // Configure Submission entity
        modelBuilder.Entity<Submission>(entity =>
        {
            entity.ToTable("submissions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasColumnType("uuid");
            entity.Property(e => e.FormId).HasColumnName("form_id").HasColumnType("uuid").IsRequired();
            entity.Property(e => e.MetaData).HasColumnName("meta_data").HasColumnType("jsonb");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasColumnType("varchar");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasColumnType("varchar");
            entity.Property(e => e.IsActive).HasColumnName("is_active");

            // Relationship: Submission -> Form
            entity.HasOne(s => s.Form)
                .WithMany(form => form.Submissions)
                .HasForeignKey(s => s.FormId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed data for Submission
            var sampleFormId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var sampleSubmissionId = Guid.Parse("44444444-4444-4444-4444-444444444444");
            var seedDateTime = new DateTime(2026, 1, 11, 1, 46, 7);
            entity.HasData(
                new Submission
                {
                    Id = sampleSubmissionId,
                    FormId = sampleFormId,
                    CreatedAt = seedDateTime,
                    UpdatedAt = seedDateTime,
                    CreatedBy = "System",
                    UpdatedBy = "System",
                    IsActive = true
                }
            );
        });

        // Configure Answer entity
        modelBuilder.Entity<Answer>(entity =>
        {
            entity.ToTable("answers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasColumnType("uuid");
            entity.Property(e => e.SubmissionId).HasColumnName("submission_id").HasColumnType("uuid").IsRequired();
            entity.Property(e => e.FieldId).HasColumnName("field_id").HasColumnType("uuid").IsRequired();
            entity.Property(e => e.Value).HasColumnName("value").HasColumnType("jsonb").IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasColumnType("varchar");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasColumnType("varchar");
            entity.Property(e => e.IsActive).HasColumnName("is_active");

            // Relationship: Answer -> Submission
            entity.HasOne(a => a.Submission)
                .WithMany(s => s.Answers)
                .HasForeignKey(a => a.SubmissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship: Answer -> Field
            entity.HasOne(a => a.Field)
                .WithMany(f => f.Answers)
                .HasForeignKey(a => a.FieldId)
                .OnDelete(DeleteBehavior.Restrict);

            // Note: Answer seed data removed because JsonDocument cannot be seeded with HasData
            // Use migration SQL or DbContext seeding at runtime instead
        });
    }
}
