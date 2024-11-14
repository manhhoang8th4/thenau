using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

public class PaymentService
{
    public async Task<string> CreateMoMoPaymentAsync(decimal amount, string orderInfo, string redirectUrl, string callbackUrl)
    {
        {
            string requestUrl = "http://localhost:3000/api/momo";
            var requestBody = new
            {
                amount = (int)amount,
                orderInfo = orderInfo,
                redirectUrl = redirectUrl,
                callbackUrl = callbackUrl
            };
            var jsonBody = JsonConvert.SerializeObject(requestBody);

            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(requestUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    throw new Exception($"Failed to create MoMo payment. Status code: {response.StatusCode}");
                }
            }
        }
    }
}
public static class HttpRequestHelper
{
    private static readonly HttpClient httpClient = new HttpClient();

    public static async Task<HttpResponse> SendPostRequestAsync(string requestUrl, string requestBody, Dictionary<string, string> headers)
    {
        var response = new HttpResponse();
        try
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
            };

            // Add headers
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    requestMessage.Headers.Add(header.Key, header.Value);
                }
            }

            var httpResponse = await httpClient.SendAsync(requestMessage);
            response.StatusCode = (int)httpResponse.StatusCode;
            response.Body = await httpResponse.Content.ReadAsStringAsync();
        }
        catch (Exception e)
        {
            response.StatusCode = 500;
            response.Body = e.Message;
        }
        return response;
    }

    public static async Task<HttpResponse> SendDeleteRequestAsync(string requestUrl, Dictionary<string, string> headers)
    {
        var response = new HttpResponse();
        try
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Delete, requestUrl);

            // Add headers
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    requestMessage.Headers.Add(header.Key, header.Value);
                }
            }

            var httpResponse = await httpClient.SendAsync(requestMessage);
            response.StatusCode = (int)httpResponse.StatusCode;
            response.Body = await httpResponse.Content.ReadAsStringAsync();
        }
        catch (Exception e)
        {
            response.StatusCode = 500;
            response.Body = e.Message;
        }
        return response;
    }

    public static async Task<HttpResponse> SendGetRequestAsync(string requestUrl, Dictionary<string, string> headers)
    {
        var response = new HttpResponse();
        try
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUrl);

            // Add headers
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    requestMessage.Headers.Add(header.Key, header.Value);
                }
            }

            var httpResponse = await httpClient.SendAsync(requestMessage);
            response.StatusCode = (int)httpResponse.StatusCode;
            response.Body = await httpResponse.Content.ReadAsStringAsync();
        }
        catch (Exception e)
        {
            response.StatusCode = 500;
            response.Body = e.Message;
        }
        return response;
    }

    public static async Task<HttpResponse> SendPatchRequestAsync(string requestUrl, string requestBody, Dictionary<string, string> headers)
    {
        var response = new HttpResponse();
        try
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Put, requestUrl)
            {
                Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
            };

            // Workaround for PATCH using X-HTTP-Method-Override
            requestMessage.Headers.Add("X-HTTP-Method-Override", "PATCH");

            // Add headers
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    requestMessage.Headers.Add(header.Key, header.Value);
                }
            }

            var httpResponse = await httpClient.SendAsync(requestMessage);
            response.StatusCode = (int)httpResponse.StatusCode;
            response.Body = await httpResponse.Content.ReadAsStringAsync();
        }
        catch (Exception e)
        {
            response.StatusCode = 500;
            response.Body = e.Message;
        }
        return response;
    }

    public class HttpResponse
    {
        public int StatusCode { get; set; }
        public string Body { get; set; }
    }
}