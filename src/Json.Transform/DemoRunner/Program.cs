using Json.Transform.Demo;
using System.Diagnostics;

namespace Json.Transform.DemoRunner;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("🎬 JSON Transform Library Demo Generator");
        Console.WriteLine("=========================================");
        Console.WriteLine();

        var runTests = args.Contains("--run-tests") || args.Contains("-t");
        var openBrowser = !args.Contains("--no-browser");
        var outputPath = GetOutputPath(args);

        try
        {
            // Run tests if requested
            if (runTests)
            {
                Console.WriteLine("🧪 Running tests first...");
                var testResult = await RunTests();
                if (!testResult)
                {
                    Console.WriteLine("❌ Tests failed! Demo will still be generated but may show errors.");
                }
                Console.WriteLine();
            }

            // Generate demo
            Console.WriteLine("🔄 Generating dynamic demo with real transformation results...");
            var generator = new DemoGenerator();
            
            var stopwatch = Stopwatch.StartNew();
            await generator.SaveDemoToFile(outputPath);
            stopwatch.Stop();

            Console.WriteLine($"✅ Demo generated successfully in {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine($"📄 Output: {outputPath}");
            Console.WriteLine();

            // Display demo info
            DisplayDemoInfo();

            // Open in browser
            if (openBrowser)
            {
                Console.WriteLine("🌐 Opening demo in default browser...");
                OpenInBrowser(outputPath);
            }
            else
            {
                Console.WriteLine($"💡 To view the demo, open: {outputPath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error generating demo: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private static string GetOutputPath(string[] args)
    {
        var outputIndex = Array.IndexOf(args, "--output");
        if (outputIndex >= 0 && outputIndex + 1 < args.Length)
        {
            return args[outputIndex + 1];
        }

        var outputIndexShort = Array.IndexOf(args, "-o");
        if (outputIndexShort >= 0 && outputIndexShort + 1 < args.Length)
        {
            return args[outputIndexShort + 1];
        }

        // Default to the project root
        var currentDir = Directory.GetCurrentDirectory();
        var projectRoot = FindProjectRoot(currentDir);
        return System.IO.Path.Combine(projectRoot, "demo.html");
    }

    private static string FindProjectRoot(string startPath)
    {
        var current = new DirectoryInfo(startPath);
        while (current != null)
        {
            if (current.GetFiles("*.sln").Any() || 
                current.GetFiles("demo.html").Any() ||
                current.GetDirectories("src").Any())
            {
                return current.FullName;
            }
            current = current.Parent;
        }
        return startPath;
    }

    private static async Task<bool> RunTests()
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "test --logger:console;verbosity=minimal",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            
            await process.WaitForExitAsync();

            if (process.ExitCode == 0)
            {
                Console.WriteLine("✅ All tests passed!");
                return true;
            }
            else
            {
                Console.WriteLine($"❌ Tests failed with exit code: {process.ExitCode}");
                if (!string.IsNullOrEmpty(error))
                    Console.WriteLine($"Error: {error}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error running tests: {ex.Message}");
            return false;
        }
    }

    private static void DisplayDemoInfo()
    {
        Console.WriteLine("📊 Demo Features:");
        Console.WriteLine("  🔄 Field Mapping - Copy/move fields between JSON structures");
        Console.WriteLine("  🎯 Conditional Logic - If/else conditions with expressions");
        Console.WriteLine("  📊 Aggregation - Sum, average, min, max operations");
        Console.WriteLine("  🧮 Math Operations - Arithmetic operations on fields");
        Console.WriteLine("  🔗 String Concatenation - Combine fields with templates");
        Console.WriteLine("  🏗️ Complex Transformations - Deep nested object mapping");
        Console.WriteLine();
        Console.WriteLine("🎯 All transformations are executed with REAL DATA and REAL RESULTS!");
        Console.WriteLine();
    }

    private static void OpenInBrowser(string filePath)
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
            else if (OperatingSystem.IsMacOS())
            {
                Process.Start("open", filePath);
            }
            else if (OperatingSystem.IsLinux())
            {
                Process.Start("xdg-open", filePath);
            }
            else
            {
                Console.WriteLine($"💡 Please manually open: {filePath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Could not open browser automatically: {ex.Message}");
            Console.WriteLine($"💡 Please manually open: {filePath}");
        }
    }
}
