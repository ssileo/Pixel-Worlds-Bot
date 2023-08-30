using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using PixelWorldsBot.BSON;

namespace PixelWorldsBot
{
    public class Send
    {
        PixelWorldsBot Bot;
        public Send(PixelWorldsBot _bot)
        {
            Bot = _bot;
        }

        public void CustomPacket(BSONObject packet)
        {
            Bot.PushPacket(packet);
        }

        public void DD()
        {
            Bot.packetQueue.Clear();
            BSONObject bson = new BSONObject
            {
                ["ID"] = "DD"
            };
            Bot.PushPacket(bson);
        }

        public void VChk()
        {
            BSONObject bson = new BSONObject
            {
                ["ID"] = "VChk",
                ["OS"] = "IPhonePlayer",
                ["OSt"] = 1
            };
            Bot.PushPacket(bson);
        }

        public void SendMove(Vector2 _pos)
        {
            SendMove(_pos.X, _pos.Y);
        }

        public void SendMove(float x, float y)
        {
            int newX = (int)Math.Floor(x * 3.2);
            int newY = (int)Math.Floor(y * 3.2);

            int playerX = (int)Math.Floor(Bot.worldPosition.X * 3.2);
            int playerY = (int)Math.Floor(Bot.worldPosition.Y * 3.2);

            int a = 1;
            int d = 7;

            if (this.Bot.worldPosition.X != newX || this.Bot.worldPosition.Y != newY)
            {
                byte[] bufx = BitConverter.GetBytes(newX);
                byte[] bufy = BitConverter.GetBytes(newY); ;

                byte[] buf = new byte[8];
                Buffer.BlockCopy(bufx, 0, buf, 0, bufx.Length);
                Buffer.BlockCopy(bufy, 0, buf, bufx.Length, bufy.Length);

                BSONObject bson = new BSONObject
                {
                    ["ID"] = "mp",
                    ["pM"] = buf
                };

                Bot.PushPacket(bson);
            }


            /*if (x == Bot.worldPosition.X && y == Bot.worldPosition.Y)
            {
                Bot.PushPacket(new BSONObject
                {
                    ["ID"] = "mP"
                });
                return;
            }*/

            //if (Bot.worldPosition.X * 3.2 != newX || Bot.worldPosition.Y != newY)
            
            Bot.PushPacket(
                new BSONObject
                {
                    ["ID"] = "mP",
                    ["t"] = Utils.GetTimeStamp(),
                    ["x"] = x,
                    ["y"] = y,
                    ["a"] = a,
                    ["d"] = d
                });

            this.Bot.worldPosition.X = (float)x;
            this.Bot.worldPosition.Y = (float)y;
        }

        public void gLSI()
        {
            BSONObject bson = new BSONObject
            {
                ["ID"] = "gLSI",
            };
            Bot.PushPacket(bson);
        }

        public void GPd(Requests.LoginInfo loginInfo)
        {
            BSONObject bson = new BSONObject
            {
                ["ID"] = "GPd",
                ["CoID"] = loginInfo.identityId,
                ["Tk"] = loginInfo.logintoken,
                ["cgy"] = 877
            };
            Bot.PushPacket(bson);
        }

        public void ReadyToPlay()
        {
            BSONObject bson = new BSONObject
            {
                ["ID"] = "RtP"
            };
            Bot.PushPacket(bson);
        }

        public void GetWorld(string _name)
        {
            BSONObject bson = new BSONObject
            {
                ["ID"] = "Gw",
                ["eID"] = "",
                ["W"] = _name.ToUpper()
            };
            Bot.PushPacket(bson);
        }

        public void TryToJoinWorld(string _name)
        {
            Bot.worldPosition = new Vector2(-1, -1);
            BSONObject bson = new BSONObject
            {
                ["ID"] = "TTjW",
                ["W"] = _name.ToUpper(),
                ["WB"] = 0,
                ["Amt"] = 0
            };
            Bot.PushPacket(bson);
        }

        /// <summary>
        /// The id for this packet is PSicU it takes 1 param which is the status icon.
        /// </summary>
        /// <param name="_sic">The status icon id</param>
        public void PlayerStatusIconUpdate(int _sic)
        {
            BSONObject bson = new BSONObject
            {
                ["ID"] = "PSicU",
                ["SIc"] = _sic
            };
            Bot.PushPacket(bson);
        }

        public void WorldChatMessage(string _message)
        {
            BSONObject bson = new BSONObject
            {
                ["ID"] = "WCM",
                ["msg"] = _message
            };
            Bot.PushPacket(bson);
        }

        public void LeaveWorld()
        {
            Bot.world = null;
            Bot.worldPosition = new Vector2(-1, -1);
            Bot.spawned = false;
            BSONObject bson = new BSONObject
            {
                ["ID"] = "LW"
            };
            Bot.PushPacket(bson);
        }
    }
}
