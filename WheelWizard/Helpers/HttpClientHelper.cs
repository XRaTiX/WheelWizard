using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using WheelWizard.Models;

namespace WheelWizard.Helpers;


//todo: Use static HttpClient with pooled connection lifetime https://stackoverflow.com/a/77379657
public static class HttpClientHelper
{
    // This is bool here is just for testing purposes and should never be used in the production code.
    // It allows us to test the application asif there is no internet connection.
    public static bool FakeConnectionToInternet { get; set; } = true;
    
    private static readonly Lazy<HttpClient> LazyHttpClient = new Lazy<HttpClient>(() =>
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("WheelWizard/2.0");
        return client;
    });
    
    private static HttpClient HttpClient => LazyHttpClient.Value;

    public static async Task<HttpClientResult<T>> PostAsync<T>(string url, HttpContent? body)
    {
    #if DEBUG
        if (!FakeConnectionToInternet)
            return GetErrorResult<T>(new ("No internet connection"));
    #endif
        
        HttpClientResult<T> result;
        try
        {
            var response = await HttpClient.PostAsync(url, body);
            result = new HttpClientResult<T>()
            {
                StatusCode = (int)response.StatusCode,
                Succeeded = response.IsSuccessStatusCode,
                StatusMessage = response.ReasonPhrase
            };

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                if (typeof(T) == typeof(string))
                    result.Content = (T)(object)content;
                else
                    result.Content = JsonConvert.DeserializeObject<T>(content);
            }
        }
        catch (Exception e)
        {
            result = GetErrorResult<T>(e);
        }

        return result;
    }

    public static async Task<HttpClientResult<T>> GetAsync<T>(string url)
    {
    #if DEBUG
        if (!FakeConnectionToInternet)
            return GetErrorResult<T>(new ("No internet connection"));
    #endif

        HttpClientResult<T> result;
        try
        {
            var response = await HttpClient.GetAsync(url);

            result = new HttpClientResult<T>()
            {
                StatusCode = (int)response.StatusCode,
                Succeeded = response.IsSuccessStatusCode,
                StatusMessage = response.ReasonPhrase
            };

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                if (typeof(T) == typeof(string))
                    result.Content = (T)(object)content;
                else
                    result.Content = JsonConvert.DeserializeObject<T>(content);
            }
        }
        catch (Exception e)
        {
            result = GetErrorResult<T>(e);
        }

        return result;
    }
    
    public static async Task<HttpClientResult<Stream>> GetStreamAsync(string url, CancellationToken cancellationToken = default)
    {
        HttpClientResult<Stream> result;
        try
        {
            var response = await HttpClient.GetAsync(url, cancellationToken);
            result = new HttpClientResult<Stream>()
            {
                StatusCode = (int)response.StatusCode,
                Succeeded = response.IsSuccessStatusCode,
                StatusMessage = response.ReasonPhrase
            };

            if (response.IsSuccessStatusCode)
            {
                var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                result.Content = stream;
            }
        }
        catch (Exception e)
        {
            result = GetErrorResult<Stream>(e);
        }

        return result;
    }


    private static HttpClientResult<T> GetErrorResult<T>(Exception e)
    {
        return new HttpClientResult<T>()
        {
            StatusCode = 500,
            StatusMessage = e.Message,
            Succeeded = false
        };
    }

    public static HttpClientResult<T> MockResult<T>(T content)
    {
        return new HttpClientResult<T>()
        {
            StatusCode = 200,
            StatusMessage = "OK",
            Succeeded = true,
            Content = content
        };
    }

    public static HttpClientResult<T> MockResult<T>(string content)
    {
        var result = new HttpClientResult<T>()
        {
            StatusCode = 200,
            StatusMessage = "OK",
            Succeeded = true
        };

        if (typeof(T) == typeof(string))
            result.Content = (T)(object)content;
        else
            result.Content = JsonConvert.DeserializeObject<T>(content);

        return result;
    }
    
    public static void CleanupConnections()
    {
        if (!LazyHttpClient.IsValueCreated) return;
        HttpClient.DefaultRequestHeaders.Clear();
        HttpClient.CancelPendingRequests();
    }
    
    public static void ResetHttpClient()
    {
        if (LazyHttpClient.IsValueCreated)
            HttpClient.Dispose();
        
        // This will force the creation of a new HttpClient on the next use
        new Lazy<HttpClient>(() =>
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("WheelWizard/2.0");
            return client;
        });
    }
}
