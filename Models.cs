using Microsoft.Data.Sqlite;

namespace EFCoreApplication;

public class DeviceOnlineLoggingContext : DbContext {
	public DbSet<OnlineLog> OnlineLogs { get; set; }
	public DbSet<Device> Devices { get; set; }

	public static string DbFolder {
		get {
			var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			return Path.Join(localAppData, "LC", "EFCoreApplication");
		}
	}

	private static string DbPath => Path.Join(DbFolder, "data.db");

	private static string ConnectionString => new SqliteConnectionStringBuilder {
		DataSource = DbPath,
	}.ToString();

	protected override void OnConfiguring(DbContextOptionsBuilder options)
		=> options.UseSqlite(ConnectionString);

	protected override void OnModelCreating(ModelBuilder modelBuilder) =>
		modelBuilder.Entity<Device>().HasIndex(e => e.DeviceName).IsUnique();
}

public class OnlineLog {
	public int OnlineLogId { get; set; }

	public DateTime LogTime { get; set; }

	public required IPAddress LANIPAddress { get; set; }

	public string? Message { get; set; }


	public Guid DeviceId { get; set; }

	public Device Device { get; set; } = null!;
}

public class Device {
	public Guid DeviceId { get; set; }

	public required string DeviceName { get; set; }

	public DateTime LatestLogTime { get; set; }

	public required IPAddress LatestLANIPAddress { get; set; }


	public ICollection<OnlineLog> OnlineLogs { get; } = [];
}