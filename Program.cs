using System;
using System.Diagnostics;
using System.IO;

class Program
{
    static readonly string directory = "/home/tomjerry/Downloads";
    static readonly string fileName = "random_9.6gb.dat";
    static readonly string filePath = Path.Combine(directory, fileName);
    static readonly long fileSize = (long)(9.6 * 1024 * 1024 * 1024);

    static void Main()
    {
        Console.WriteLine($"=== {fileSize / (1024 * 1024 * 1024):F2} GB USB 3.0 速度テスト ===\n");

        WriteTest();
        ReadTest();

        Console.WriteLine("\nテスト完了！");
    }

    static void WriteTest()
    {
        Directory.CreateDirectory(directory);
        Console.WriteLine("【書き込みテスト開始】");

        using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write,
            FileShare.None, 128 * 1024 * 1024, FileOptions.SequentialScan);

        fs.SetLength(fileSize);

        byte[] buffer = new byte[128 * 1024 * 1024];
        long written = 0;
        var rand = new Random();
        var sw = Stopwatch.StartNew();
        DateTime last = DateTime.Now;

        while (written < fileSize)
        {
            int len = (int)Math.Min(buffer.Length, fileSize - written);
            rand.NextBytes(buffer.AsSpan(0, len));
            fs.Write(buffer, 0, len);
            written += len;

            if ((DateTime.Now - last).TotalSeconds >= 1.0)
            {
                double speed = written / (1024.0 * 1024) / sw.Elapsed.TotalSeconds;
                Console.Write($"\r書き込み中... {written / (1024.0 * 1024 * 1024):F2} GB | {speed:F1} MB/s");
                last = DateTime.Now;
            }
        }

        fs.Flush(true);
        sw.Stop();
        Console.WriteLine($"\n\n✅ 書き込み完了 → {fileSize / (1024.0 * 1024) / sw.Elapsed.TotalSeconds:F1} MB/s");
    }

    static void ReadTest()
    {
        Console.WriteLine("\n【読み込みテスト開始】");

        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read,
            FileShare.Read, 128 * 1024 * 1024, FileOptions.SequentialScan);

        byte[] buffer = new byte[128 * 1024 * 1024];
        long total = 0;
        var sw = Stopwatch.StartNew();
        DateTime last = DateTime.Now;

        while (total < fileSize)
        {
            int len = (int)Math.Min(buffer.Length, fileSize - total);
            int read = fs.Read(buffer, 0, len);
            if (read == 0) break;
            total += read;

            if ((DateTime.Now - last).TotalSeconds >= 1.0)
            {
                double speed = total / (1024.0 * 1024) / sw.Elapsed.TotalSeconds;
                Console.Write($"\r読み込み中... {total / (1024.0 * 1024 * 1024):F2} GB | {speed:F1} MB/s");
                last = DateTime.Now;
            }
        }

        sw.Stop();
        Console.WriteLine($"\n\n✅ 読み込み完了 → {fileSize / (1024.0 * 1024) / sw.Elapsed.TotalSeconds:F1} MB/s");
    }
}