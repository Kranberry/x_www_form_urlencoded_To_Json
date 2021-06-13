﻿using System;
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
        private static Regex FindKey = new("(?<=\\\")(.*?)(?=\":)");
        private static Regex FindValue = new("(?<=\\:\")(.*?)(?=\\\")");

        public static string UrlEncodedJsonifier(string urlEncodedStr)
        {
            StringBuilder sb = new();
            
            NameValueCollection dict = HttpUtility.ParseQueryString(urlEncodedStr);
            for (int i = 0; i < dict.AllKeys.Length; i++)
            {
                sb.Append($"\"{dict.AllKeys[i]}\":\"{dict[dict.AllKeys[i]]}\"");
                if (dict.AllKeys.Length - i != 1)
                    sb.Append(',');
                sb.Append('\n');
            }

            string[] rows = sb.ToString().Split('\n');
            List<JsonObject> jsonObjects = new();
            foreach(string row in rows)
            {
                BuildObject(row).ForEach(j => jsonObjects.Add(j));
                //jsonObjects.Add(BuildObject(row));
            }

            IEnumerable<IGrouping<string, JsonObject>> grouping = jsonObjects.GroupBy(j => j.ParentObjectName);
            List<JsonObject> joinedObjects = new();
            foreach(IGrouping<string, JsonObject> jsons in grouping)
            {
                JsonObject joinedObj = new()
                {
                    ObjectName = jsons.ElementAt(0).ObjectName,
                    ChildObjects = new(),
                    Properties = new(),
                    ParentObjectName = jsons.ElementAt(0).ParentObjectName
                };

                foreach(JsonObject obj in jsons)
                {
                    foreach (JsonProperty prop in obj.Properties)
                        joinedObj.Properties.Add(prop);

                    foreach (JsonObject jobj in obj.ChildObjects)
                        joinedObj.ChildObjects.Add(jobj);
                }

                joinedObjects.Add(joinedObj);
            }

            foreach(JsonObject obj in joinedObjects)
            {
                obj.Properties = obj.Properties.Where(o => !string.IsNullOrEmpty(o.PropertyName)).ToList();
                Console.WriteLine(obj.ParentObjectName + " --> " + obj.ObjectName);
                //foreach(JsonObject cobj in obj.ChildObjects)
                //    Console.WriteLine("child Obj: " + cobj.ChildObjects);
                foreach(JsonProperty prop in obj.Properties)
                    Console.WriteLine("prop: " + prop.PropertyName + " : " + prop.Value);
            }

            StringBuilder json = new("{");
            //for(int i = 0; i < jsonObjects.Count; i++)
            //{
            //    json.Append($"\"{jsonObjects[i].PropertyName}\":\"{jsonObjects[i].Value}\"");
            //    if (jsonObjects.Count - i != 1)
            //        json.Append(',');
            //}
            json.Append('}');
            return json.ToString();
        }

        private static List<JsonObject> BuildObject(string rawStringRow)
        {
            List<JsonObject> jsonObjects = new();
            JsonObject rootObj = new() { ParentObjectName = "Document", ObjectName = "Root", ChildObjects = new(), Properties = new() };
            jsonObjects.Add(rootObj);

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
                string shortKey;
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
                key = key.Substring(shortKey.Length + subCount);
                if(shortKey[^1].Equals(']'))
                    shortKey = shortKey.Remove(shortKey.Length - 1);

                if (objectCount != i)
                {
                    JsonObject jsonObject = new()
                    {
                        ParentObjectName = jsonObjects.Last().ObjectName,
                        ObjectName = shortKey,
                        ChildObjects = new(),
                        Properties = new()
                    };
                    jsonObjects.Add(jsonObject);
                    // This means that it is an object, and not a property
                }
                else
                {
                    JsonObject parent = jsonObjects.Last();
                    //if(parent.ObjectName == "merges")
                    //    Console.WriteLine();
                    parent.Properties.Add(new JsonProperty()
                    {
                        ParentName = parent.ObjectName,
                        PropertyName = shortKey,
                        Value = propValue
                    });
                    // This means that it is a property
                }
            }

            return jsonObjects;
        }

        private static JsonProperty BuildProperties(string rawStringRow)
        {
            Regex findParentName = new("");

            string key = FindKey.Match(rawStringRow).Value;
            if(key.Contains('[') && key.Contains(']'))
            {
                int lastIndex = key.LastIndexOf('[') + 1;
                key = key.Substring(lastIndex, key.LastIndexOf(']') - lastIndex);
            }

            string value = FindValue.Match(rawStringRow).Value;

            JsonProperty jsonProperty = new()
            {
                PropertyName = key,
                Value = value
            };
            return jsonProperty;
        }
    }
}
