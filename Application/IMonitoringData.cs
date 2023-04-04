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
        Email BuildEmail(Asset? asset, double? currentValue, double? maxValue, double? minValue, AssetsAction actions);
        (string subject, string body) MountEmailInfos(Asset? asset, double? currentValue, double? maxValue, double? minValue, AssetsAction actions);

        Task<string> SendRequestToaAPIAsync(string assetToLookFor);
        TimeSpan ValidateNextSendTime(DateTime nextSendDate);
    }
}
