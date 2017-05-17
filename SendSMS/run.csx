using System;
using Twilio;
 
public static void Run(TimerInfo myTimer, TraceWriter log)
{
    log.Info($"C# Timer trigger function executed at: {DateTime.Now}");    
 
    string accountSid = "YOUR_ACCOUNT_ID";
    string authToken = "YOUR_AUTH_ID";
 
    var client = new TwilioRestClient(accountSid, authToken);
 
        client.SendMessage(
            "+61........", // Insert your Twilio from SMS number here
            "+61........", // Insert your verified to SMS number here
            "How are you feeling?"  //customise your message         
        );
}