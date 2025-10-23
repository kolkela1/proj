using System;
using System.IO;

namespace VirtualDiskProject
{
	public class VirtualDisk
	{
		public const int CLUSTER_SIZE = 1024;
		public const int CLUSTERS_NUMBER = 1024;

		private long diskSize;
		private string diskPath;
		private FileStream diskFile;
		private bool isOpen;

		public bool IsOpen => isOpen;
		public long DiskSize => diskSize;
		public string DiskPath => diskPath;

		// ---------------------------------------------------------
		// Initializes the virtual disk.
		// ---------------------------------------------------------
		public void Initialize(string path, bool createIfMissing = true)
		{
			if (isOpen)
				throw new InvalidOperationException("Disk is already initialized.");

			diskPath = path;
			diskSize = CLUSTER_SIZE * CLUSTERS_NUMBER;

			try
			{
				if (!File.Exists(diskPath))
				{
					if (createIfMissing)
						CreateEmptyDisk(diskPath);
					else
						throw new FileNotFoundException("Couldn't find the specified disk path.");
				}

				diskFile = new FileStream(diskPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
				isOpen = true;
			}
			catch (Exception ex)
			{
				isOpen = false;
				throw new IOException($"Failed to open disk: {ex.Message}", ex);
			}
		}

		// ---------------------------------------------------------
		// Creates a new empty virtual disk file.
		// ---------------------------------------------------------
		private void CreateEmptyDisk(string path)
		{
			try
			{
				using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
				{
					byte[] emptyCluster = new byte[CLUSTER_SIZE];
					for (int i = 0; i < CLUSTERS_NUMBER; i++)
					{
						fs.Write(emptyCluster, 0, CLUSTER_SIZE);
					}
					fs.Flush();
				}
			}
			catch (Exception ex)
			{
				throw new IOException($"Failed to create disk file: {ex.Message}", ex);
			}
		}

		// ---------------------------------------------------------
		// Writes data to a specific cluster index.
		// ---------------------------------------------------------
		public void WriteCluster(int clusterIndex, byte[] data)
		{
			if (!isOpen)
				throw new InvalidOperationException("Disk is not open.");

			if (clusterIndex < 0 || clusterIndex >= CLUSTERS_NUMBER)
				throw new ArgumentOutOfRangeException(nameof(clusterIndex), "Invalid cluster index.");

			if (data.Length > CLUSTER_SIZE)
				throw new ArgumentException("Data exceeds cluster size.");

			long offset = clusterIndex * CLUSTER_SIZE;
			diskFile.Seek(offset, SeekOrigin.Begin);

			// Fill with zeros if smaller than cluster size
			byte[] buffer = new byte[CLUSTER_SIZE];
			Array.Copy(data, buffer, data.Length);

			diskFile.Write(buffer, 0, buffer.Length);
			diskFile.Flush();
		}

		// ---------------------------------------------------------
		// Reads data from a specific cluster index.
		// ---------------------------------------------------------
		public byte[] ReadCluster(int clusterIndex)
		{
			if (!isOpen)
				throw new InvalidOperationException("Disk is not open.");

			if (clusterIndex < 0 || clusterIndex >= CLUSTERS_NUMBER)
				throw new ArgumentOutOfRangeException(nameof(clusterIndex), "Invalid cluster index.");

			long offset = clusterIndex * CLUSTER_SIZE;
			diskFile.Seek(offset, SeekOrigin.Begin);

			byte[] buffer = new byte[CLUSTER_SIZE];
			int bytesRead = diskFile.Read(buffer, 0, CLUSTER_SIZE);

			if (bytesRead < CLUSTER_SIZE)
				Array.Resize(ref buffer, bytesRead);

			return buffer;
		}

		// ---------------------------------------------------------
		// Closes the disk file safely.
		// ---------------------------------------------------------
		public void Close()
		{
			if (isOpen)
			{
				diskFile.Close();
				isOpen = false;
			}
		}
	}
}
