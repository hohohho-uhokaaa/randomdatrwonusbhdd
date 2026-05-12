//
// copilot advised me to.
// 10GBのファイルを作成してUSB 3.0の速度を測定するC#プログラム
// read/write 時の buffer サイズは 1GB に設定
//
// 使い方: dotnet run -- [保存先ディレクトリ]
// 例: dotnet run -- "/home/orenopasokon/10lrandom.dat"
// 注意: 大容量ファイルの作成と読み書きには時間がかかるため、十分な空き容量と時間を確保
//      保存先ディレクトリは SATA3 HDD 上のパスを指定する
//      SATA3 HDD に write できなかった場合は /tmp が write 先になる
//
// SIGINT で強制終了を追加
// Yeah, watching code execute is mind-numbingly boring. Threw in some SIGINT catching so I can bail instantly with Ctrl+C. Way better.
// Grok->Like Elon Musk would say.
//

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

public enum ByteUnit { KB = 1, MB = 2, GB = 3, TB = 4 }

public static class ByteExtensions
{
    private const double BytesPerUnitBase = 1024.0;

    /// <summary>
    /// バイト数を指定された単位に変換します。
    /// </summary>
    /// <param name="bytes">変換するバイト数。負の値は許可されません。</param>
    /// <param name="unit">変換先の単位。</param>
    /// <returns>指定された単位の値。</returns>
    /// <exception cref="ArgumentOutOfRangeException">bytes が負の場合にスローされます。</exception>
    public static double ConvertBytes(this long bytes, ByteUnit unit)
    {
        if (bytes < 0) throw new ArgumentOutOfRangeException(nameof(bytes), "バイト数は負の値にできません。");
        return bytes / Math.Pow(BytesPerUnitBase, (int)unit);
    }

    /// <summary>
    /// バイト数をギガバイトに変換します。
    /// </summary>
    /// <param name="bytes">変換するバイト数。</param>
    /// <returns>ギガバイト単位の値。</returns>
    public static double ToGB(this long bytes)
    {
        return bytes.ConvertBytes(ByteUnit.GB);
    }

    /// <summary>
    /// バイト数をギガバイトに変換し、フォーマット済みの文字列を返します。
    /// </summary>
    /// <param name="bytes">変換するバイト数。</param>
    /// <param name="format">フォーマット文字列（デフォルト: "F2"）。</param>
    /// <returns>フォーマット済みの文字列。</returns>
    public static string ToGBString(this long bytes, string format = "F2")
    {
        double gb = bytes.ToGB();
        return gb.ToString(format);
    }
}

class Program
{
    static readonly string fileName = "random_100gb.dat";
    static readonly long fileSize = 10L * 1024 * 1024 * 1024;

    static void Main(string[] args)
    {
        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
            Console.WriteLine("\nSIGINT 受信: 終了処理中...");
        };

        string directory = args.Length > 0 ? args[0] : "/home/orenopasokon/10lrandom.dat";
        string filePath = Path.Combine(directory, fileName);
        {
            try
            {
                Console.WriteLine($"=== {fileSize.ToGBString()} GB USB 3.0 速度テスト ===\n");

                // テスト前にファイルが存在すれば削除
                if (File.Exists(filePath))
                {
                    Console.WriteLine("既存ファイルを削除中...");
                    File.Delete(filePath);
                }
                // 書き込みテストと読み込みテストを実行
                WriteTest(filePath, cts.Token);
                ReadTest(filePath, cts.Token);

                Console.WriteLine("\n✅ テスト完了！");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("\n⚠️ テストは中断されました。ファイルを削除して終了します。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ エラーが発生しました: {ex.Message}");
            }
            finally
            {
                // テスト後にテストファイルを削除（オプション）
                if (File.Exists(filePath))
                {
                    try { File.Delete(filePath); }
                    catch { }
                }
            }
        }

        static void WriteTest(string filePath, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                string directory = Path.GetDirectoryName(filePath) ?? "/tmp";
                Directory.CreateDirectory(directory);
                Console.WriteLine("【書き込みテスト開始】");

                using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write,
                    FileShare.None, 1024 * 1024 * 1024, FileOptions.SequentialScan);

                fs.SetLength(fileSize);

                // buffer = 512MB の場合は 1024 を 512 に修正して build & run
                byte[] buffer = new byte[1024 * 1024 * 1024];
                long written = 0;
                var rand = new Random();
                var sw = Stopwatch.StartNew();
                DateTime last = DateTime.Now;

                while (written < fileSize)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    int len = (int)Math.Min(buffer.Length, fileSize - written);
                    rand.NextBytes(buffer.AsSpan(0, len));
                    fs.Write(buffer, 0, len);
                    written += len;

                    if ((DateTime.Now - last).TotalSeconds >= 1.0)
                    {
                        double speed = written / (1024.0 * 1024) / sw.Elapsed.TotalSeconds;
                        Console.Write($"\r書き込み中... {written.ToGBString()} GB | {speed:F1} MB/s  ");
                        last = DateTime.Now;
                    }
                }

                fs.Flush(true);
                sw.Stop();
                double finalSpeed = fileSize / (1024.0 * 1024) / sw.Elapsed.TotalSeconds;
                Console.WriteLine($"\n✅ 書き込み完了 → {finalSpeed:F1} MB/s ({sw.Elapsed.TotalSeconds:F1}秒)\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ 書き込みテスト失敗: {ex.Message}");
                throw;
            }
        }

        static void ReadTest(string filePath, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                Console.WriteLine("【読み込みテスト開始】");

                // メモリを明示的にクリア
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                // Linuxの場合、ページキャッシュをクリア sudo 必須
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    try
                    {
                        File.WriteAllText("/proc/sys/vm/drop_caches", "3");
                        System.Threading.Thread.Sleep(500);
                    }
                    catch
                    {
                        // root権限がない場合はスキップ
                        Console.WriteLine("（キャッシュクリアをスキップ: root権限が必要）");
                    }
                }

                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read,
                    FileShare.Read, 1024 * 1024 * 1024, FileOptions.SequentialScan);

                // buffer = 512MB の場合は 1024 を 512 に修正して build & run
                byte[] buffer = new byte[1024 * 1024 * 1024];
                long total = 0;
                var sw = Stopwatch.StartNew();
                DateTime last = DateTime.Now;

                while (total < fileSize)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    int len = (int)Math.Min(buffer.Length, fileSize - total);
                    int read = fs.Read(buffer, 0, len);
                    if (read == 0) break;
                    total += read;

                    if ((DateTime.Now - last).TotalSeconds >= 1.0)
                    {
                        double speed = total / (1024.0 * 1024) / sw.Elapsed.TotalSeconds;
                        Console.Write($"\r読み込み中... {total.ToGBString()} GB | {speed:F1} MB/s  ");
                        last = DateTime.Now;
                    }
                }

                sw.Stop();
                double finalSpeed = fileSize / (1024.0 * 1024) / sw.Elapsed.TotalSeconds;
                Console.WriteLine($"\n✅ 読み込み完了 → {finalSpeed:F1} MB/s ({sw.Elapsed.TotalSeconds:F1}秒)\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ 読み込みテスト失敗: {ex.Message}");
                throw;
            }
        }
    }
}
