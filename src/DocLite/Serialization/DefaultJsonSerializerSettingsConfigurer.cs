using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DocLite.Serialization
{
    public class DefaultJsonSerializerSettingsConfigurer
    {
        public static void Configure(JsonSerializerSettings cfg)
        {
            var contractResolver = new ContractResolver();
            cfg.TypeNameHandling = TypeNameHandling.Auto;
            cfg.TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple;
            cfg.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            cfg.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
            cfg.ContractResolver = contractResolver;
        }

        private class ContractResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var prop = base.CreateProperty(member, memberSerialization);

                if (!prop.Writable)
                {
                    var property = member as PropertyInfo;
                    if (property != null)
                    {
                        var hasPrivateSetter = property.GetSetMethod(true) != null;
                        prop.Writable = hasPrivateSetter;
                    }
                }

                return prop;
            }

            protected override List<MemberInfo> GetSerializableMembers(Type objectType)
            {
                var members = objectType
                    .GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(t => t is PropertyInfo || t is FieldInfo || t is EventInfo)
                    .ToList();
                return members;
            }   
        }
    }
}