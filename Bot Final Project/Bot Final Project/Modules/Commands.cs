using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Bot_Final_Project.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        private DiscordSocketClient _client;

        [Command("flip")]
        public async Task Flip()
        {
            System.Random random = new System.Random();
            int coin = random.Next(2);
            if (coin == 0)
            {
                await ReplyAsync("The coin is heads!");
            }
            else
            {
                await ReplyAsync("The coin is tails!");
            }
        }

        [Command("ANGELS")]
        public async Task Angel()
        {
            await ReplyAsync("Get in Unit 01 Shinji!");
        }

        [Command("users")]
        public async Task GetUserList()
        {
            var guild = await _client.GetGuild(123);
        }
    }   
}
