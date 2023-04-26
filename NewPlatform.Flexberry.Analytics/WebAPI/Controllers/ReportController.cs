namespace NewPlatform.Flexberry.Analytics.WebAPI
{
    using ICSSoft.STORMNET;
    using NewPlatform.Flexberry.Analytics.Abstractions;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
#if NETFRAMEWORK
    using System.Web.Http;
#endif

#if NETCOREAPP || NETSTANDARD
    using Microsoft.AspNetCore.Mvc;

    using ApiController = Microsoft.AspNetCore.Mvc.ControllerBase;
    using FromUri = Microsoft.AspNetCore.Mvc.FromQueryAttribute;
    using IHttpActionResult = Microsoft.AspNetCore.Mvc.IActionResult;

    [Route("reportapi/[controller]/[action]")]
    [ApiController]
#endif
    public class ReportController : ApiController
    {
        private const string ReportPathParamName = "reportPath";

        private readonly IReportManager ReportManager;

        public ReportController(IReportManager reportManager)
        {
            ReportManager = reportManager;
        }

        /// <summary>
        ///     Получает готовый отчет из сервера отчетов с заданными параметрами.
        /// </summary>
        /// <param name="parameters">Параметры отчёта.</param>
        /// <param name="ct">Маркер отмены.</param>
        [HttpPost]
        [ActionName("getReport")]
        public async Task<IHttpActionResult> GetReportHtml([FromBody] JObject parameters, CancellationToken ct)
        {
            try
            {
                string reportPath = GetParameterValue(parameters, ReportPathParamName);
                parameters.Remove(ReportPathParamName);
                string result = await ReportManager.GetReportHtml(reportPath, parameters, ct);

                // Для NETFRAMEWORK тип OkNegotiatedContentResult, для NETSTANDARD тип OkObjectResult - оба создаются через Ok, для NETCOREAPP тип JsonResult
#if NETFRAMEWORK || NETSTANDARD
                return Ok(result);
#elif NETCOREAPP
                return new JsonResult(result);
#endif
            }
            catch (TaskCanceledException tce)
            {
                LogService.LogInfo("Запрос генерации отчета был отменен", tce);
                return null;
            }
            catch (ArgumentNullException ane)
            {
                LogService.LogError("Ошибка получения параметра", ane);
                return BadRequest(ane.Message);
            }
            catch (Exception e)
            {
                LogService.LogError("Исключение при построении отчёта.", e);
#if NETFRAMEWORK
                return InternalServerError(e);
#elif NETCOREAPP || NETSTANDARD
                throw;
#endif
            }
        }

        /// <summary>
        ///     Экспортирует отчёт.
        /// </summary>
        /// <param name="parameters">Параметры отчёта.</param>
        /// <param name="ct">Маркер отмены.</param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("export")]
        public async Task<IHttpActionResult> ExportReport([FromBody] JObject parameters, CancellationToken ct)
        {
            try
            {
                string reportPath = GetParameterValue(parameters, ReportPathParamName);
                parameters.Remove(ReportPathParamName);
                var reportBytes = await ReportManager.ExportReport(reportPath, parameters, ct);
#if NETFRAMEWORK
                return ResponseMessage(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(reportBytes),
                });
#elif NETCOREAPP || NETSTANDARD
                return new FileContentResult(reportBytes, "application/octet-stream");
#endif
            }
            catch (TaskCanceledException tce)
            {
                LogService.LogInfo("Запрос экспорта отчета был отменен", tce);
                return null;
            }
            catch (ArgumentNullException ane)
            {
                LogService.LogError("Ошибка получения параметра", ane);
                return BadRequest(ane.Message);
            }
            catch (Exception ex)
            {
                LogService.LogError("Исключение при экспорте отчёта.", ex);
#if NETFRAMEWORK
                return InternalServerError(ex);
#elif NETCOREAPP || NETSTANDARD
                throw;
#endif
            }
        }

        /// <summary>
        ///     Получает количество страниц в отчёте.
        /// </summary>
        /// <param name="parameters">Параметры отчёта.</param>
        /// <param name="ct">Маркер отмены.</param>
        [HttpPost]
        [ActionName("getPageCount")]
        public async Task<IHttpActionResult> GetReportPageCount([FromBody] JObject parameters, CancellationToken ct)
        {
            try
            {
                string reportPath = GetParameterValue(parameters, ReportPathParamName);
                parameters.Remove(ReportPathParamName);
                int result = await ReportManager.GetReportPageCount(reportPath, parameters, ct);
                return Ok(result);
            }
            catch (TaskCanceledException tce)
            {
                LogService.LogInfo("Запрос получения количества страниц был отменен", tce);
                return null;
            }
            catch (ArgumentNullException ane)
            {
                LogService.LogError("Ошибка получения параметра", ane);
                return BadRequest(ane.Message);
            }
            catch (Exception e)
            {
                LogService.LogError("Исключение при получении количества страниц отчёта.", e);
#if NETFRAMEWORK
                return InternalServerError(e);
#elif NETCOREAPP || NETSTANDARD
                throw;
#endif
            }
        }

        /// <summary>
        /// Получает значение параметра.
        /// </summary>
        /// <param name="parameters">Список параметров.</param>
        /// <param name="parameterName">Имя параметра.</param>
        /// <returns>Возвращает не пустое значение параметра.</returns>
        private string GetParameterValue(JObject parameters, string parameterName)
        {
            if (string.IsNullOrEmpty(parameterName))
            {
                throw new ArgumentNullException();
            }

            string result = parameters.GetValue(parameterName)?.ToString();
            if (string.IsNullOrEmpty(result))
            {
                throw new ArgumentNullException(parameterName, $"Параметр не должен быть пустым.");
            }

            return result;
        }
    }
}
