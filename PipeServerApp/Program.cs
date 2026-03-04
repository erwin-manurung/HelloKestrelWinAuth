using System.Diagnostics;
using System.IO.Pipes;

internal class Program
{
    private static void Main(string[] args)
    {

        //var pipeName = "PipeServer" + Process.GetCurrentProcess().Id;
        var pipeName = "PipeServer-Hola-Shared";
        var currFlag = 0;
        const int KillFlag = 200;
        while (currFlag != KillFlag)
        {
            using (NamedPipeServerStream pipe = new NamedPipeServerStream(pipeName, PipeDirection.InOut))
            {
                Console.WriteLine($"Waiting for connection (pipename {pipeName})...");

                pipe.WaitForConnection();
                byte[] b = new byte[4];
                pipe.Read(b, 0, 4);
                currFlag = BitConverter.ToInt32(b, 0);
                Console.WriteLine($"Receiving data:{currFlag}");

            }
            Task.Delay(1000);
        }
        
    }
}