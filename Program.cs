using EFCoreApplication;

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
		Console.WriteLine("6. 列出最近在线日志");
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
				Actions.CreateDevice(deviceName.Trim());
				break;
			case "2":
				Actions.ListDevices();
				break;
			case "3":
				Console.Write("输入设备名称：");
				var deviceNameQuery = Console.ReadLine();
				if (string.IsNullOrWhiteSpace(deviceNameQuery)) {
					Console.WriteLine("设备名称不能为空");
					continue;
				}
				Actions.QueryDevice(deviceNameQuery.Trim());
				break;
			case "4":
				Console.Write("输入设备名称：");
				var deviceNameLog = Console.ReadLine();
				if (string.IsNullOrWhiteSpace(deviceNameLog)) {
					Console.WriteLine("设备名称不能为空");
					continue;
				}
				Console.Write("输入设备上报的地址：");
				var reportedAddressString = Console.ReadLine();
				if (string.IsNullOrWhiteSpace(reportedAddressString)) {
					Console.WriteLine("设备上报的地址不能为空");
					continue;
				}
				if (IPAddress.TryParse(reportedAddressString.Trim(), out var ipAddress) == false) {
					Console.WriteLine("无效的 IP 地址格式");
					continue;
				}
				Actions.LogOnline(deviceNameLog.Trim(), ipAddress);
				break;
			case "5":
				Console.Write("输入设备名称：");
				var deviceNameQueryLogs = Console.ReadLine();
				if (string.IsNullOrWhiteSpace(deviceNameQueryLogs)) {
					Console.WriteLine("设备名称不能为空");
					continue;
				}
				Console.Write("输入要查询的日志数量（默认 5，最大 100）：");
				var countString = Console.ReadLine();
				if (string.IsNullOrWhiteSpace(countString)) {
					Actions.QueryLogs(deviceNameQueryLogs.Trim());
				} else {
					if (!uint.TryParse(countString, out var count)) {
						count = 5;
						Console.WriteLine("无效的数量输入，已自动设置为默认值 5");
					}
					Actions.QueryLogs(deviceNameQueryLogs.Trim(), count);
				}
				break;
			case "6":
				Console.Write("输入要查询的日志数量（默认 20，最大 200）：");
				var listCountString = Console.ReadLine();
				if (string.IsNullOrWhiteSpace(listCountString)) {
					Actions.ListLogs();
				} else {
					if (!uint.TryParse(listCountString, out var listCount)) {
						listCount = 20;
						Console.WriteLine("无效的数量输入，已自动设置为默认值 20");
					}
					Actions.ListLogs(listCount);
				}
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