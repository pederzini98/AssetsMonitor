using Domain.Entities;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Application
{
    public class MonitoringData
    {
        NotifyUser _notifyUser { get; }
        public MonitoringData()
        {
            _notifyUser = new();
        }
        public async Task FindAssetAsync(Asset assetToLookFor, NameValueCollection appSettings)
        {
            using var httpClient = new HttpClient();
            Stopwatch nextrequest = new();
            using var request = new HttpRequestMessage(new HttpMethod("GET"), $"https://brapi.dev/api/quote/{assetToLookFor.Name}");
            request.Headers.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");
            while (true)
            {

                var response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    try
                    {

                        AssetFoundFromAPI assetFoundFromAPI = DeserializeResponseFromAPI(result);

                        if (assetFoundFromAPI is not null)
                        {
                            if (assetFoundFromAPI?.CurrentValue > assetToLookFor.ValueToBuy)
                            {
                                (string subject, string body) = MountEmailInfos(assetFoundFromAPI?.CurrentValue, true);
                                Email email = new(body, subject, appSettings);
                                _notifyUser.SendNotificationEmail(email);
                                Console.WriteLine($"Your Next request will be done at {DateTime.UtcNow.AddMinutes(2)}");

                                continue;
                            }
                            if (assetFoundFromAPI?.CurrentValue < assetToLookFor.ValueToSell)
                            {

                                (string subject, string body) = MountEmailInfos(assetFoundFromAPI?.CurrentValue, false);
                                Email email = new(body, subject, appSettings);
                                _notifyUser.SendNotificationEmail(email);
                                Console.WriteLine($"Your Next request will be done at? {DateTime.Now.AddMinutes(2).ToLocalTime()}");

                                continue;
                            }
                        }
                        nextrequest.Reset();
                        nextrequest.Start();
                        while (nextrequest.ElapsedMilliseconds < 120000)
                        {

                        }
                        nextrequest.Stop();
                    }
                    catch (Exception e)
                    {

                        string errorr = e.Message;
                    }

                }

            }

            /*     while (true)
                 {
                     await Task.Delay(2000);
                 }*/

        }

        private static AssetFoundFromAPI DeserializeResponseFromAPI(string response)
        {
            JsonDocument document = JsonDocument.Parse(response);
            JsonElement root = document.RootElement;
            JsonElement valuesOfResponse = root.GetProperty("results");
            string? valuesOfResponseToString = valuesOfResponse.ToString();
            return JsonSerializer.Deserialize<List<AssetFoundFromAPI>>(valuesOfResponseToString).FirstOrDefault();
        }
        private static (string subject, string body) MountEmailInfos(double? currentValue, bool timetoSell)
        {
            throw new NotImplementedException();
        }
    }
}
