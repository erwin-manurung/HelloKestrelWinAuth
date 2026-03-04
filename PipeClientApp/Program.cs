using System.IO.Pipes;
using System.Security.Principal;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        var pipeName = "PipeServer-Hola-Shared";
        using (NamedPipeClientStream regPipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Delegation))
        {
            regPipe.Connect(5000);
            WriteByte(regPipe, BitConverter.GetBytes(200), 0, 4);
        }
    }

    private static void WriteByte(Stream pipe, byte[] buffer, int offset, int count)
    {
        pipe.Write(buffer, offset, count);
    }
}