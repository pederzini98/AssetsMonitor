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
            bool forceStop = false;
            while (forceStop is false)
            {
                if (requestsThrottle < DateTime.UtcNow)// Timeout to not flood the api that i'm using
                {
                    try
                    {
                        string resultFromApi = await SendRequestToaAPIAsync(assetToLookFor.Name);

                        if (resultFromApi.Contains("NotFound"))
                        {
                            Console.WriteLine($"Unable to find assets with the name: '{assetToLookFor.Name}'. \nStopping the program");
                            break;
                        }
                        if (string.IsNullOrEmpty(resultFromApi) is false)
                        {
                            try
                            {
                                AssetFoundFromAPI assetFoundFromAPI = DeserializeResponseFromAPI(resultFromApi);

                                if (assetFoundFromAPI is not null && assetFoundFromAPI.CurrentValue is not null)
                                {
                                    switch (assetFoundFromAPI?.CurrentValue)
                                    {
                                        case var value when assetFoundFromAPI?.CurrentValue == lastValueFound:
                                            actions = AssetsAction.NoChanges;
                                            break;
                                        case var value when assetFoundFromAPI?.CurrentValue < assetToLookFor.ValueToBuy:
                                            actions = AssetsAction.TimeToBuy;
                                            break;
                                        case var value when assetFoundFromAPI?.CurrentValue > assetToLookFor.ValueToSell:
                                            actions = AssetsAction.TimeToSell;
                                            break;
                                        default:
                                            break;
                                    }

                                    if (assetFoundFromAPI is not null && emailThrottle.ContainsKey(key: HotSettings.EmailAddress) is false)
                                    {
                                        assetFoundFromAPI.NextEmailUpdate = DateTime.UtcNow;
                                        emailThrottle.TryAdd(HotSettings.EmailAddress, assetFoundFromAPI);
                                    }
                             
                                    switch (actions)
                                    {
                                        case AssetsAction.NoChanges:
                                            requestsThrottle = DateTime.UtcNow.AddMinutes(HotDefault.ApiRequestTimeout);
                                            Console.WriteLine($"Last value got from the api about '{assetToLookFor.Name}': {assetFoundFromAPI?.CurrentValue}'. No changes so no email will be sent");
                                            continue;
                                        case AssetsAction.TimeToSell:
                                        case AssetsAction.TimeToBuy:
                                            var emailToSend = BuildEmail(assetToLookFor, assetFoundFromAPI?.CurrentValue, assetFoundFromAPI?.MaxValue, assetFoundFromAPI?.MinValue, actions);

                                            if (NotifyUser.SendNotificationEmail(emailToSend) is false)
                                            {
                                                forceStop = true; // no point keep running the program due to configuration or input errors
                                                break;
                                            }
                                            Console.WriteLine($"Email sent to {string.Join(",", emailToSend.To)}.If we receive any other change, your next email will be sent at {DateTime.Now.AddMinutes(HotDefault.SendEmailTimeout)}");
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
                                Console.WriteLine($"Unexpected error {e.Message}. Please enter in contact with our support: {HotDefault.SupportLink}");
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

        #region Email builder and data manipulation
        public AssetFoundFromAPI DeserializeResponseFromAPI(string response)
        {
            JsonDocument document = JsonDocument.Parse(response);
            JsonElement root = document.RootElement;
            JsonElement valuesOfResponse = root.GetProperty("results");
            string? valuesOfResponseToString = valuesOfResponse.ToString();
            return JsonSerializer.Deserialize<List<AssetFoundFromAPI>>(valuesOfResponseToString)?.FirstOrDefault() ?? new AssetFoundFromAPI();
        }
        public Email BuildEmail(Asset? asset, double? currentValue, double? maxValue, double? minValue, AssetsAction actions)
        {
            (string subject, string body) = MountEmailInfos(asset, currentValue, maxValue, minValue, actions);
            Email email = new(subject, body);
            return email;
        }
        public (string subject, string body) MountEmailInfos(Asset? asset, double? currentValue, double? maxValue, double? minValue, AssetsAction actions)
        {
            string subjectStr = string.Empty;
            string body = string.Empty;

            switch (actions)
            {
                case AssetsAction.TimeToSell:
                    subjectStr = HotDefault.SellSubject.Replace("{x}", asset?.Name);
                    body = @$"<span style='font-family:trebuchet ms, helvetica, sans-serif;'>Hello Trader!<br><br>Temos Novidades sobre o ativo:  <b> {asset?.Name}</b>. O valor atual é de <b>{currentValue} </b>. De acordo com as suas configurações é hora de vende-lô.<br><br> Valor máximo do ativo até o momento: <b>{(maxValue == 0 ? "Não foi possível resgatar o valor máximo" : maxValue)}</b><br><br>Valor minímo do ativo até o momento: <b>{(minValue == 0 ? "Não foi possível resgatar o valor mínimo" : minValue)}</b></span>";
                    break;
                case AssetsAction.TimeToBuy:
                    subjectStr = HotDefault.BuySubject.Replace("{x}", asset?.Name);
                    body = @$"<span style='font-family:trebuchet ms, helvetica, sans-serif;'>Hello Trader!<br><br>Temos Novidades sobre o ativo:  <b> {asset?.Name}</b>. O valor atual é de <b>{currentValue} </b>. De acordo com as suas configurações é hora de compra-lô.<br><br> Valor máximo do ativo até o momento: <b>{(maxValue == 0 ? "Não foi possível resgatar o valor máximo" : maxValue)}</b><br><br>Valor minímo do ativo até o momento: <b>{(minValue == 0 ? "Não foi possível resgatar o valor mínimo" : minValue)}</b></span>";

                    break;
                default:
                    break;
            }
            return (subjectStr, body);
        }
        #endregion

        #region Look for asset in the api
        public async Task<string> SendRequestToaAPIAsync(string assetToLookFor)
        {
            try
            {
                using HttpClient httpClient = new();
                using var request = new HttpRequestMessage(new HttpMethod("GET"), $"{HotDefault.ApiURl}{assetToLookFor}");
                request.Headers.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");
                var response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode is false)
                {
                    return response.StatusCode.ToString();
                }
                return await response.Content.ReadAsStringAsync();

            }
            catch (Exception e)
            {
                Console.WriteLine($"Unexpected error with our request to the api, wait for the next retry or enter in contact with our support: {HotDefault.SupportLink}\n {e.Message}");
                return string.Empty;

            }
        }

        #endregion

        #region Control actions
        public TimeSpan ValidateNextSendTime(DateTime nextSendDate) //I Believe the ideal time beetwen emails with the same asset value from the previous request should be very higher
        {
            TimeSpan difference = DateTime.UtcNow - nextSendDate;
            return difference;
        }

        #endregion
    }
}
