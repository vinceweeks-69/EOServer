using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels.ControllerModels
{
    public class GenericGetRequest
    {
        string _uri;
        string _paramName;
        long _paramId;
        string _paramValue;

        public string Uri { get { return _uri; } }

        public string ParamName { get { return _paramName; } }

        //usualy PK
        public long ParamId { get { return _paramId; } }

        //find by name
        public string ParamValue { get { return _paramValue; } }

        public GenericGetRequest(string uri, string paramName, long paramId)
        {
            this._uri = uri;
            this._paramName = paramName;
            this._paramId = paramId;
            this._paramValue = String.Empty;
        }

        public GenericGetRequest(string uri, string paramName, string paramValue)
        {
            this._uri = uri;
            this._paramName = paramName;
            this._paramId = 0;
            this._paramValue = paramValue;
        }
    }
}
