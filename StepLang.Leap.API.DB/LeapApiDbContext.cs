using Leap.API.DB.Entities;
using Microsoft.EntityFrameworkCore;

namespace Leap.API.DB;

public class LeapApiDbContext(DbContextOptions<LeapApiDbContext> options) : DbContext(options)
{
	public DbSet<Library> Libraries { get; set; } = null!;

	public DbSet<LibraryVersion> LibraryVersions { get; set; } = null!;

	public DbSet<PendingLibraryVersion> PendingLibraryVersions { get; set; } = null!;

	public DbSet<Author> Authors { get; set; } = null!;

	public Task InitializeAsync(CancellationToken cancellationToken = default)
	{
		return Database.MigrateAsync(cancellationToken);
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<Library>()
			.HasOne<LibraryVersion>(l => l.LatestVersion);

		modelBuilder.Entity<Library>()
			.HasMany<LibraryVersion>(l => l.Versions)
			.WithOne(v => v.Library);

		modelBuilder.Entity<Library>()
			.HasMany<Author>(l => l.Maintainers)
			.WithMany(a => a.Libraries);

		modelBuilder.Entity<Library>()
			.HasIndex(
				l => new
				{
					l.Author,
					l.Name,
				}
			)
			.IsUnique();

		modelBuilder.Entity<LibraryLink>()
			.HasKey(
				d => new
				{
					VersionId = d.SourceId, DependencyId = d.TargetId,
				}
			);

		// modelBuilder.Entity<Library>()
		// 	.HasMany<LibraryLink>(l => l.Dependents)
		// 	.WithOne(d => d.Target);

		modelBuilder.Entity<LibraryVersion>()
			.HasMany<LibraryLink>(v => v.Links)
			.WithOne(l => l.Source);

		modelBuilder.Entity<PendingLibraryVersion>()
			.Navigation(p => p.Library)
			.AutoInclude();
	}
}
