using PixelWorldsBot.BSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PixelWorldsBot
{
    /// <summary>
    /// This class is kinda pointless but It makes me feel like my code is neater.
    /// </summary>
    class PacketHandler
    {
        private PixelWorldsBot Bot;
        public PacketHandler (PixelWorldsBot _bot)
        {
            this.Bot = _bot;
        }

        public void HandleOtherOwnerIPMessage(BSONObject _bson)
        {
            this.Bot.Disconnect();
            Thread.Sleep(100);
            Bot.logger.LogMessage($"Redirecting to {Bot.ip}...");
            this.Bot.Connect();
        }

        public void HandleVersionNumberCheck()
        {
            Bot.send.GPd(Bot.requests.loginInfo);
        }

        public void HandleGetPlayerData(BSONObject packet)
        {
            Bot.send.TryToJoinWorld(Bot.worldname);
            Bot.client_id = packet["U"].stringValue;
        }

        public void HandleTryToJoinWorld(BSONObject packet)
        {
            Bot.send.GetWorld(Bot.worldname);
        }

        public void HandleGetWorldMessage(BSONObject packet)
        {
            Bot.spawned = false;
            Bot.world = null;
            Bot.world = new World();
            byte[] data = LZMAHelper.DecompressLZMA(packet["W"].binaryValue);
            Bot.world.SetupWorld("KRAKSCLEAR", data);
            
        }

        public void HandleAddNewPlayer(BSONObject packet)
        {
            string username = packet["UN"].stringValue;
            string userID = packet["U"].stringValue;

            this.Bot.NetworkPlayers.Add(username.ToUpper(), userID);
        }

        public void HandlePlayerLeft(BSONObject packet)
        {
            var networkPlayers = Bot.NetworkPlayers.ToList();
            string userID = packet["U"].stringValue;
            for (int i = 0; i < networkPlayers.Count; i++)
            {
                var player = networkPlayers[i];
                
                if (player.Value == userID)
                {
                    Bot.NetworkPlayers.Remove(player.Key);
                    return;
                }
                
            }
        }
    
        
    }
}
