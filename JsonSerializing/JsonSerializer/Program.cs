using JsonTest.dto;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        SimpleDto dto = new SimpleDto();
        var s = JsonSerializer.Serialize(dto);
        Console.WriteLine(s);

        string[] cpuStats = File.ReadAllLines("/proc/stat")[0].Split(' ');
        Console.WriteLine("Total cpuStats : "+((cpuStats != null) ? cpuStats.Length : 0));
        foreach (string cpuStat in cpuStats)
        {
            Console.WriteLine(cpuStat);
        }
        string dotnetInfo;
        try
        {
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = "cat";
            p.StartInfo.Arguments = "/proc/stat";

            p.Start();
            dotnetInfo = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            Console.WriteLine(dotnetInfo);
        }
        catch (Exception ex)
        {
            {
                Console.WriteLine(ex.ToString());
            }

        }
    }
}