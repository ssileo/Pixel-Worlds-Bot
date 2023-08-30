using Amazon.Runtime.Internal.Util;
using PixelWorldsBot.BSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PixelWorldsBot
{
    public class Logger
    {
        public bool log = true;
        public ConsoleColor colour = ConsoleColor.Blue;
        public ConsoleColor bsonColour = ConsoleColor.Blue;
        public ConsoleColor errorColour = ConsoleColor.Blue;
        public RichTextBox logbox;

        public string logs = "";
        public Logger() {
            
        }

        public void LogMessage(string message)
        {
            if (!log) return;

            Console.ForegroundColor = colour;
            Console.WriteLine(message);
            logs += message + "\n";
            Console.ForegroundColor = ConsoleColor.White;

            /*if (logbox != null)
            {
                // Use Invoke to execute a delegate on the UI thread
                logbox.Invoke((MethodInvoker)delegate
                {
                    // Append the message to the logbox RichTextBox
                    logbox.AppendText(message + "\n");
                    logbox.ScrollToCaret();
                });
            }*/
            //Console.WriteLine(message);
        }

        public void LogError(string message)
        {
            if (!log) return;
            Console.ForegroundColor = errorColour;
            Console.WriteLine(message);
            logs += message + "\n";
            Console.ForegroundColor = ConsoleColor.White;
        }

        public void LogBSON(BSONObject SinglePacket, string Parent = "")
        {
            if (!log) return;
            
            foreach (string Key in SinglePacket.Keys)
            {
                try
                {
                    BSONValue Packet = SinglePacket[Key];
                    Console.ForegroundColor = bsonColour;
                    switch (Packet.valueType)
                    {
                        case BSONValue.ValueType.String:
                            LogMessage($"{Parent} = {Key} | {Packet.valueType} = {Packet.stringValue}");
                            break;
                        case BSONValue.ValueType.Boolean:
                            LogMessage($"{Parent} = {Key} | {Packet.valueType} = {Packet.boolValue}");
                            break;
                        case BSONValue.ValueType.Int32:
                            LogMessage($"{Parent} = {Key} | {Packet.valueType} = {Packet.int32Value}");
                            break;
                        case BSONValue.ValueType.Int64:
                            LogMessage($"{Parent} = {Key} | {Packet.valueType} = {Packet.int64Value}");
                            break;
                        case BSONValue.ValueType.Binary: // BSONObject
                            LogMessage($"{Parent} = {Key} | {Packet.valueType}");
                            Console.ForegroundColor = ConsoleColor.Green;
                            LogBSON(SimpleBSON.Load(Packet.binaryValue), Key);
                            break;
                        case BSONValue.ValueType.Double:
                            LogMessage($"{Parent} = {Key} | {Packet.valueType} = {Packet.doubleValue}");
                            break;
                        case BSONValue.ValueType.UTCDateTime:
                            LogMessage($"{Parent} = {Key} | {Packet.valueType} = {Packet.dateTimeValue}");
                            break;
                        default:
                            LogMessage($"{Parent} = {Key} = {Packet.valueType}");
                            break;
                    }

                    Console.ForegroundColor = ConsoleColor.White;
                }
                catch
                {
                }
            }
            
        }
    }
}
