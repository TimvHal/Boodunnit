using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace DefaultNamespace
{
    public class FirebaseHTTPController
    {
        public static readonly string apiKey = "AIzaSyC5lGUMD6hlJPbI7zkcWEU3aV_seCde3Wo";
        public static readonly string username = "drijvern@gmail.com";
        public static readonly string credentials = "B00D3n1t";

        public static UserLoginCredentials GetLogin()
        {
            return new UserLoginCredentials(username, credentials);
        }

        public class UserLoginCredentials
        {
            public string email;
            public string password;
            public bool returnSecureToken = true;

            public UserLoginCredentials(string email, string password)
            {
                this.email = email;
                this.password = password;
            }
        }

        public class LoginResponse
        {
            public string idToken;
            public string email;
            public string refreshToken;
            public string expiresIn;
            public string localId;
        }
    }
}