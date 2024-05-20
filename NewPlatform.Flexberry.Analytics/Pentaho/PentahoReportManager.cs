﻿namespace NewPlatform.Flexberry.Analytics.Pentaho
{
    using NewPlatform.Flexberry.Analytics.Abstractions;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     Менеджер отчетов для Pentaho.
    /// </summary>
    public class PentahoReportManager : IReportManager
    {
        private readonly HttpClient PentahoHttpClient;

        /// <summary>
        /// Менеджер работы с системой отчетов.
        /// </summary>
        /// <param name="reportServiceEndpoint">Адрес системы отчетов.</param>
        /// <param name="login">Логин для входа.</param>
        /// <param name="password">Пароль для входа.</param>
        /// <param name="timeout">Время ожидания запроса.</param>
        public PentahoReportManager(string reportServiceEndpoint, string login, string password, int timeout = 0)
        {
            if (string.IsNullOrEmpty(reportServiceEndpoint))
            {
                throw new ArgumentNullException("URL-адрес системы отчетов.", "Адрес системы отчетов не может быть пустым.");
            }

            if (string.IsNullOrEmpty(login))
            {
                throw new ArgumentNullException("Логин для подключения к системе отчетов", "Логин для подключения к системе отчетов не может быть пустым.");
            }

            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException("Пароль для подключения к системе отчетов.", "Пароль для подключения к системе отчетов не может быть пустым.");
            }

            PentahoHttpClient = new HttpClient()
            {
                BaseAddress = new Uri(reportServiceEndpoint),
                Timeout = timeout > 0 ? TimeSpan.FromSeconds(timeout) : Timeout.InfiniteTimeSpan
            };

            PentahoHttpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{login}:{password}")));
        }

        /// <inheritdoc />
        public async Task<string> GetReportHtml(string reportPath, JObject parameters, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
            {
                ct.ThrowIfCancellationRequested();
            }

            const string pageNumberKey = "accepted-page";
            if (parameters[pageNumberKey] == null || !int.TryParse(parameters[pageNumberKey].ToString(), out _))
            {
                parameters.Add(pageNumberKey, 0);
            }

            string requestUri = $"/pentaho/api/repos/{reportPath}/generatedContent?" + GetRequestBody(parameters);

            var response = await PentahoHttpClient.GetAsync(requestUri, ct);
            response.EnsureSuccessStatusCode();
            string reportHtml = await response.Content.ReadAsStringAsync();
            string css = await GetReportStyleSheet(reportHtml, ct);

            // Очищаем отчет от относительных ссылок.
            reportHtml = Regex.Replace(reportHtml, "(<link.*>)", "");

            return $"<style>{css}</style>{reportHtml}";
        }

        /// <inheritdoc />
        public async Task<byte[]> ExportReport(string reportPath, JObject parameters, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
            {
                ct.ThrowIfCancellationRequested();
            }

            string contentType = parameters.GetValue("output-target")?.ToString();

            string requestUri = $"/pentaho/api/repos/{reportPath}/generatedContent?" + GetRequestBody(parameters);

            HttpResponseMessage response = await PentahoHttpClient.GetAsync(requestUri, ct);
            response.EnsureSuccessStatusCode();

            byte[] resultData = await GetReportData(contentType, parameters, response);

            return resultData;
        }

        /// <inheritdoc />
        public async Task<int> GetReportPageCount(string reportPath, JObject parameters, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
            {
                ct.ThrowIfCancellationRequested();
            }

            parameters.Add("changedParameters", "null");

            string requestUri = $"/pentaho/api/repos/{reportPath}/parameter";
            string requestBody = GetRequestBody(parameters);

            var content = new StringContent(requestBody, Encoding.UTF8, "application/x-www-form-urlencoded");
            HttpResponseMessage response = await PentahoHttpClient.PostAsync(requestUri, content, ct);
            string reportMetadata = await response.Content.ReadAsStringAsync();

            var regex = new Regex("page-count=\"([0-9]+)\"");
            var match = regex.Match(reportMetadata);

            if (match.Groups.Count > 1)
            {
                return int.Parse(match.Groups[1].Value);
            }

            return -1;
        }

        /// <summary>
        ///     Получить данные отчёта.
        /// </summary>
        /// <param name="contentType">Тип содержимого.</param>
        /// <param name="parameters">Параметры отчёта.</param>
        /// <param name="response">Http-ответ.</param>
        /// <returns>Массив байт с данными отчёта.</returns>
        protected virtual async Task<byte[]> GetReportData(string contentType, JObject parameters, HttpResponseMessage response)
        {
            byte[] resultData;
            switch (contentType)
            {
                case ("table/csv;page-mode=stream"):
                    byte[] bytes = await response.Content.ReadAsByteArrayAsync();
                    resultData = Encoding.Convert(Encoding.UTF8, Encoding.GetEncoding("windows-1251"), bytes);
                    break;

                default:
                    resultData = await response.Content.ReadAsByteArrayAsync();
                    break;
            }

            return resultData;
        }

        /// <summary>
        ///     Получает таблицу стилей для отчёта, извлекая ссылку на нее из разметки отчёта.
        /// </summary>
        /// <param name="reportHtml">Разметка отчёта.</param>
        /// <param name="ct">Маркер отмены.</param>
        private async Task<string> GetReportStyleSheet(string reportHtml, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
            {
                ct.ThrowIfCancellationRequested();
            }

            HttpResponseMessage responseMsg = null;

            var regex = new Regex("<link\\w|\\shref=\"(?=([^\"]*))");
            var match = regex.Match(reportHtml);

            if (match.Groups.Count > 1)
            {
                string cssRequestUri = match.Groups[1].Value;
                responseMsg = await PentahoHttpClient.GetAsync(cssRequestUri, ct);
            }

            return responseMsg == null ?
                string.Empty :
                await responseMsg.Content.ReadAsStringAsync();
        }

        /// <summary>
        ///     Возвращает тело GET-запроса для отправки на сервер Pentaho.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private string GetRequestBody(JObject parameters)
        {
            List<string> keyValuePair = new List<string>();

            foreach (var parameter in parameters)
            {
                JToken paramName = parameter.Value.HasValues ? parameter.Value["paramName"] : parameter.Key;
                JToken paramValue = parameter.Value.HasValues ? parameter.Value["value"] : parameter.Value;

                if (paramValue is JArray)
                {
                    foreach (var arrayItem in paramValue)
                    {
                        JToken arrayItemValue = arrayItem["value"] ?? arrayItem;
                        keyValuePair.Add($"{parameter.Key}={arrayItemValue}");
                    }
                }
                else
                {
                    keyValuePair.Add($"{paramName}={paramValue}");
                }
            }

            return string.Join("&", keyValuePair.ToArray());
        }
    }
}
