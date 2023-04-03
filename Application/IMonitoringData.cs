using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application
{
    public interface IMonitoringData
    {
        Task FindAssetAsync(Asset assetToLookFor);
        AssetFoundFromAPI DeserializeResponseFromAPI(string response);
        Email BuildEmail(double? currentValue, string? assetName, AssetsAction actions);
        (string subject, string body) MountEmailInfos(double? currentValue, string? assetName, AssetsAction actions);

        Task<string> SendRequestToaAPIAsync(Asset assetToLookFor);
        TimeSpan ValidateNextSendTime(DateTime nextSendDate);
    }
}
