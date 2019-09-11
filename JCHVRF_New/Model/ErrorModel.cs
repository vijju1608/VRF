using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCHVRF_New.Model
{
    public enum ErrorType
    {
        Error = 1,
        Warning = 2,
        Message = 3
    }
    public enum ErrorCategory
    {
        PipingErrors = 1,
        WiringError = 2
    }

    public class SystemError
    {
        public ErrorType Type { get; set; }
        public ErrorCategory Category { get; set; }
        public string Description { get; set; }

        public string ErrorType
        {
            get
            {
                return Type.ToString();
            }
        }

        public string ErrorCategory
        {
            get
            {
                return Category.ToString();
            }
        }

        public string ErrorTypeImg
        {
            get
            {
                switch(Type)
                {
                    case Model.ErrorType.Error:
                        return "..\\..\\Image\\TypeImages\\Error.png";

                    case Model.ErrorType.Warning:
                        return "..\\..\\Image\\TypeImages\\Warning.png";

                    default: return "..\\..\\Image\\TypeImages\\Error.png";
                }
              
            }
        }

    }

}
