using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Amazon;
using Amazon.Auth.AccessControlPolicy;
using Amazon.CognitoIdentity;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.Runtime;
using Newtonsoft.Json;

namespace PixelWorldsBot
{
    public class Requests
    {
        AmazonLambdaClient LambdaClient;
        PixelWorldsBot Bot;
        private CognitoAWSCredentials credentials;

        public LoginInfo loginInfo;

        private readonly string IdentityPoolId = "us-east-1:b9d5be2b-8fae-4bc6-8a4b-e35fab411d76";

        public Requests(PixelWorldsBot _bot) 
        { 
            Bot = _bot;

            UpdateLamda();
        }

        private void UpdateLamda()
        {
            var cfg = new AmazonLambdaConfig();
            cfg.RegionEndpoint = RegionEndpoint.USEast1;
            this.LambdaClient = new AmazonLambdaClient(credentials, cfg);
        }

        
        public void ConnectCognito()
        {
            Bot.logger.LogMessage("Connecting to Cognito and get new ID");
            this.credentials = new CognitoAWSCredentials(this.IdentityPoolId, RegionEndpoint.USEast1);
            this.credentials.GetIdentityIdAsync();
            Bot.logger.LogMessage(credentials.GetIdentityId());

            string coid = credentials.GetIdentityIdAsync().ToString();

            this.loginInfo = new LoginInfo()
            {
                identityId = coid,
                logintoken = coid
            };

        }

        public void RecoverUsernameOrPassword(string email)
        {
            InvokeRequest invokeRequest = new InvokeRequest();
            invokeRequest.FunctionName = "TinyWorlds_LostPassword" + ":prod";
            invokeRequest.Payload = "{\"email\" : \"" + email + "\"}";
            invokeRequest.InvocationType = InvocationType.RequestResponse;
            UpdateLamda();
            var responseObject = this.LambdaClient.InvokeAsync(invokeRequest);
            if (responseObject.Exception != null)
            {
                Bot.logger.LogMessage(responseObject.Exception.ToString());
            }
            string @string = Encoding.ASCII.GetString(responseObject.Result.Payload.ToArray());
            Bot.logger.LogMessage(@string);

            
        }

        public void LoginWithUsernameAndPassword(string username = "", string password = "", bool loginAfterRegistration = false)
        {
            username = username.ToUpper();
            InvokeRequest invokeRequest = new InvokeRequest();
            invokeRequest.FunctionName = "TinyWorlds_LoginWithUserNameAndPassword" + ":prod";
            invokeRequest.Payload = string.Concat(new string[]
            {
                 "{\"username\" : \"",
                 username,
                 "\", \"password\" : \"",
                 password,
                 "\"}"
            });
            invokeRequest.InvocationType = InvocationType.RequestResponse;
            UpdateLamda();

            var responseObject = this.LambdaClient.InvokeAsync(invokeRequest);
            

            if (responseObject.Exception != null)
            {
                Bot.logger.LogMessage("LOGIN FAILED");
                Bot.OnLoginFailed();
                return;
            }
            else
            {
                string @string = Encoding.ASCII.GetString(responseObject.Result.Payload.ToArray());
                //Console.WriteLine(@string);
                //{"login":false}
                if (@string == "{\"login\":false}")
                {
                    Bot.logger.LogMessage("Login failed.");
                    Bot.logger.LogMessage(@string);
                    Bot.OnLoginFailed();
                    return;
                }

                loginInfo = JsonConvert.DeserializeObject<LoginInfo>(@string);
                Bot.logger.LogMessage(loginInfo.GetLoginInfo()) ;
                
            }

        }
 
        public class LoginInfo
        {
            
            public string login { get; set; }

            
            public string identityId { get; set; }

            
            public string token { get; set; }

            
            public string logintoken { get; set; }

            public string GetLoginInfo()
            {
                return $"{identityId};{logintoken}";
            }

        }

    }
}