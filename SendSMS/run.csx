using System;
using Twilio;
 
public static void Run(TimerInfo myTimer, TraceWriter log)
{
    log.Info($"C# Timer trigger function executed at: {DateTime.Now}");    
 
    string accountSid = "AC53406df7ae83f4b8fddf46f4a49f4014";
    string authToken = "394304b76edcfa4ffb149ec761bea4e4";
 
    var client = new TwilioRestClient(accountSid, authToken);
 
        client.SendMessage(
            "+61476856054", // Insert your Twilio from SMS number here
            "+61430054104", // Insert your verified to SMS number here
            "How are you feeling?"          
        );
}