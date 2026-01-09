using BaseService.Domain.Entities;
using BaseService.Infrastructure.Contexts;
using ClientService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore.Models;

namespace ClientService.Infrastructure.Contexts;

public class FormsDbContext : AppDbContext
{
    public FormsDbContext(DbContextOptions<FormsDbContext> options) : base(options)
    {
    }

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
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasColumnType("varchar");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasColumnType("varchar");
            entity.Property(e => e.IsActive).HasColumnName("is_active");

            // Relationship: Form -> User (from BaseService)
            entity.HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);
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
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasColumnType("varchar");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasColumnType("varchar");
            entity.Property(e => e.IsActive).HasColumnName("is_active");

            // Relationship: Field -> Form
            entity.HasOne(f => f.Form)
                .WithMany(form => form.Fields)
                .HasForeignKey(f => f.FormId)
                .OnDelete(DeleteBehavior.Cascade);
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
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp");
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
        });

        // Configure Submission entity
        modelBuilder.Entity<Submission>(entity =>
        {
            entity.ToTable("submissions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasColumnType("uuid");
            entity.Property(e => e.FormId).HasColumnName("form_id").HasColumnType("uuid").IsRequired();
            entity.Property(e => e.MetaData).HasColumnName("meta_data").HasColumnType("jsonb");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasColumnType("varchar");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by").HasColumnType("varchar");
            entity.Property(e => e.IsActive).HasColumnName("is_active");

            // Relationship: Submission -> Form
            entity.HasOne(s => s.Form)
                .WithMany(form => form.Submissions)
                .HasForeignKey(s => s.FormId)
                .OnDelete(DeleteBehavior.Cascade);
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
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp");
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
        });
    }
}
