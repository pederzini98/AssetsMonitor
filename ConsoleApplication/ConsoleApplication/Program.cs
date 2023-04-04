﻿using Application;
using Domain.Entities;
using Domain.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.Threading;
using System.Configuration;

namespace ConsoleApplication
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Please enter all the required arguments separeted by space: Ex: Petra4 20,2 30,3");
                return;
            }
            try
            {

                Asset asset = new(args[0], args[1], args[2]);
                if (asset is null || string.IsNullOrEmpty(asset.Name))
                {
                    Console.WriteLine("Name can't be Empty");
                    return;

                }
                if (asset.ValueToBuy < 0 || asset.ValueToSell < 0)
                {
                    Console.WriteLine("Can't use negative values to get a stock");
                    return;

                }
                if (asset.ValueToBuy > asset.ValueToSell)
                {
                    Console.WriteLine("You choosed a higher buy value than the one to sell, i won't let you loose money >.<");
                    return;
                }
                MonitoringData monitoringData = new();
                HotSettings.StartValue(ConfigurationManager.AppSettings);

                await monitoringData.FindAssetAsync(asset);

            }
            catch (Exception e)
            {
                if (e.Message.Contains("was not in a correct format."))
                {

                    Console.WriteLine($"Please enter valid data to check the stock price. " +
                        $"\nValue to sell (must be a positive number): {args[1]} \nValue to sell (must be a positive number): " +
                        $"{args[2]}.\nThis error can be caused by a wrong 'port' in your config\nPress enter to leave");
                }

            }
            finally
            {
                Console.ReadLine();

            }
            return;


        }
    }
}