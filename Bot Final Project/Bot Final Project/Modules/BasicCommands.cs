using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;


namespace Bot_Final_Project.Modules
{
    public class BasicCommands : ModuleBase<SocketCommandContext>
    {
        [Command("flip")]
        [Summary("Flips a coin")]
        private async Task Flip()
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
        [Summary("A command used to test out embedding")]
        private async Task Angel()
        {
            await ReplyAsync("Get in Unit 01 Shinji!");
            var EmbedBuilder = new EmbedBuilder()
                .WithDescription($":rage: shinji was banned\n**Reason** not getting in the unit")
                .WithFooter(footer =>
                {
                    footer
                    .WithText("User Ban Log")
                    .WithIconUrl("https://i.imgur.com/B1ANMeZ.jpeg");
                });
            Embed embed = EmbedBuilder.Build();
            await ReplyAsync(embed: embed);
        }   

        [Command("whoami")]
        [Summary("Tells the user their username, the server they're in, and the server's ID")]
        private async Task WhoAmI()
        {
            var user = Context.User as SocketGuildUser;
            await ReplyAsync($"You are -> [" + user.Username + "]");
            await Context.Channel.SendMessageAsync($"This Discord server's name is {Context.Guild} and ID is {Context.Guild.Id}");
        }

        [Command("up")]
        [Summary
        ("An easy way to checks if the bot is running")]
        [Alias("running")]
        private async Task UpStatement()
        {   
            await ReplyAsync($"Unit01 is up and running!");
        }

    }       
}
