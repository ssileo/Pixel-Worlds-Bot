using Amazon.DynamoDBv2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelWorldsBot
{
    public class BotManager
    {
        public static BotManager instance;

        public List<PixelWorldsBot> ActiveBots = new List<PixelWorldsBot>();

        public BotManager() {
            this.Run();
        }

        public void Run()
        {
            if (instance != null) return;

            instance = this;
        }

        public int GetActiveBots()
        {
            return ActiveBots.Count;
        }

        public void AddBot(PixelWorldsBot bot)
        {
            ActiveBots.Add(bot);
        }

        public void RemoveBot(PixelWorldsBot bot)
        {
            ActiveBots.Remove(bot);
        }
    }
}
