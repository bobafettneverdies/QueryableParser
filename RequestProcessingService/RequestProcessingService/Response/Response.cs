using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequestProcessingService
{
    public class Response
    {
        public object Data { get; set; }
        public Int64 Total { get; set; }
        public bool Success { get; set; }
        public IList<String> ErrorList { get; set; } 
        
        public Response() 
        {
            Success = true;
            ErrorList = new List<String>();
        }

        public Response AddError(String error)
        {
            ErrorList.Add(error);
            Success = false;

            return this;
        }
    }
}
