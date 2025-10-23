using System;
using System.Text;

namespace VirtualDiskProject
{
	internal class Program
	{
		static void Main(string[] args)
		{
			try
			{
				VirtualDisk disk = new VirtualDisk();
				string path = "virtual_disk.bin";

				// Initialize or create the disk file
				disk.Initialize(path, createIfMissing: true);
				Console.WriteLine($"Disk initialized at: {path}");
				Console.WriteLine($"Disk size: {disk.DiskSize} bytes");

				// Example: Write data to cluster 0
				string message = "Hello Virtual Disk!";
				byte[] data = Encoding.UTF8.GetBytes(message);
				disk.WriteCluster(0, data);
				Console.WriteLine("Data written to cluster 0.");

				// Example: Read data back
				byte[] readData = disk.ReadCluster(0);
				string readMessage = Encoding.UTF8.GetString(readData).TrimEnd('\0');
				Console.WriteLine($"Data read from cluster 0: {readMessage}");

				// Close disk
				disk.Close();
				Console.WriteLine("Disk closed successfully.");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
			}
		}
	}
}
