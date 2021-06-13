using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace x_www_form_urlencoded_To_Json
{
    class JsonObject
    {
        public string ParentObjectName { get; set; }
        public string ObjectName { get; set; }
        public List<JsonProperty> Properties { get; set; }
        public List<JsonObject> ChildObjects { get; set; }
    }
}
