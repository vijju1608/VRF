using JCHVRF.Model;
using JCHVRF.Model.NextGen;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JCHVRF.BLL.NextGen
{
    public class Registration
    {
        RegistrationModel sv = null;
        public Registration()
        {
            sv = GetSVRF();
            list = (new RegionBLL()).GetParentRegionList();

        }
        public List<MyRegion> list = null;
        public void DoRegistration(MyRegion myRegion, UserVRF user)
        {
            if (myRegion != null && !string.IsNullOrEmpty(myRegion.RegistPassword))
            {
                RegistrationModel registrationModel = new RegistrationModel();
                DateTime dt = System.DateTime.Now;
                registrationModel.Dealer = false;
                registrationModel.LeftDay = 360;
                registrationModel.PriceValid = false;
                registrationModel.RegDate = dt;
                registrationModel.SuperUser = false;
                registrationModel.Validflag = 0;
                //List<MyRegion> list = (new RegionBLL()).GetParentRegionList();
                if (list == null)
                    return;

                foreach (MyRegion item in list)
                {
                    registrationModel.Region = item.Code;
                    if (myRegion.RegistPassword == item.RegistPassword)
                    {
                        registrationModel.Validflag = 1;
                        if (user.UserRole.Equals("SuperUser", StringComparison.OrdinalIgnoreCase))
                        {
                            registrationModel.SuperUser = true;
                            registrationModel.PriceValid = true;
                            registrationModel.BrandCode = "ALL";
                        }
                        //if (registrationModel.Region == "Super")
                        //{
                        //    registrationModel.SuperUser = true;
                        //    registrationModel.PriceValid = true;
                        //    registrationModel.BrandCode = "ALL";

                        //}
                        registrationModel.Username = user.Username;
                        registrationModel.Password = user.Password;
                        registrationModel.Token = user.Token;
                        registrationModel.SyncDate = user.SyncDate;
                        UpdateValidDate(registrationModel);
                        return;
                    }
                    else if (!string.IsNullOrWhiteSpace(myRegion.YorkPassword) && (myRegion.YorkPassword == item.YorkPassword))
                    {
                        registrationModel.Validflag = 1;
                        registrationModel.BrandCode = "Y";
                        registrationModel.Username = user.Username;
                        registrationModel.Password = user.Password;
                        registrationModel.Token = user.Token;
                        registrationModel.SyncDate = user.SyncDate;
                        UpdateValidDate(registrationModel);
                        return;
                    }
                    else if (!string.IsNullOrWhiteSpace(myRegion.HitachiPassword) && myRegion.HitachiPassword == item.HitachiPassword)
                    {
                        registrationModel.Validflag = 1;
                        registrationModel.BrandCode = "H";
                        registrationModel.Username = user.Username;
                        registrationModel.Password = user.Password;
                        registrationModel.Token = user.Token;
                        registrationModel.SyncDate = user.SyncDate;
                        UpdateValidDate(registrationModel);
                        return;
                    }
                }
            }
        }

        public MyRegion GetRegionVRF(string RegionCode = "ME_A")
        {
            if (list != null && list.Count > 0)
            {
                return list.FirstOrDefault(MM => MM.Code == RegionCode);
            }
            else
            {
                list = (new RegionBLL()).GetParentRegionList();
                return list.FirstOrDefault(MM => MM.Code == RegionCode);

            }
        }
        public bool IsValid(out string ErrorMessage, out CustomExceptionRAC customExceptionRAC)
        {
            customExceptionRAC = null;
            CustomExceptionVRF customExceptionVRF = null;
            bool IsValidUserRole = false;
            sv = GetSVRF();
            if (CheckNet())
            {
                string RACBaseUrl = ConfigurationManager.AppSettings["RACBaseUrl"];
                string RACSignInUrl = "auth/sign-in";
                bool IsValidUser = CheckUserPassword(sv, RACBaseUrl, RACSignInUrl, out ErrorMessage, out customExceptionRAC);
                if (IsValidUser)
                {
                    IsValidUserRole = UserRoleSpecific(sv.Username, sv.Password, out customExceptionVRF);
                    //if (customExceptionVRF != null)//todo
                    //{

                    //}
                }
                return IsValidUser && IsValidUserRole;
            }
            else
            {
                ErrorMessage = "Internet is not available";
                return true;
            }
        }
        public bool CheckUserPassword(RegistrationModel registrationModel, string BaseUrl, string action, out string ErrorMessage, out CustomExceptionRAC customException)
        {
            customException = null;
            ErrorMessage = string.Empty;
            if (!string.IsNullOrWhiteSpace(registrationModel.Username) && !string.IsNullOrWhiteSpace(registrationModel.Password))
            {
                UserRACRequest user = new UserRACRequest { email = registrationModel.Username, password = registrationModel.Password };
                UserRACResponse userResponse = new UserRACResponse();
                if (Registration.CheckNet())
                {
                    userResponse = Registration.HttpClientRAC<UserRACRequest, UserRACResponse>(user, BaseUrl, action, out customException);
                    if (userResponse != null && !string.IsNullOrEmpty(userResponse.token))
                    {

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    ErrorMessage = "Internet is not available";
                    return true;
                }
            }
            else
            {
                return false;
            }

        }
        public bool IsSuperUser()
        {
            sv = GetSVRF();
            if (sv != null && sv.SuperUser)
            {
                return true;
            }
            return false;
        }

        public string GetRegionCode()
        {
            sv = GetSVRF();
            if (sv != null && !sv.SuperUser)
            {
                return sv.Region;
            }
            return "";
        }


        public void UpdateValidDate(RegistrationModel registrationModel)
        {
            string FileName = "sv.dat";
            string FilePath = AppDomain.CurrentDomain.BaseDirectory + FileName;
            if (File.Exists(FilePath))
            {
                File.WriteAllText(FilePath, String.Empty);
                using (FileStream fs = File.Open(FilePath, FileMode.Open, FileAccess.ReadWrite))
                {
                    string TextData = getRegistrationInformation(registrationModel);
                    Byte[] info = new UTF8Encoding(true).GetBytes(TextData);
                    fs.Write(info, 0, info.Length);
                }
            }
            else
            {
                using (FileStream fs = File.Create(FilePath))
                {
                    string TextData = getRegistrationInformation(registrationModel);
                    Byte[] info = new UTF8Encoding(true).GetBytes(TextData);
                    fs.Write(info, 0, info.Length);
                }
            }
        }
        public string getRegistrationInformation(RegistrationModel registrationModel)
        {
            Type myType = registrationModel.GetType();
            IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties());
            string ObjectData = string.Empty;
            foreach (PropertyInfo prop in props)
            {
                object propValue = prop.GetValue(registrationModel, null);
                if (!string.IsNullOrWhiteSpace(ObjectData))
                    ObjectData = ObjectData + "|" + Encrypt(prop.Name + "@" + Convert.ToString(propValue));
                else
                    ObjectData = Encrypt(prop.Name + "@" + propValue.ToString());
            }
            return ObjectData;
        }
        public RegistrationModel GetSVRF()
        {
            string FileName = "sv.dat";
            RegistrationModel svreturn = new RegistrationModel();
            string FilePath = AppDomain.CurrentDomain.BaseDirectory + FileName;
            if (File.Exists(FilePath))
            {
                string ReturnObjectValue = "";
                using (StreamReader sr = File.OpenText(FilePath))
                {
                    string ReadLine = string.Empty;
                    while ((ReadLine = sr.ReadLine()) != null)
                    {
                        ReturnObjectValue = ReturnObjectValue + ReadLine;
                    }
                }
                string[] reg = ReturnObjectValue.Split('|');
                foreach (var item in reg)
                {
                    if (!string.IsNullOrWhiteSpace(item))
                    {
                        string DecryptedData = Decrypt(item);
                        string[] str = DecryptedData.Split("@".ToArray(), 2);
                        if (str.Length == 2)
                        {
                            setRegistrationInformation(svreturn, str[0], str[1]);
                        }
                    }
                }
            }
            //else
            //{
            //    RegistrationModel sv = new RegistrationModel();
            //    DateTime dt = System.DateTime.Now;
            //    sv.Dealer = false;
            //    sv.LeftDay = 360;
            //    sv.PriceValid = false;
            //    sv.RegDate = dt;
            //    sv.SuperUser = false;
            //    sv.Validflag = 0;

            //    using (FileStream fs = File.Create(FilePath))
            //    {
            //        string TextData = getRegistrationInformation(sv);
            //        Byte[] info = new UTF8Encoding(true).GetBytes(TextData);
            //        fs.Write(info, 0, info.Length);
            //    }
            //}
            return svreturn;
        }
        public string setRegistrationInformation(RegistrationModel registrationModel, string propName, string propValue)
        {
            Type myType = registrationModel.GetType();
            IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties());
            string ObjectData = string.Empty;
            foreach (PropertyInfo prop in props)
            {
                if (prop.Name.Equals(propName, StringComparison.OrdinalIgnoreCase))
                {
                    prop.SetValue(registrationModel, Convert.ChangeType(propValue, prop.PropertyType));
                    break;
                }
                else
                    continue;

            }
            return ObjectData;
        }


        [System.Runtime.InteropServices.DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int Description, int ReservedValue);
        public static bool CheckNet()
        {
            int desc;
            return InternetGetConnectedState(out desc, 0);
        }
        public static string HttpClientReqHeader<T>(T request, string BaseAddress, string Uri, string RequestType = "POST")
        {
            string AuthorizationToken = string.Empty;
            var result = HttpResponse<T>(request, BaseAddress, Uri, RequestType);
            if (result != null)
            {
                if (result.IsSuccessStatusCode)
                {
                    if (result.Headers.Contains("Authorization"))
                    {
                        AuthorizationToken = result.Headers.GetValues("Authorization").FirstOrDefault().ToString();
                    }
                }
            }
            return AuthorizationToken;
        }

        public static R HttpClientRAC<T, R>(T request, string BaseAddress, string Uri, out CustomExceptionRAC customException, string RequestType = "POST")
        {
            customException = null;
            try
            {
                var result = HttpResponse<T>(request, BaseAddress, Uri, RequestType);
                if (result != null)
                {
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsAsync<R>();
                        readTask.Wait();
                        // R user = readTask.Result;
                        return readTask.Result;
                    }
                    else
                    {
                        customException = new CustomExceptionRAC();
                        try
                        {
                            var readTask = result.Content.ReadAsAsync<CustomExceptionRAC>();
                            readTask.Wait();
                            var Exception = readTask.Result;
                            customException = Exception as CustomExceptionRAC;
                            // customException.IsValid = true;
                        }
                        catch (Exception Ex)
                        {
                            customException.errorState = result.ReasonPhrase;
                            //customException.IsValid = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                customException = new CustomExceptionRAC();
                customException.errorState = ex != null ? (ex.InnerException != null ? ex.InnerException.Message : ex.Message) : "Some thing unexpected happen";

            }
            return default(R);
        }

        public static R HttpClientJCH<T, R>(T request, string BaseAddress, string Uri, out CustomExceptionVRF customException, string RequestType = "POST")
        {
            customException = null;
            try
            {
                var result = HttpResponse<T>(request, BaseAddress, Uri, RequestType);
                if (result != null)
                {
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsAsync<R>();
                        readTask.Wait();
                        // R user = readTask.Result;
                        return readTask.Result;
                    }
                    else if (result.StatusCode.Equals(HttpStatusCode.NotFound))
                    {
                        customException = new CustomExceptionVRF();
                        //try
                        //{
                        customException.desc = "The user login is not mapped to the role";
                        customException.stackTrace = result.ReasonPhrase;
                        // customException.IsValid = true;
                        //}
                        //catch (Exception Ex)
                        //{
                        //    customException.stackTrace = result.ReasonPhrase;
                        //    //customException.IsValid = false;
                        //}

                    }
                    else
                    {
                        customException = new CustomExceptionVRF();
                        try
                        {
                            var readTask = result.Content.ReadAsAsync<CustomExceptionVRF>();
                            readTask.Wait();
                            var Exception = readTask.Result;
                            customException = Exception as CustomExceptionVRF;
                            // customException.IsValid = true;
                        }
                        catch (Exception Ex)
                        {
                            customException.stackTrace = result.ReasonPhrase;
                            //customException.IsValid = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                customException = new CustomExceptionVRF();
                customException.stackTrace = ex != null ? (ex.InnerException != null ? ex.InnerException.Message : ex.Message) : "Some thing unexpected happen";

            }
            return default(R);
        }

        public static bool HttpClient<T>(T request, string BaseAddress, string Uri, out CustomExceptionRAC customException, string RequestType = "POST")
        {
            bool ReturnValue = false;
            customException = null;
            try
            {
                var result = HttpResponse<T>(request, BaseAddress, Uri, RequestType);
                if (result != null)
                {
                    if (result.IsSuccessStatusCode)
                    {
                        ReturnValue= true;
                    }
                    else if (result.StatusCode.Equals(HttpStatusCode.NotFound))
                    {
                        ReturnValue= false;
                    }
                    else
                    {
                        customException = new CustomExceptionRAC();
                        try
                        {
                            var readTask = result.Content.ReadAsAsync<CustomExceptionRAC>();
                            readTask.Wait();
                            var Exception = readTask.Result;
                            customException = Exception as CustomExceptionRAC;
                            ReturnValue = false;
                            // customException.IsValid = true;
                        }
                        catch (Exception Ex)
                        {
                            ReturnValue = false;
                            customException.errorState = result.ReasonPhrase;
                            //customException.IsValid = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                customException = new CustomExceptionRAC();
                customException.errorState = ex != null ? (ex.InnerException != null ? ex.InnerException.Message : ex.Message) : "Some thing unexpected happen";
                ReturnValue = false;

            }
            return ReturnValue;
        }

        static HttpResponseMessage HttpResponse<T>(T request, string BaseAddress, string Uri, string RequestType)
        {
            using (var client = new HttpClient())
            {
                //BaseAddress = "https://vrf.azurewebsites.net/";
                //Uri = "api/login/authenticate";
                client.BaseAddress = new Uri(BaseAddress);
                dynamic Result = null;

                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                switch (RequestType)
                {
                    case "POST":
                        var responseTask = client.PostAsJsonAsync<T>(Uri, request);
                        responseTask.Wait();
                        Result = responseTask.Result;
                        break;
                    case "PUT":
                        responseTask = client.PutAsJsonAsync<T>(Uri, request);
                        responseTask.Wait();
                        Result = responseTask.Result;
                        break;
                    case "DELETE":
                        responseTask = client.DeleteAsync(Uri);
                        responseTask.Wait();
                        Result = responseTask.Result;
                        break;
                    case "GET":
                        responseTask = client.GetAsync(Uri);
                        responseTask.Wait();
                        Result = responseTask.Result;
                        break;
                    default:
                        break;
                }
                if (Result != null)
                    return Result;
                else
                    return null;
            }
        }
        public static string Encrypt(string encryptValue)
        {
            string key = "12345678912345678912345678912345";
            string Indicator = "mdhyadhsddmyghjq";
            byte[] password = Encoding.ASCII.GetBytes(encryptValue);
            byte[] Key = Encoding.ASCII.GetBytes(key);
            byte[] IV = Encoding.ASCII.GetBytes(Indicator);
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            System.Security.Cryptography.Rijndael alg = System.Security.Cryptography.Rijndael.Create();
            alg.Key = Key;
            alg.IV = IV;
            System.Security.Cryptography.CryptoStream cs = new System.Security.Cryptography.CryptoStream(ms,
            alg.CreateEncryptor(), System.Security.Cryptography.CryptoStreamMode.Write);
            cs.Write(password, 0, password.Length);
            cs.Close();
            byte[] encryptedData = ms.ToArray();
            string str = null;
            //convert byte into comma seprate string
            for (int i = 0; i < encryptedData.Length; i++)
            {
                str = str + encryptedData[i] + '.';
            }
            //trim , at the end
            str = str.TrimEnd('.');
            return str;
        }
        public static string Decrypt(string decryptValue)
        {
            string[] csbytes = decryptValue.Split('.');
            byte[] temp = new byte[csbytes.Length];
            StringBuilder sUC = new StringBuilder();
            for (int ictr = 0; ictr < csbytes.Length; ictr++)
            {
                temp[ictr] = Convert.ToByte(csbytes[ictr].ToString());
            }
            string key = "12345678912345678912345678912345";
            string Indicator = "mdhyadhsddmyghjq";
            byte[] Key = Encoding.ASCII.GetBytes(key);
            byte[] IV = Encoding.ASCII.GetBytes(Indicator);

            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            System.Security.Cryptography.Rijndael alg = System.Security.Cryptography.Rijndael.Create();
            alg.Key = Key;
            alg.IV = IV;
            System.Security.Cryptography.CryptoStream cs = new System.Security.Cryptography.CryptoStream(ms, alg.CreateDecryptor(), System.Security.Cryptography.CryptoStreamMode.Write);
            cs.Write(temp, 0, temp.Length);
            cs.Close();
            byte[] decryptedData = ms.ToArray();
            string org;
            org = (Encoding.UTF8.GetString(decryptedData));
            return org;
        }
        public bool UserRoleSpecific(string email, string password, out CustomExceptionVRF customExceptionVRF)
        {
            bool IsValid = false;
            customExceptionVRF = null;
            string ErrorMessage = string.Empty;
            UserRegionRequest userRegionRequest = new UserRegionRequest
            {
                // Username = email
                Username = "Johnson_C@jch.com"
            };//TODO
            UserRegionResponse userRegionResponse = GetVRFUserData(userRegionRequest, out customExceptionVRF, out ErrorMessage);//VRF WebApi
            if (userRegionResponse != null && customExceptionVRF == null)
            {
                if (string.IsNullOrWhiteSpace(ErrorMessage))
                {
                    UserVRF userVrf = new UserVRF { Username = email, UserRole = userRegionResponse.data.role, UserRegion = userRegionResponse.data.regionCode, Password = password, SyncDate = DateTime.Now };
                    MyRegion myRegion = GetRegionVRF(userVrf.UserRegion);
                    DoRegistration(myRegion, userVrf);
                    IsValid = true;
                }
            }
            return IsValid;
        }
        public UserRegionResponse GetVRFUserData(UserRegionRequest userRegion, out CustomExceptionVRF customExceptionVRF, out string ErrorMessage)
        {
            customExceptionVRF = null;
            ErrorMessage = string.Empty;
            UserRegionResponse userRegionResponse = null;
            if (Registration.CheckNet())
            {
                string VRFBaseUrl = ConfigurationManager.AppSettings["VRFBaseUrl"];
                string VrfUserDataActionUrl = "api/user/{0}";
                userRegionResponse = Registration.HttpClientJCH<UserRegionRequest, UserRegionResponse>(userRegion, VRFBaseUrl, string.Format(VrfUserDataActionUrl, userRegion.Username), out customExceptionVRF, "GET");
            }
            else
            {
                ErrorMessage = "Internet is not available";
            }
            return userRegionResponse;
        }
        public UserRACResponse UserLoginJCHAuth(UserRACRequest user, out string ErrorMessage, out CustomExceptionRAC customException)
        {
            customException = null;
            ErrorMessage = string.Empty;
            UserRACResponse userResponse = new UserRACResponse();
            if (Registration.CheckNet())
            {
                string RACBaseUrl = ConfigurationManager.AppSettings["RACBaseUrl"];
                string RACSignInUrl = "auth/sign-in";
                userResponse = Registration.HttpClientRAC<UserRACRequest, UserRACResponse>(user, RACBaseUrl, RACSignInUrl, out customException);
                if (customException != null)
                {
                    return null;
                }

            }
            else
            {
                ErrorMessage = "Internet is not available";
            }
            return userResponse;

        }

        public bool RecoveryEmailJCH(RecoveryEmailRACRequest recoveryEmailRACRequest, out string ErrorMessage, out CustomExceptionRAC customException)
        {
            customException = null;
            ErrorMessage = string.Empty;
            bool recoveryEmailRACResponse = false;
            if (Registration.CheckNet())
            {
                string RACBaseUrl = ConfigurationManager.AppSettings["RACBaseUrl"];
                string RACSignInUrl = "account/forgot-password";
                recoveryEmailRACResponse = Registration.HttpClient<RecoveryEmailRACRequest>(recoveryEmailRACRequest, RACBaseUrl, RACSignInUrl, out customException);
                if (customException != null)
                {
                    return false;
                }

            }
            else
            {
                ErrorMessage = "Internet is not available";
            }
            return recoveryEmailRACResponse;

        }
    }
}
