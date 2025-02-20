using System.Diagnostics;
using Newtonsoft.Json.Linq;

class Program
{
    private const string INGV_URL = "https://webservices.ingv.it/fdsnws/event/1/query?format=geojson&limit=10";
    private const string TELEGRAM_BOT_TOKEN = "8004142014:AAGQzhUAPLnpkAHi-d_2dCu1Or1OS7_QZhM";
    private const string TELEGRAM_CHAT_ID = "-1002304241907";
    private static readonly int TIMEOUT_VALUE = 1;
    private static readonly HttpClient httpClient = new HttpClient();

    private static long _latestEventSended = 0;



    static async Task Main()
    {
        Console.WriteLine("Hello, I'm here!");

        while (true)
        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(INGV_URL);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Errore: {response.StatusCode} {response.ReasonPhrase}");
                    continue;
                }

                string responseBody = await response.Content.ReadAsStringAsync();
                JObject data = JObject.Parse(responseBody);

                if (data["features"] != null)
                {
                    var earthquakelist = data["features"]
                                        .OrderBy(earthquake => (long)earthquake["properties"]["eventId"])
                                        .ToList();
                    foreach (var earthquake in earthquakelist)
                    {
                        var earthquakeProperties = earthquake["properties"];
                        var earthquakeGeometry = earthquake["geometry"];
                        var eventDate = DateTime.Parse(
                            (string)earthquakeProperties["time"]!,
                            null,
                            System.Globalization.DateTimeStyles.RoundtripKind
                        );
                        long eventId = (long)earthquakeProperties["eventId"];
                        Console.WriteLine(eventId);
                        if (eventId > _latestEventSended)
                        {
                            _latestEventSended = eventId;
                            double magnitude = (double)earthquakeProperties["mag"];
                            double depth = (double)earthquakeGeometry["coordinates"][2];
                            double latitude = (double)earthquakeGeometry["coordinates"][1];
                            double longitude = (double)earthquakeGeometry["coordinates"][0];
                            string locationInfo = (string)earthquakeProperties["place"];

                            string message = $"\U0001F30D Location: {locationInfo}\n" +
                                             $"\U0001F4CF Magnitude: {magnitude}\n" +
                                             $"\U0001F53D Depth: {depth} km\n" +
                                             $"\U0001F4CD Geo: Lat {latitude}, Lon {longitude}";

                            Console.WriteLine($"====================================\n{message}\n====================================");
                            await SendTelegramMessage(message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eccezione: {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromMinutes(TIMEOUT_VALUE));
        }
    }

    private static async Task SendTelegramMessage(string message)
    {
        try
        {
            string telegramUrl = $"https://api.telegram.org/bot{TELEGRAM_BOT_TOKEN}/sendMessage" +
                                 $"?chat_id={TELEGRAM_CHAT_ID}&text={Uri.EscapeDataString(message)}";

            HttpResponseMessage response = await httpClient.GetAsync(telegramUrl);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Errore nell'invio del messaggio Telegram: " + await response.Content.ReadAsStringAsync());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Eccezione nell'invio del messaggio Telegram: " + ex.Message);
        }
    }
}