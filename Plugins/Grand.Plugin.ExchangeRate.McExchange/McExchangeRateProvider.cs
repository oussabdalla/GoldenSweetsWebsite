﻿using Grand.Core;
using Grand.Core.Plugins;
using Grand.Services.Directory;
using Grand.Services.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Xml;

namespace Grand.Plugin.ExchangeRate.EcbExchange
{
    public class EcbExchangeRateProvider : BasePlugin, IExchangeRateProvider
    {
        private readonly ILocalizationService _localizationService;
        private Dictionary<string, decimal> currencies;

        public EcbExchangeRateProvider(ILocalizationService localizationService)
        {
            this._localizationService = localizationService;
            currencies = new Dictionary<string, decimal>
            {
                {"USD", 1},
                {"EUR", 1},
                {"TND", 1}
            };
        }

        /// <summary>
        /// Gets currency live rates
        /// </summary>
        /// <param name="exchangeRateCurrencyCode">Exchange rate currency code</param>
        /// <returns>Exchange rates</returns>
        public  IList<Core.Domain.Directory.ExchangeRate> GetCurrencyLiveRates(string exchangeRateCurrencyCode)
        {
            //if (String.IsNullOrEmpty(exchangeRateCurrencyCode) ||
            //    exchangeRateCurrencyCode.ToLower() != "eur")
            //    throw new GrandException(_localizationService.GetResource("Plugins.ExchangeRate.EcbExchange.SetCurrencyToEURO"));

            //var exchangeRates = new List<Grand.Core.Domain.Directory.ExchangeRate>();

            //var request = WebRequest.Create("http://www.ecb.int/stats/eurofxref/eurofxref-daily.xml") as HttpWebRequest;
            //using (WebResponse response = request.GetResponse())
            //{
            //    var document = new XmlDocument();
            //    document.Load(response.GetResponseStream());
            //    var nsmgr = new XmlNamespaceManager(document.NameTable);
            //    nsmgr.AddNamespace("ns", "http://www.ecb.int/vocabulary/2002-08-01/eurofxref");
            //    nsmgr.AddNamespace("gesmes", "http://www.gesmes.org/xml/2002-08-01");

            //    var node = document.SelectSingleNode("gesmes:Envelope/ns:Cube/ns:Cube", nsmgr);
            //    var updateDate = DateTime.ParseExact(node.Attributes["time"].Value, "yyyy-MM-dd", null);

            //    var provider = new NumberFormatInfo();
            //    provider.NumberDecimalSeparator = ".";
            //    provider.NumberGroupSeparator = "";
            //    foreach (XmlNode node2 in node.ChildNodes)
            //    {
            //        exchangeRates.Add(new Core.Domain.Directory.ExchangeRate()
            //        {
            //            CurrencyCode = node2.Attributes["currency"].Value,
            //            Rate = decimal.Parse(node2.Attributes["rate"].Value, provider),
            //            UpdatedOn = updateDate
            //        }
            //        );
            //    }
            //}

            // Convert Currency By Consuming Restful API 

            

            if (String.IsNullOrEmpty(exchangeRateCurrencyCode))
                throw new GrandException("Plugins.ExchangeRate.EcbExchange IsNull Or Empty");

            // preparing the currency list

            currencies = new Dictionary<string, decimal>
            {
                {"USD", 1},
                {"EUR", 1},
                {"TND", 1}
            };
            GetCurrency(exchangeRateCurrencyCode);

            var exchangeRates = new List<Grand.Core.Domain.Directory.ExchangeRate>();

            var updateDate = DateTime.Now;

            foreach (KeyValuePair<string, decimal> kvp in currencies)
            {

                exchangeRates.Add(new Core.Domain.Directory.ExchangeRate()
                {
                    CurrencyCode = kvp.Key,
                    Rate = kvp.Value,
                    UpdatedOn = updateDate
                }
                );

            }


            return exchangeRates;
        }
        public async void GetCurrency(string primaryCurrency)
        {
            
            using (var client = new HttpClient())
            {
                Dictionary<string, decimal> seccurrencies = new Dictionary<string, decimal>()
                {
                    {"USD", 1},
                    {"EUR", 1},
                    {"TND", 1}
                };
                 foreach (KeyValuePair<string, decimal> kvp in seccurrencies)
                {
                    string path = "https://free.currencyconverterapi.com/api/v6/convert?q=" + primaryCurrency + "_" + kvp.Key + "&compact=ultra&apiKey=122a3376cb5587975411";

                    var response = client.GetAsync(path).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var stringresult = response.Content.ReadAsStringAsync().Result;
                        dynamic currency = JsonConvert.DeserializeObject(stringresult);
                        currencies[kvp.Key] = currency[primaryCurrency + "_" + kvp.Key];
                    }
                }
            }



            //HttpClient client = new HttpClient();

            //client.BaseAddress = new Uri("https://free.currencyconverterapi.com/api/v6/convert?q=");
            ////client.DefaultRequestHeaders.Accept.Add(
            ////    new MediaTypeWithQualityHeaderValue("application/json"));

            //foreach (KeyValuePair<string, decimal> kvp in currencies)
            //{

            //    HttpResponseMessage response2 = await client.GetAsync(primaryCurrency + "_" + kvp.Key+ "&compact=ultra&apiKey=122a3376cb5587975411");
            //    if (response2.IsSuccessStatusCode)
            //    {
            //        var stringresult =  response2.Content.ReadAsStringAsync();
            //        KeyValuePair<string, decimal> currency = JsonConvert.DeserializeObject<KeyValuePair<string, decimal>>(stringresult);
            //        currencies[kvp.Key] = currency.Value;
            //    }
            //}

        }
        public override void Install()
        {
            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.ExchangeRate.EcbExchange.SetCurrencyToEURO", "You can use ECB (European central bank) exchange rate provider only when the primary exchange rate currency is set to EURO");
            base.Install();
        }

        public override void Uninstall()
        {
            //locales
            this.DeletePluginLocaleResource("Plugins.ExchangeRate.EcbExchange.SetCurrencyToEURO");
            base.Uninstall();
        }
    }
}