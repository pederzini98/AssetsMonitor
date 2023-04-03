using Domain.Entities;
using Domain.Enums;
using Domain.Helpers;
using Microsoft.VisualStudio.Threading;
using System.Text.Json;

namespace Application
{
    public class MonitoringData : IMonitoringData
    {

        private readonly Dictionary<string, AssetFoundFromAPI> emailThrottle = new();

        #region Main function
        public async Task FindAssetAsync(Asset assetToLookFor)
        {

            DateTime requestsThrottle = DateTime.UtcNow;
            AssetsAction actions = AssetsAction.NoChanges;
            double? lastValueFound = 0;
            while (true)
            {
                if (requestsThrottle < DateTime.UtcNow)// Timeout to not flood the api that i'm using
                {
                    try
                    {
                        string resultFromApi = await SendRequestToaAPIAsync(assetToLookFor); ;
                        if (string.IsNullOrEmpty(resultFromApi) is false)
                        {
                            try
                            {
                                AssetFoundFromAPI assetFoundFromAPI = DeserializeResponseFromAPI(resultFromApi);

                                if (assetFoundFromAPI is not null && assetFoundFromAPI.CurrentValue is not null)
                                {

                                    if (assetFoundFromAPI?.CurrentValue > assetToLookFor.ValueToBuy)
                                    {
                                        actions = AssetsAction.TimeToBuy;
                                    }
                                    if (assetFoundFromAPI?.CurrentValue > assetToLookFor.ValueToSell)
                                    {
                                        actions = AssetsAction.TimeToSell;
                                    }
                                    if (emailThrottle.ContainsKey(key: HotSettings.EmailAddress) is false)
                                    {
                                        assetFoundFromAPI.NextEmailUpdate = DateTime.UtcNow;
                                        emailThrottle.TryAdd(HotSettings.EmailAddress, assetFoundFromAPI);
                                    }
                                    if (emailThrottle[HotSettings.EmailAddress].NextEmailUpdate > DateTime.UtcNow // Timeout to send emails,just to not ended up being blocked by your smtp servert
                                        || (lastValueFound == assetFoundFromAPI?.CurrentValue && 
                                        ValidateNextSendTime(assetFoundFromAPI.NextEmailUpdate.Value) < TimeSpan.FromMinutes(HotDefault.SendEmailTimeout)))
                                    {
                                        requestsThrottle = DateTime.UtcNow.AddMinutes(ValidateNextSendTime(assetFoundFromAPI.NextEmailUpdate.Value).TotalMinutes);
                                        continue;
                                    }

                                    switch (actions)
                                    {
                                        case AssetsAction.NoChanges:
                                            continue;
                                        case AssetsAction.TimeToSell:
                                        case AssetsAction.TimeToBuy:
                                            var emailToSend = BuildEmail(assetFoundFromAPI?.CurrentValue, assetToLookFor.Name, actions);
                                            NotifyUser.SendNotificationEmail(emailToSend);
                                            requestsThrottle = DateTime.UtcNow.AddMinutes(HotDefault.ApiRequestTimeout);
                                            emailThrottle[HotSettings.EmailAddress].NextEmailUpdate = DateTime.UtcNow.AddMinutes(HotDefault.SendEmailTimeout);
                                            actions = AssetsAction.NoChanges;
                                            lastValueFound = assetFoundFromAPI?.CurrentValue;
                                            continue;
                                        default:
                                            continue;
                                    }
                                }
                                requestsThrottle = DateTime.UtcNow.AddMinutes(HotDefault.ApiRequestTimeout);
                                Console.WriteLine($"Unable Desserialize the result from the server, please enter in contact with our support: {HotDefault.SupportLink}");
                                continue;

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

                        Console.WriteLine($"Unexpected error {e.Message}. Please enter in contact with our support: {HotDefault.SupportLink}");
                    }
                }

            }

        }

        #endregion

        #region Email build and data manipulation
        public AssetFoundFromAPI DeserializeResponseFromAPI(string response)
        {
            JsonDocument document = JsonDocument.Parse(response);
            JsonElement root = document.RootElement;
            JsonElement valuesOfResponse = root.GetProperty("results");
            string? valuesOfResponseToString = valuesOfResponse.ToString();
            return JsonSerializer.Deserialize<List<AssetFoundFromAPI>>(valuesOfResponseToString)?.FirstOrDefault() ?? new AssetFoundFromAPI();
        }
        public  Email BuildEmail(double? currentValue, string? assetName, AssetsAction actions)
        {
            (string subject, string body) = MountEmailInfos(currentValue, assetName, actions);
            Email email = new(subject, body);
            return email;
        }
        public  (string subject, string body) MountEmailInfos(double? currentValue, string? assetName, AssetsAction actions)
        {
            string subjectStr = string.Empty;
            string body = string.Empty;

            switch (actions)
            {

                case AssetsAction.TimeToSell:
                    subjectStr = HotDefault.SellSubject.Replace("{x}", assetName);
                    body = $"<span style='font-family:trebuchet ms, helvetica, sans-serif;'>Hello Trader!,!<br><br>Temos Novidades sobre o ativo:  <b> {assetName}</b>. O valor atual dele é de  <b>{currentValue} <b/>.  De acordo com as suas notificações é hora de vende-lô";
                    break;
                case AssetsAction.TimeToBuy:
                    subjectStr = HotDefault.BuySubject.Replace("{x}", assetName);
                    body = $"<span style='font-family:trebuchet ms, helvetica, sans-serif;'>Hello Trader!,!<br><br>Temos Novidades sobre o ativo:  <b> {assetName}</b>. O valor atual dele é de  <b>{currentValue} <b/>.  De acordo com as suas notificações é hora de compra-lô";

                    break;
                default:
                    break;
            }
            return (subjectStr, body);
        }
        #endregion

        #region Send Data
        public  async Task<string> SendRequestToaAPIAsync(Asset assetToLookFor)
        {
            try
            {
                using HttpClient httpClient = new();
                using var request = new HttpRequestMessage(new HttpMethod("GET"), $"{HotDefault.ApiURl}{assetToLookFor.Name}");
                request.Headers.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");
                var response = await httpClient.SendAsync(request);
                return await response.Content.ReadAsStringAsync();

            }
            catch (Exception e)
            {

                return e.Message;

            }
        }

        #endregion

        #region Control actions
        public  TimeSpan ValidateNextSendTime(DateTime nextSendDate)
        {
            TimeSpan difference = DateTime.UtcNow - nextSendDate;
            return difference;
        }

        #endregion
    }
}
