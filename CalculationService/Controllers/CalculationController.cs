using Microsoft.AspNetCore.Mvc;
using RestSharp;
using Monitoring;
using SharedModel;

namespace CalculationService.Controllers;

[ApiController]
[Route("[controller]")]
public class CalculationController : ControllerBase
{
    private static RestClient AddRestClient = new RestClient("http://add-service/");
    private static RestClient SubtractRestClient = new RestClient("http://subtract-service/");
    private static RestClient HistoryRestClient = new RestClient("http://calculation-history-service/");
    
    [HttpGet]
    public double Get(double numberA, double numberB, string calculation)
    {
        using var activity = MonitoringService.ActivitySource.StartActivity();
        MonitoringService.Log.Here().Debug("Entered Calculation method with the {calclation} operator",calculation);
        if (calculation == "Add")
        {
            var result = (double)FetchAdd(numberA, numberB);
            PostCalculation(numberA, numberB, result, calculation);
            return result;
        }
        else
        {
            var result = (double)FetchSubtract(numberA, numberB);
            PostCalculation(numberA, numberB, result, calculation);
            return result;
        }
    }
    [HttpPost]
    public List<Calculation>? Post()
    {
        using var activity = MonitoringService.ActivitySource.StartActivity();
        MonitoringService.Log.Here().Debug("Entered method to call History service to get db data");
        return FetchCalculationHistory();
    }
    
    private static double? FetchSubtract(double numberA, double numberB)
    {
        using var activity = MonitoringService.ActivitySource.StartActivity();
        MonitoringService.Log.Here().Debug("Entered FetchSubtract method");
        var task = SubtractRestClient.GetAsync<double>(new RestRequest($"Subtract?numberA={numberA}&numberB={numberB}"));
        return task.Result;
    }
    
    private static double? FetchAdd(double numberA, double numberB)
    {
        using var activity = MonitoringService.ActivitySource.StartActivity();
        MonitoringService.Log.Here().Debug("Entered FetchAdd method");
        var task = AddRestClient.GetAsync<double>(new RestRequest($"Add?numberA={numberA}&numberB={numberB}"));
        return task.Result;
    }
    
    private static List<Calculation>? FetchCalculationHistory()
    {
        using var activity = MonitoringService.ActivitySource.StartActivity();
        MonitoringService.Log.Here().Debug("Entered FetchCalculationHistory method");
        var task = HistoryRestClient.GetAsync<List<Calculation>>(new RestRequest($"CalculationHistory"));
        return task.Result;
    }
    
    private static void PostCalculation(double numberA, double numberB, double result, string mathOperator)
    {
        using var activity = MonitoringService.ActivitySource.StartActivity();
        MonitoringService.Log.Here().Debug("Entered PostCalculation method");
        HistoryRestClient.PostAsync(new RestRequest($"CalculationHistory?numberA={numberA}&numberB={numberB}&result={result}&mathOperator={mathOperator}"));
    }
}