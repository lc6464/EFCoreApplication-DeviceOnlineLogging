using Microsoft.EntityFrameworkCore;
using System.Net;

try {
	Directory.CreateDirectory(DeviceOnlineLoggingContext.DbFolder);
} catch (Exception e) {
	Console.Error.WriteLine($"无法创建数据库文件夹 {DeviceOnlineLoggingContext.DbFolder}\n异常信息：{e.Message}");
	return -1;
}


try {
	using var db = new DeviceOnlineLoggingContext();
	db.Database.Migrate();
} catch (Exception e) {
	Console.Error.WriteLine($"无法创建或迁移（更新）数据库文件\n异常信息：{e.Message}");
	return -1;
}

Console.WriteLine("设备在线日志管理控制台");

while (true) {
	try {
		Console.WriteLine("\n请选择操作：");
		Console.WriteLine("1. 创建设备");
		Console.WriteLine("2. 列出所有设备");
		Console.WriteLine("3. 查询设备");
		Console.WriteLine("4. 记录设备在线日志");
		Console.WriteLine("5. 查询设备在线日志");
		Console.WriteLine("0. 退出");
		Console.Write("输入选项编号：");

		switch (Console.ReadLine()) {
			case "1":
				Console.Write("输入设备名称：");
				var deviceName = Console.ReadLine();
				if (string.IsNullOrWhiteSpace(deviceName)) {
					Console.WriteLine("设备名称不能为空");
					continue;
				}
				CreateDevice(deviceName.Trim());
				break;
			case "2":
				ListDevices();
				break;
			case "3":
				Console.Write("输入设备名称：");
				var deviceNameQuery = Console.ReadLine();
				if (string.IsNullOrWhiteSpace(deviceNameQuery)) {
					Console.WriteLine("设备名称不能为空");
					continue;
				}
				QueryDevice(deviceNameQuery.Trim());
				break;
			case "4":
				Console.Write("输入设备名称：");
				var deviceNameLog = Console.ReadLine();
				if (string.IsNullOrWhiteSpace(deviceNameLog)) {
					Console.WriteLine("设备名称不能为空");
					continue;
				}
				Console.Write("输入设备 LAN IP 地址：");
				var lanIpAddress = Console.ReadLine();
				if (string.IsNullOrWhiteSpace(lanIpAddress)) {
					Console.WriteLine("设备 LAN IP 地址不能为空");
					continue;
				}
				if (IPAddress.TryParse(lanIpAddress.Trim(), out var ipAddress) == false) {
					Console.WriteLine("无效的 IP 地址格式");
					continue;
				}
				LogLANIPAddress(deviceNameLog.Trim(), ipAddress);
				break;
			case "5":
				Console.Write("输入设备名称：");
				var deviceNameQueryLogs = Console.ReadLine();
				if (string.IsNullOrWhiteSpace(deviceNameQueryLogs)) {
					Console.WriteLine("设备名称不能为空");
					continue;
				}
				QueryLogs(deviceNameQueryLogs.Trim());
				break;
			case "0":
				return 0;
			default:
				Console.WriteLine("无效选项，请重新输入");
				break;
		}
	} catch (Exception e) {
		Console.Error.WriteLine($"检测到异常：{e.Message}");
		continue;
	}
}


void CreateDevice(string deviceName) {
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

void ListDevices() {
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

void QueryDevice(string deviceName) {
	using var db = new DeviceOnlineLoggingContext();

	var device = db.Devices.FirstOrDefault(d => d.DeviceName == deviceName);
	if (device == null) {
		Console.WriteLine($"未找到设备 {deviceName}");
		return;
	}

	Console.WriteLine($"已找到设备 {deviceName} ({device.DeviceId})");
	Console.WriteLine($"LatestLANIPAddress: {device.LatestLANIPAddress}, LatestLogTime: {device.LatestLogTime}");
}

void LogLANIPAddress(string deviceName, IPAddress lanIPAddress, string? message = null) {
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

void QueryLogs(string deviceName, uint count = 5) {
	using var db = new DeviceOnlineLoggingContext();

	var result = db.Devices.Where(d => d.DeviceName == deviceName)
		.Select(d => new {
			DeviceName = d.DeviceName,
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