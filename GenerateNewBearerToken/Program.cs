using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Net.Http.Headers;

var builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

var app = builder.Build();
using (var httpClient = new HttpClient())
{
    using (var request = new HttpRequestMessage(new HttpMethod("POST"), app.GetSection("Auth0:TokenUrl").Value))
    {
        string content = $"{{\"client_id\":\"{app.GetSection("Auth0:ClientId").Value}\"," +
             $"\"client_secret\":\"{app.GetSection("Auth0:ClientSecret").Value}\"," +
             $"\"audience\":\"{app.GetSection("Auth0:Audience").Value}\"," +
             $"\"grant_type\":\"{app.GetSection("Auth0:GrantType").Value}\"}}";

        request.Content = new StringContent(content);
        ;
        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

        var response = await httpClient.SendAsync(request);
        string filePath = $"{app.GetSection("TokenDirectory").Value.ToString()}/GeneratedToken.txt";

        Directory.CreateDirectory(app.GetSection("TokenDirectory").Value.ToString());
        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
        }

        System.IO.File.WriteAllText(filePath, await response.Content.ReadAsStringAsync());
        Process.Start("notepad.exe", filePath);
    }
}

Environment.Exit(0);