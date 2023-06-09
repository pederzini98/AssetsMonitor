﻿using System.Text.Json.Serialization;

namespace Domain.Entities
{
    public class Asset
    {
        public Asset(string name, string valueToSell, string valueToBuy)
        {
            Name = name;
            ValueToSell = double.Parse(valueToSell);
            ValueToBuy = double.Parse(valueToBuy);
        }

        public string? Name { get; set; }
        public double? ValueToSell { get; set; }
        public double? ValueToBuy { get; set; }
    }
    public class AssetFoundFromAPI
    {
        [JsonPropertyName("regularMarketPrice")] public double? CurrentValue { get; set; }
        [JsonPropertyName("regularMarketDayLow")] public double? MinValue { get; set; }
        [JsonPropertyName("regularMarketDayHigh")] public double? MaxValue { get; set; }
        [JsonPropertyName("longName")] public string? Name { get; set; }
        [JsonIgnore] public DateTime? NextEmailUpdate { get; set; }
    }
    public class DeserializeAssetObjt
    {
        [JsonPropertyName("results")] public List<AssetFoundFromAPI>? Result { get; set; }
        [JsonPropertyName("requestedAt")] public DateTime DateRequest { get; set; }
    }
}
