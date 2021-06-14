using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace x_www_form_urlencoded_To_Json
{
    class UrlEncodedToJsonCoverter
    {
        private static readonly Regex FindKey = new("(?<=\\\")(.*?)(?=\":)");
        private static readonly Regex FindValue = new("(?<=\\:\")(.*?)(?=\\\")");

        public static string UrlEncodedJsonifier(string urlEncodedStr)
        {
            StringBuilder sb = new();            
            // Decode the string into a "dictionary"
            NameValueCollection dict = HttpUtility.ParseQueryString(urlEncodedStr);

            // Sort it up
            for (int i = 0; i < dict.AllKeys.Length; i++)
            {
                sb.Append($"\"{dict.AllKeys[i]}\":\"{dict[dict.AllKeys[i]]}\"");
                if (dict.AllKeys.Length - i != 1)
                    sb.Append(',');
                sb.Append('\n');
            }
            // Get every line, this will let us get what properties goesto waht object
            string[] rows = sb.ToString().Split('\n');
            
            // Build the json objects
            List<JsonObject> jsonObjects = new();
            foreach(string row in rows)
            {
                BuildObject(row).ForEach(j => jsonObjects.Add(j));
                //jsonObjects.Add(BuildObject(row));
            }

            // Group every object into groups of their parent
            IEnumerable<IGrouping<string, JsonObject>> grouping = jsonObjects.GroupBy(j => j.ParentObjectName);
            List<JsonObject> joinedObjects = new();
            // Combine each JsonObject to a singular object with the right parent as a parent.
            foreach(IGrouping<string, JsonObject> jsons in grouping)
            {
                // If there are multiple json objects as a child under one json object, then we need to differenciate between them
                IEnumerable<IGrouping<string, JsonObject>> groups = jsons.GroupBy(x => x.ObjectName);

                foreach (IGrouping<string, JsonObject> group in groups)
                {
                    JsonObject joinedObj = new()
                    {
                        ObjectName = group.ElementAt(0).ObjectName,
                        ChildObjects = new(),
                        Properties = new(),
                        ParentObjectName = group.ElementAt(0).ParentObjectName
                    };
                    foreach (JsonObject obj in group)
                    {
                        foreach (JsonProperty prop in obj.Properties)
                            joinedObj.Properties.Add(prop);

                        foreach (JsonObject jobj in obj.ChildObjects)
                            joinedObj.ChildObjects.Add(jobj);
                    }

                    joinedObjects.Add(joinedObj);
                }
            }

            foreach (JsonObject obj in joinedObjects.ToList())
            {
                // Give every JsonObject their respective child objects
                List<JsonObject> childs = joinedObjects.Where(o => obj.ObjectName.Equals(o.ParentObjectName)).ToList();
                childs.ForEach(o => 
                {
                    obj.ChildObjects.Add(o);
                    joinedObjects.Remove(o);
                });
            }

            // Start building the Json string
            StringBuilder json = BuildJsonFromObjectList(joinedObjects);
            json.Insert(0, '{');
            json.Append('}');

            return json.ToString();
        }

        // Build the jsonstring from a list of JsonObjects
        private static StringBuilder BuildJsonFromObjectList(List<JsonObject> objects)
        {
            StringBuilder json = new();
            for (int i = 0; i < objects.Count; i++)
            {
                // Remove junk
                objects[i].Properties = objects[i].Properties.Where(o => !string.IsNullOrEmpty(o.PropertyName)).ToList();
                // Do not include the root object in the json string. Root is the very first bracket pair
                if (!objects[i].ObjectName.Equals("Root"))
                    json.Append($"\"{objects[i].ObjectName}\":{{");

                // Build the json properties as json strings
                for (int j = 0; j < objects[i].Properties.Count; j++)
                {
                    json.Append($"\"{objects[i].Properties[j].PropertyName}\":\"{objects[i].Properties[j].Value}\"");
                    if (objects.Count - i != 0)
                    {
                        if(objects[i].ChildObjects.Count > 0 || j != objects[i].Properties.Count - 1)
                            json.Append(',');
                    }
                }

                // Use recursion to do the same things for every child JsonObject
                json.Append(BuildJsonFromObjectList(objects[i].ChildObjects));
                if (!objects[i].ObjectName.Equals("Root"))
                    json.Append('}');
                if (objects.Count - i != 1)
                    json.Append(',');
            }
            
            return json;
        }

        private static List<JsonObject> BuildObject(string rawStringRow)
        {
            List<JsonObject> jsonObjects = new();
            JsonObject rootObj = new() { ParentObjectName = "Document", ObjectName = "Root", ChildObjects = new(), Properties = new() };
            jsonObjects.Add(rootObj);

            // Find the raw key(including property name and object names), and the property value
            string key = FindKey.Match(rawStringRow).Value;
            string propValue = FindValue.Match(rawStringRow).Value;
            int objectCount = key.Count(c => c.Equals('['));
            if(objectCount == 0)
            {
                JsonProperty prop = new() { ParentName = "Root", PropertyName = key, Value = propValue };
                rootObj.Properties.Add(prop);
                return jsonObjects;
            }

            for (int i = 0; i <= objectCount; i++)
            {
                string shortKey;    // shortKey is the next object, or property
                int subCount = 0;
                if (key[0].Equals('['))
                {
                    subCount = 1;
                    shortKey = key.Substring(1, key.IndexOf(']'));
                }
                else
                {
                    subCount = 0;
                    shortKey = key.Substring(0, key.IndexOf('['));
                }
                key = key[(shortKey.Length + subCount)..];
                if(shortKey[^1].Equals(']'))
                    shortKey = shortKey.Remove(shortKey.Length - 1);    // Remove the last square bracket if it has one

                if (objectCount != i)
                {
                    // This means that it is an object, and not a property
                    JsonObject jsonObject = new()
                    {
                        ParentObjectName = jsonObjects.Last().ObjectName,
                        ObjectName = shortKey,
                        ChildObjects = new(),
                        Properties = new()
                    };
                    jsonObjects.Add(jsonObject);
                }
                else
                {
                    // This means that it is a property
                    JsonObject parent = jsonObjects.Last();
                    parent.Properties.Add(new JsonProperty()
                    {
                        ParentName = parent.ObjectName,
                        PropertyName = shortKey,
                        Value = propValue
                    });
                }
            }

            return jsonObjects;
        }
    }
}
