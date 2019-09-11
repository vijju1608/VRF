using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JCHVRF.Model.New
{
    [Serializable]
    public class Client : ModelBase
    {
        private int _id;
        public int Id
        {
            get
            {

                return _id;
            }
            set
            {
                this.SetValue(ref _id, value);
            }
        }
        //mandetory
        private string _companyName;
        public string CompanyName
        {
            get
            {

                return _companyName;
            }
            set
            {
                this.SetValue(ref _companyName, value);
            }
        }

        private string _streetAddress;
        public string StreetAddress
        {
            get
            {
                return _streetAddress;
            }
            set
            {
                this.SetValue(ref _streetAddress, value);
            }
        }
        private string _suburb;
        public string Suburb
        {
            get
            {
                return _suburb;
            }
            set
            {
                this.SetValue(ref _suburb, value);
            }
        }

        private string _townCity;
        public string TownCity
        {
            get
            {
                return _townCity;
            }
            set
            {
                this.SetValue(ref _townCity, value);
            }
        }
        //mandetory
        private string _country;
        public string Country
        {
            get
            {
                return _country;
            }
            set
            {
                this.SetValue(ref _country, value);
            }
        }
        private string _gpsPosition;
        public string GpsPosition
        {
            get
            {
                return _gpsPosition;
            }
            set
            {
                this.SetValue(ref _gpsPosition, value);
            }
        }
        //mandetory
        private string _contactName;
        public string ContactName
        {
            get
            {
                return _contactName;
            }
            set
            {
                this.SetValue(ref _contactName, value);
            }
        }
        //mandetory
        private string _phone;
        public string Phone
        {
            get
            {
                return _phone;
            }
            set
            {
                this.SetValue(ref _phone, value);
            }
        }
        //mandetory
        private string _contactEmail;
        public string ContactEmail
        {
            get
            {
                return _contactEmail;
            }
            set
            {
                this.SetValue(ref _contactEmail, value);
            }
        }

        private string _idNumber;
        public string IdNumber
        {
            get
            {
                return _idNumber;
            }
            set
            {
                this.SetValue(ref _idNumber, value);
            }
        }
    }
}
