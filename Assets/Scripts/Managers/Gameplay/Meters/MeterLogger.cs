using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public static class MeterLogger
{
    private static readonly string LoggingDomain = "https://miniraidlogsapi.vercel.app/api/battles";

    private static string FetchMeterToken()
    {
        if (!PlayerPrefs.HasKey("meterToken"))
        {
            string token = Guid.NewGuid().ToString();
            PlayerPrefs.SetString("meterToken", token);
            PlayerPrefs.Save();
        }

        return PlayerPrefs.GetString("meterToken");
    }

    public static async void LogMeters(bool battleWon, int battleDifficulty, List<int> modifierIDs, float battleDuration, Dictionary<string, float> damage, Dictionary<string, float> stagger, Dictionary<string, float> healing)
    {
        string meterToken = FetchMeterToken();

        string json = JsonConvert.SerializeObject(new { meterToken, battleWon, battleDifficulty, modifierIDs, battleDuration, damage, stagger, healing });
        byte[] body = Encoding.UTF8.GetBytes(json);

        using UnityWebRequest request = new(LoggingDomain, "POST");
        request.uploadHandler = new UploadHandlerRaw(body);
        request.SetRequestHeader("Content-Type", "application/json");

        var operation = request.SendWebRequest();
        while (!operation.isDone)
            await System.Threading.Tasks.Task.Yield();

        if (request.result != UnityWebRequest.Result.Success)
            Debug.LogError($"MeterLogger failed: {request.error}");
    }
}