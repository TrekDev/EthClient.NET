﻿using Eth.Rpc;
using Newtonsoft.Json;
using System;

namespace Eth.Json.Converters
{
    public class EthSolidityContractConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(EthSolidityContract);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            EthSolidityContract contract = new EthSolidityContract();
            int startingDepth = reader.Depth;

            while (!(reader.TokenType == JsonToken.EndObject && reader.Depth == startingDepth))
            {
                reader.Read();
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    string propertyName = reader.Value.ToString();

                    if (String.Equals(propertyName, "code"))
                    {
                        reader.Read();
                        contract.Code = serializer.Deserialize<byte[]>(reader);
                    }
                    else if (String.Equals(propertyName, "info"))
                    {
                        reader.Read();
                        contract.Info = serializer.Deserialize<ContractInfo>(reader);
                    }
                    else
                    {
                        contract.ContractName = reader.Value.ToString();
                        reader.Read();
                    }
                }
            }

            return contract;
        }

        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanWrite { get { return false; } }
    }
}
