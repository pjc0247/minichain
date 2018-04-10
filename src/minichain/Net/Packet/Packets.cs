using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace minichain
{
    public class NonPublicPropertiesResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);
            if (member is PropertyInfo pi)
            {
                prop.Readable = (pi.GetMethod != null);
                prop.Writable = (pi.SetMethod != null);
            }
            return prop;
        }
    }

    public class PacketBase
    {
        private static JsonSerializerSettings SerializeSetting =>
            new JsonSerializerSettings()
            {
                ContractResolver = new NonPublicPropertiesResolver(),
                TypeNameHandling = TypeNameHandling.All,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
            };

        public static PacketBase FromJson(string json)
        {
            try
            {
                return (PacketBase)JsonConvert.DeserializeObject(json, SerializeSetting);
            }
            catch(Exception e)
            {
                return null;
            }
        }

        public string pid;

        public PacketBase()
        {
            pid = UniqID.Generate();
        }
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, SerializeSetting);
        }
    }
    public class BroadcastPacket : PacketBase
    {
        public int ttl = 10;
    }

    public class PktRequestPeers : PacketBase
    {
    }
    public class PktResponsePeers : PacketBase
    {
        public string[] addrs;
    }

    public class PktRequestBlock : PacketBase
    {
    }

    public class PktBroadcastNewBlock : BroadcastPacket
    {
        public Block block;
    }
    public class PktNewTransaction : BroadcastPacket
    {
        public int sentBlockNo;
        public Transaction tx;
    }
}
