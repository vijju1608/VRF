using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCHVRF.Model.NextGen
{
    public class RegistrationModel
    {
        public int LeftDay { get; set; }
        public int Validflag { get; set; }
        public DateTime RegDate { get; set; }
        public string Region { get; set; }
        public bool SuperUser { get; set; }
        public bool Dealer { get; set; }
        public bool PriceValid { get; set; }
        public string BrandCode { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        public DateTime SyncDate { get; set; }
    }
    public class UserVRF
    {
        public string Username { get; set; }
        public string UserID { get; set; }
        public string UserRegion { get; set; }
        public string UserRole { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        public DateTime SyncDate { get; set; }
    }
    public class UserRACRequest
    {
        public string email { get; set; }
        public string mobileNumber { get; set; }
        public string password { get; set; }
        public string username { get; set; }
    }
    public class RecoveryEmailRACRequest
    {
        public string email { get; set; }
        public string mobileNumber { get; set; }
    }
    public class UserRACResponse
    {
        public string token { get; set; }
        public DateTime SyncDate { get; set; }

    }
   
    public class UserRegionRequest
    {
        public string Username { get; set; } = "Johnson_C@jch.com";


    }
    public class UserRegionResponse
    {
        public UserRegionData data { get; set; }

        public Status status { get; set; }
    }
    public class UserRegionData
    {
        public int id { get; set; }
        public string userName { get; set; }
        public string regionCode { get; set; }
        public string role { get; set; }
    }
    public class Status
    {
        public string success { get; set; }
        public string code { get; set; }
        public string message { get; set; }
        public List<Error> errors { get; set; } = new List<Error>();

        public Status()
        {
                
        }
    }
    public class Error
    {
        public string code { get; set; }
        public string message { get; set; }
    }
    public class CustomExceptionRAC
    {
        public string errorState { get; set; }
        public int attemptLeft { get; set; }

        // public bool IsValid { get; set; }

    }
    public class CustomExceptionVRF
    {
        public string type { get; set; }
        public string desc { get; set; }
        public string stackTrace { get; set; }
        // public bool IsValid { get; set; }

    }
}
