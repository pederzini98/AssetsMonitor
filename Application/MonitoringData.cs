using Domain.Entities;
using Domain.Enums;
using Domain.Helpers;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Application
{
    public class MonitoringData : IMonitoringData
    {

        private readonly Dictionary<string, AssetFoundFromAPI> emailThrottle = new();
        public async Task FindAssetAsync(Asset assetToLookFor)
        {

            DateTime requestsThrottle = DateTime.UtcNow;
            Actions actions = Actions.NoChanges;
            double? lastValueFound = 0;
            while (true)
            {
                if (requestsThrottle < DateTime.UtcNow)
                {
                    try
                    {

                        string resultFromApi = await SendRequestToaAPIAsync(assetToLookFor); ;
                        if (string.IsNullOrEmpty(resultFromApi) is false)
                        {
                            try
                            {
                                AssetFoundFromAPI? assetFoundFromAPI = DeserializeResponseFromAPI(resultFromApi);

                                if (assetFoundFromAPI is not null && assetFoundFromAPI.CurrentValue is not null)
                                {

                                    if (assetFoundFromAPI?.CurrentValue > assetToLookFor.ValueToBuy)
                                    {
                                        actions = Actions.TimeToBuy;

                                    }
                                    if (assetFoundFromAPI?.CurrentValue > assetToLookFor.ValueToSell)
                                    {
                                        actions = Actions.TimeToSell;
                                    }
                                    if (emailThrottle.ContainsKey(HotSettings.EmailAddress) is false)
                                    {
                                        assetFoundFromAPI.NextEmailUpdate = DateTime.UtcNow;
                                        emailThrottle.TryAdd(HotSettings.EmailAddress, assetFoundFromAPI);
                                    }
                                    if (emailThrottle[HotSettings.EmailAddress].NextEmailUpdate > DateTime.UtcNow
                                        || (lastValueFound == assetFoundFromAPI?.CurrentValue && ValidateNextSendTime(assetFoundFromAPI.NextEmailUpdate) < TimeSpan.FromMinutes(2)))
                                    {
                                        requestsThrottle = DateTime.UtcNow.AddMinutes(ValidateNextSendTime(assetFoundFromAPI.NextEmailUpdate).TotalMinutes);
                                        continue;
                                    }

                                    switch (actions)
                                    {
                                        case Actions.NoChanges:
                                            continue;
                                        case Actions.TimeToSell:
                                        case Actions.TimeToBuy:
                                            var emailToSend = BuildEmail(assetFoundFromAPI?.CurrentValue, assetToLookFor.Name, actions);
                                            NotifyUser.SendNotificationEmail(emailToSend);
                                            requestsThrottle = DateTime.UtcNow.AddMinutes(1);
                                            emailThrottle[HotSettings.EmailAddress].NextEmailUpdate = DateTime.UtcNow.AddMinutes(1);
                                            actions = Actions.NoChanges;
                                            lastValueFound = assetFoundFromAPI?.CurrentValue;
                                            continue;
                                        default:
                                            continue;
                                    }
                                }


                            }
                            catch (Exception e)
                            {
                                string errorr = e.Message;
                            }
                            continue;

                        }
                        Console.WriteLine($"Unable to get the request from the Server. Next retry: {requestsThrottle.ToLocalTime()}");
                    }
                    catch (Exception e)
                    {

                        Console.WriteLine($"Expetion to process Queue {e.Message}");
                    }
                }

            }

            /*     while (true)
                 {
                     await Task.Delay(2000);
                 }*/

        }

        private static async Task<string> SendRequestToaAPIAsync(Asset assetToLookFor)
        {
            try
            {
                using HttpClient httpClient = new();
                using var request = new HttpRequestMessage(new HttpMethod("GET"), $"https://brapi.dev/api/quote/{assetToLookFor.Name}");
                request.Headers.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");
                var response = await httpClient.SendAsync(request);
                return await response.Content.ReadAsStringAsync();

            }
            catch (Exception e)
            {

                return e.Message;

            }
        }

        private static AssetFoundFromAPI DeserializeResponseFromAPI(string response)
        {
            JsonDocument document = JsonDocument.Parse(response);
            JsonElement root = document.RootElement;
            JsonElement valuesOfResponse = root.GetProperty("results");
            string? valuesOfResponseToString = valuesOfResponse.ToString();
            return JsonSerializer.Deserialize<List<AssetFoundFromAPI>>(valuesOfResponseToString).FirstOrDefault();
        }
        private static Email BuildEmail(double? currentValue, string? assetName, Actions actions)
        {
            (string subject, string body) = MountEmailInfos(currentValue, assetName, actions);
            Email email = new(subject, body);
            return email;
        }
        private static (string subject, string body) MountEmailInfos(double? currentValue, string? assetName, Actions actions)
        {
            string subjectStr = string.Empty;
            string body = string.Empty;

            switch (actions)
            {

                case Actions.TimeToSell:
                    subjectStr = HotDefault.SellSubject.Replace("{x}", assetName);
                    body = $"<span style='font-family:trebuchet ms, helvetica, sans-serif;'>Hello Trader!,!<br><br>Temos Novidades sobre o ativo:  <b> {assetName}</b>. O valor atual dele é de  <b>{currentValue} <b/>.  De acordo com as suas notificações é hora de vende-lô";
                    break;
                case Actions.TimeToBuy:
                    subjectStr = HotDefault.BuySubject.Replace("{x}", assetName);
                    body = $"<span style='font-family:trebuchet ms, helvetica, sans-serif;'>Hello Trader!,!<br><br>Temos Novidades sobre o ativo:  <b> {assetName}</b>. O valor atual dele é de  <b>{currentValue} <b/>.  De acordo com as suas notificações é hora de compra-lô";

                    break;
                default:
                    break;
            }
            return (subjectStr, body);
        }

        private TimeSpan ValidateNextSendTime(DateTime? nextSendDate)
        {
            TimeSpan difference = DateTime.UtcNow - emailThrottle[HotSettings.EmailAddress].NextEmailUpdate.Value;
            return difference;
        }
    }
}
