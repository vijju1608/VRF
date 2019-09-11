using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JCHVRF_New.Model;
using Lassalle.WPF.Flow;

namespace JCHVRF.Model
{
    [Serializable]
    public class SystemBase : ModelBase
    {

        private const string _imageRelativePath = @"/JCHVRF_New;component/Image/";

        // ACC - SKM - 1211  START
        public List<string> _errors = new List<string>();

        public List<string> Errors
        {
            get { return _errors; }
        }
        // ACC - SKM - 1211  END
        public SystemBase() 
        {
            this.Id = Guid.NewGuid().ToString("N");
        }
        /// <summary>
        /// 唯一编号
        /// </summary>
        public string Id { get; private set; }

        private string _name;
        /// <summary>
        /// 系统名 Out1...
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { this.SetValue(ref _name, value); }
        }

        public string HvacSystemType { get; set; }

        private SystemStatus _systemStatus=SystemStatus.WIP;
        public SystemStatus SystemStatus {get { return _systemStatus; }
            set
            {
                this.SetValue(ref _systemStatus, value);
                StatusIcon = GetSystemStatusImg();
            } }

        private string _statusIcon;
        public string StatusIcon
        {
            get
            {
                if (string.IsNullOrEmpty(_statusIcon))
                {
                    _statusIcon = GetSystemStatusImg();}
                return _statusIcon;
            }

            set { this.SetValue(ref _statusIcon, value);  }
        }

        public string SystemState { get; set; }

        [NonSerialized]
        private List<Item> _unsavedItems;
        public List<Item> UnSavedDrawing
        {
            get { return _unsavedItems;}

            set
            {
                _unsavedItems = value;

            }
        }

        private string GetSystemStatusImg()
        {
            switch (SystemStatus)
            {
                case SystemStatus.WIP:
                    return _imageRelativePath + "Path 111.png";

                case SystemStatus.VALID:
                    return _imageRelativePath + "Path 113.png";

                case SystemStatus.INVALID:
                    return _imageRelativePath + "Path 112.png";

                default:
                    return _imageRelativePath + "Path 111.png";
            }
        }


        public override bool Equals(object other)
        {
            if (other == null)
            {
                return false;
            }

            

            if (!typeof(SystemBase).IsAssignableFrom(other.GetType()))
            {
                return false;
            }

            SystemBase systemBase = (SystemBase)other;

            return this.Id.Equals(systemBase.Id) && this.HvacSystemType.Equals(systemBase.HvacSystemType);
        }


        public override int GetHashCode()
        {
            return this.Id.GetHashCode() + this.HvacSystemType.GetHashCode();
        }

        public void RegenerateId()
        {
            this.Id = Guid.NewGuid().ToString("N");
        }


    }
}
