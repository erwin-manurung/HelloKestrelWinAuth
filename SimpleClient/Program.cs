using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Net.Sockets;


public class Program
{
    public static ICredentials creds = CredentialCache.DefaultCredentials;
    public static async Task Main(string[] args) {
        var CurrentWindowsIdentity = System.Security.Principal.WindowsIdentity.GetCurrent();
        Console.WriteLine($"Hello {CurrentWindowsIdentity.Name}, World!");
        var uri = new Uri("http://jktalas01/WeatherForecast");
        uri = new Uri("http://localhost:5111/WeatherForecast");

        var defCred = CredentialCache.DefaultNetworkCredentials;
        var explicitCred = new NetworkCredential("adc\\e.manurung", "4ll3gr0@->1nd0nF");

        Program.creds = defCred;
        //Program.creds = explicitCred;

        await Program.SendGetRequestAsync<string>(Program.creds, Program.creds, uri, (a) => {
            Console.WriteLine($"Good Content: {a}");
        }, "application/json", new JsonMediaTypeFormatter());
    }


    public async static Task SendGetRequestAsync<T>(ICredentials credentials, ICredentials proxyCredentials, Uri uri, Action<T> contentHandler, string mediaType, MediaTypeFormatter formatter)
    {
        var credentialsCache = new CredentialCache {
            { uri,
                "Negotiate",
                (NetworkCredential) credentials
            }
        };

        var proxyCredentialsCache = new CredentialCache {
            { uri,
                "Negotiate",
                (NetworkCredential) proxyCredentials
            }
        };
        HttpClientHandler defaultHandler = new HttpClientHandler()
        {
            Credentials = credentialsCache,
            DefaultProxyCredentials = proxyCredentialsCache,
            ServerCertificateCustomValidationCallback = (request, cert, chain, errors) =>
            {
                Console.WriteLine("SSL error skipped");
                return true;
            },
            UseDefaultCredentials = false
        };
        var httpClient = new HttpClient(defaultHandler, false);

        HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, uri);
        if (mediaType != null)
        {
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));
        }
        //req.Headers.Add("WWW-Authenticate", "Negotiate");
        HttpResponseMessage response = null;

        try
        {
            response = httpClient.SendAsync(req).Result;
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error hitting url {uri}, Message{ex.Message}", ex);
            throw;
        }

        T line;
        if (formatter == null)
        {
            line = await response.Content.ReadAsAsync<T>(new[] { new TextMediaTypeFormatter() });
        }
        else
        {
            line = await response.Content.ReadAsAsync<T>(new[] { formatter });
        }
        contentHandler(line);
    }
}

public class TextMediaTypeFormatter : MediaTypeFormatter
{
    public TextMediaTypeFormatter()
    {
        SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/plain"));
        SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
    }

    public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
    {
        return ReadFromStreamAsync(type, readStream, content, formatterLogger, CancellationToken.None);
    }

    public override async Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger, CancellationToken cancellationToken)
    {
        using (var streamReader = new StreamReader(readStream))
        {
            return await streamReader.ReadToEndAsync();
        }
    }

    public override bool CanReadType(Type type)
    {
        return type == typeof(string);
    }

    public override bool CanWriteType(Type type)
    {
        return false;
    }
}

public class JsonMediaTypeFormatter : MediaTypeFormatter
{
    public JsonMediaTypeFormatter()
    {
        SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));
        SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/json"));
    }

    public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
    {
        return ReadFromStreamAsync(type, readStream, content, formatterLogger, CancellationToken.None);
    }

    public override async Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger, CancellationToken cancellationToken)
    {
        using (var streamReader = new StreamReader(readStream))
        {
            return await streamReader.ReadToEndAsync();
        }
    }

    public override bool CanReadType(Type type)
    {
        return type == typeof(string);
    }

    public override bool CanWriteType(Type type)
    {
        return false;
    }
}