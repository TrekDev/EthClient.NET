﻿using Eth.Rpc;
using Eth.Utilities;
using Newtonsoft.Json;
using System;

namespace Eth.Json.Converters
{
    public class DefaultBlockConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DefaultBlock);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Null:
                    return null;
                case JsonToken.String:
                    string json = reader.Value.ToString();

                    if (String.Equals(json, "latest"))
                    {
                        return DefaultBlock.Latest;
                    }
                    else if (String.Equals(json, "pending"))
                    {
                        return DefaultBlock.Pending;
                    }
                    else if (String.Equals(json, "earliest"))
                    {
                        return DefaultBlock.Earliest;
                    }
                    else
                    {
                        return new DefaultBlock(EthHex.HexStringToInt(json));
                    }
                default:
                    throw new JsonSerializationException("Unknown token type");
            }
        }

        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            if(value == null)
            {
                writer.WriteNull();
                return;
            }

            DefaultBlock db = value as DefaultBlock;

            if(db == null)
            {
                throw new ArgumentOutOfRangeException("value");
            }

            if (db.BlockNumber.HasValue)
            {
                serializer.Serialize(writer, db.BlockNumber.Value);
            }
            else
            {
                writer.WriteValue(db.Option.ToString().ToLowerInvariant());
            }
        }
    }
}
