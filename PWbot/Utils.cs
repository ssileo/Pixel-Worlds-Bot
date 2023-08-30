using PixelWorldsBot.BSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PixelWorldsBot
{
    public static class Utils
    {
        public static long GetTimeStamp()
        {
            // My math isn't the best so thats why this sum is so weird
            return (((DateTime.UtcNow.Ticks - 621355968000000000) / 10000) * 10000) + 621355968000000000;
        }

        public static void ReadBSON(BSONObject SinglePacket, Logger logger, string Parent = "")
        {
            foreach (string Key in SinglePacket.Keys)
            {
                try
                {
                    BSONValue Packet = SinglePacket[Key];
                    switch (Packet.valueType)
                    {
                        case BSONValue.ValueType.String:
                            logger.LogMessage($"{Parent} = {Key} | {Packet.valueType} = {Packet.stringValue}");
                            break;
                        case BSONValue.ValueType.Boolean:
                            logger.LogMessage($"{Parent} = {Key} | {Packet.valueType} = {Packet.boolValue}");
                            break;
                        case BSONValue.ValueType.Int32:
                            logger.LogMessage($"{Parent} = {Key} | {Packet.valueType} = {Packet.int32Value}");
                            break;
                        case BSONValue.ValueType.Int64:
                            logger.LogMessage($"{Parent} = {Key} | {Packet.valueType} = {Packet.int64Value}");
                            break;
                        case BSONValue.ValueType.Binary: // BSONObject
                            logger.LogMessage($"{Parent} = {Key} | {Packet.valueType}");
                            ReadBSON(SimpleBSON.Load(Packet.binaryValue), logger, Key);
                            break;
                        case BSONValue.ValueType.Double:
                            logger.LogMessage($"{Parent} = {Key} | {Packet.valueType} = {Packet.doubleValue}");
                            break;
                        case BSONValue.ValueType.UTCDateTime:
                            logger.LogMessage($"{Parent} = {Key} | {Packet.valueType} = {Packet.dateTimeValue}");
                            break;
                        default:
                            logger.LogMessage($"{Parent} = {Key} = {Packet.valueType}");
                            break;
                    }

                }
                catch
                {
                }
            }
        }

        public static string GetIPFromDNS(string _dns)
        {
            IPHostEntry hostEntry = Dns.GetHostEntry(_dns);

            if (hostEntry.AddressList.Length > 0)
            {
                var ip = hostEntry.AddressList[0];
                return ip.ToString();
            }
            return string.Empty;
        }
    }
}