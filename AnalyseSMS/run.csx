#r "System.Runtime"
#r "Newtonsoft.Json"
#r "Twilio.Api"


using System.Net;
using System.Text;
using Twilio.TwiML;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Dapper;
using System.Data.SqlClient;
using System.Configuration;

 /// <summary>
 /// Azure portal URL.
 /// </summary>
	
 private const string BaseUrl = "https://<LOCATION>.api.cognitive.microsoft.com/";

 /// <summary>
/// Your account key goes here.
 /// </summary>
private const string AccountKey = "ACCOUNT_KEY";

/// <summary>
/// Maximum number of languages to return in language detection API.
 /// </summary>
private const int NumLanguages = 1;
public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    
    log.Info("C# HTTP trigger function processed a request.");

    // STEP ONE: read parameter string into data field 
    //- this will give me all the message info I need (i.e. the number and the message contents)
    var data = await req.Content.ReadAsStringAsync();
    log.Info(data);
    //split data file into dictionary key value pair
    var formValues = data.Split('&')
        .Select(value => value.Split('='))
        .ToDictionary(pair => Uri.UnescapeDataString(pair[0]).Replace("+", " "), 
                      pair => Uri.UnescapeDataString(pair[1]).Replace("+", " "));

    string body = formValues["Body"];
    var number = formValues["From"];


    //STEP TWO - make cognitive API call - awaits the response
    var eventdata = await MakeRequests(log, body);

    //STEP THREE - Use the data  
    // - convert the response into dynamic objects and deserialize the returned JSON from cognitive API
    dynamic e = JsonConvert.DeserializeObject(eventdata);
    // - pull out the sentiment value from the data 
    var sentiment = float.Parse(e.documents[0].score.ToString());

    string s = "";
    // - simple if statement to handle different sentiment levels 
    if (sentiment > 0.75) {
        s = "Great to hear, since you're in a happy mood how would you like to do some online shopping? (" + sentiment+")";
    }
    else if (sentiment < 0.25) {
        s = "Yikes, that's not good. Maybe a discount will lift your spirits? (" + sentiment+")";
    }
    else {
        s = "Thanks for the feedback. Let me know if anything changes. (" + sentiment+")";
    }
    
    log.Info("\nDetect sentiment score: " + sentiment );

    // - Generate a response SMS message
    var response = new MessagingResponse();
    response.Message (s);
    var twiml = response.ToString();
    twiml = twiml.Replace("utf-16", "utf-8");

    //STEP FOUR - Save response in my SQL DB 
    // - SQL as a Service - no backend server involved 
    saveResponse(sentiment, body, number, log);


    //return HttpResponseMessage
    return new HttpResponseMessage
    {
        Content = new StringContent(twiml, Encoding.UTF8, "application/xml")
    };
    
}

 static async Task<string> MakeRequests(TraceWriter log, string body)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseUrl);

                // Request headers.
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", AccountKey);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Request body. Insert text data in JSON format.TODO update
                byte[] byteData = Encoding.UTF8.GetBytes("{\"documents\":[" +
                    "{\"id\":\"1\",\"text\":\""+body+"\"},]}" );

                // Detect sentiment:
                var uri = "text/analytics/v2.0/sentiment";
                var response = await CallEndpoint(client, uri, byteData);
                log.Info("\nDetect sentiment response:\n" + response );
                return response;
            }
        }

static async Task<String> CallEndpoint(HttpClient client, string uri, byte[] byteData)
        {
            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = await client.PostAsync(uri, content);
                return await response.Content.ReadAsStringAsync();
            }
        }


public static Boolean saveResponse(float sentiment, string message, string number, TraceWriter log)
{
    log.Info($"C# HTTP trigger function processed a request. RequestUri={DateTime.Today}");
    message = message.Replace(@"'","''");
    var successful =true;
    try
    {
	///ADD YOUR SQL CONNECTION STRING 
	///SAVE RESPONSE 
      
    }
    catch
    {
        successful=false;
        log.Info("failed to log");
    }
    
    return !successful; 
}
public class LogRequest
{
    public int Id{get;set;}
    public string Log{get;set;}
}

public static byte[] ReadStream(Stream input) {
    byte[] buffer = new byte[16 * 1024];
    using (MemoryStream ms = new MemoryStream()) {
        int read;
        while ((read = input.Read(buffer, 0, buffer.Length)) > 0) {
            ms.Write(buffer, 0, read);
        }
        return ms.ToArray();
    }
}