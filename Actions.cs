namespace EFCoreApplication;

internal static class Actions {
	public static void CreateDevice(string deviceName) {
		using var db = new DeviceOnlineLoggingContext();

		// Check if device already exists
		if (db.Devices.Any(d => d.DeviceName == deviceName)) {
			Console.WriteLine($"设备 {deviceName} 已存在");
			return;
		}

		var device = new Device {
			DeviceName = deviceName,
			LatestLANIPAddress = IPAddress.None,
			LatestLogTime = DateTime.UtcNow,
		};

		db.Devices.Add(device);
		db.SaveChanges();

		Console.WriteLine($"已创建设备 {deviceName}，DeviceId: {device.DeviceId}");
	}

	public static void ListDevices() {
		using var db = new DeviceOnlineLoggingContext();

		var devices = db.Devices
			.Select(d => new { d.DeviceName, d.DeviceId })
			.ToList();

		if (devices.Count == 0) {
			Console.WriteLine("没有找到任何设备");
			return;
		}

		Console.WriteLine($"设备列表（共 {devices.Count} 个）：");
		foreach (var device in devices) {
			Console.WriteLine($"DeviceName: {device.DeviceName}, DeviceId: {device.DeviceId}");
		}
	}

	public static void QueryDevice(string deviceName) {
		using var db = new DeviceOnlineLoggingContext();

		var device = db.Devices.FirstOrDefault(d => d.DeviceName == deviceName);
		if (device == null) {
			Console.WriteLine($"未找到设备 {deviceName}");
			return;
		}

		Console.WriteLine($"已找到设备 {deviceName} ({device.DeviceId})");
		Console.WriteLine($"LatestLANIPAddress: {device.LatestLANIPAddress}, LatestLogTime: {device.LatestLogTime}");
	}

	public static void LogLANIPAddress(string deviceName, IPAddress lanIPAddress, string? message = null) {
		using var db = new DeviceOnlineLoggingContext();

		var device = db.Devices.FirstOrDefault(d => d.DeviceName == deviceName);
		if (device == null) {
			Console.WriteLine($"未找到设备 {deviceName}");
			return;
		}

		var log = new OnlineLog {
			Device = device,
			LANIPAddress = lanIPAddress,
			LogTime = DateTime.UtcNow,
			Message = message,
		};

		db.OnlineLogs.Add(log);

		device.LatestLANIPAddress = lanIPAddress;
		device.LatestLogTime = log.LogTime;

		db.SaveChanges();

		Console.WriteLine($"已记录设备 {deviceName} 的在线日志，LAN IP 地址：{lanIPAddress}");
	}

	public static void QueryLogs(string deviceName, uint count = 5) {
		if (count == 0) {
			Console.WriteLine("日志数量必须大于 0");
			return;
		}

		if (count > 100) {
			Console.WriteLine("日志数量不能超过 100，已自动调整为 100");
			count = 100;
		}

		using var db = new DeviceOnlineLoggingContext();

		var result = db.Devices.Where(d => d.DeviceName == deviceName)
			.Select(d => new {
				d.DeviceName,
				RecentLogs = d.OnlineLogs
					.OrderByDescending(log => log.LogTime)
					.Take((int)count)
					.ToList()
			})
			.FirstOrDefault();

		if (result == null) {
			Console.WriteLine($"未找到设备 {deviceName}");
			return;
		}

		var logs = result.RecentLogs;

		if (logs.Count == 0) {
			Console.WriteLine($"设备 {result.DeviceName} 没有任何日志");
			return;
		}

		Console.WriteLine($"设备 {result.DeviceName} 的最近 {logs.Count} 条日志：");
		foreach (var log in logs) {
			Console.WriteLine($"LogTime: {log.LogTime}, LAN IP: {log.LANIPAddress}, Message: {log.Message}");
		}
	}

	public static void ListLogs(uint count = 20) {
		if (count == 0) {
			Console.WriteLine("日志数量必须大于 0");
			return;
		}

		if (count > 200) {
			Console.WriteLine("日志数量不能超过 200，已自动调整为 200");
			count = 200;
		}

		using var db = new DeviceOnlineLoggingContext();

		var logs = db.OnlineLogs
			.OrderByDescending(log => log.LogTime)
			.Take((int)count)
			.Select(log => new {
				log.Device.DeviceName,
				log.LogTime,
				log.LANIPAddress,
				log.Message
			})
			.ToList();

		if (logs.Count == 0) {
			Console.WriteLine("没有找到任何日志");
			return;
		}

		Console.WriteLine($"最近 {logs.Count} 条日志：");
		foreach (var log in logs) {
			Console.WriteLine($"Device: {log.DeviceName}, LogTime: {log.LogTime}, LAN IP: {log.LANIPAddress}, Message: {log.Message}");
		}
	}
}