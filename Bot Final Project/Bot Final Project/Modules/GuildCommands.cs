using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Collections.ObjectModel;

namespace Bot_Final_Project.Modules
{
    public class GuildCommands : ModuleBase
    {
        [Command("gamble")]
        [Alias("game")]
        [Summary("A short to game to gamble points")]
        private async Task Gamble([Summary("The amount you would like to bet")] int bet = -1)
        {
            if (bet < 5 || bet > 50)
            {
                await ReplyAsync("Invalid bet amount. The bet must be between 5 and 50");
                return;
            }
            IGuild guild = await Context.Client.GetGuildAsync(Context.Guild.Id);
            IGuildUser self = await guild.GetUserAsync(Context.User.Id);
            await ChangeMoney(self, bet, false);
            Random rnd = new Random();
            int num = rnd.Next(0, 22);
            double multiplier = num * 0.1;
            int newBalance = (int)Math.Ceiling(multiplier * bet);
            await ChangeMoney(self, newBalance, true);
            await ReplyAsync($"Your points multiplier was *{Math.Round(multiplier,1)}*");
            if(newBalance > bet)
            {
                await ReplyAsync($"You won {newBalance - bet} points!");
            } 
            else if (newBalance == bet)
            {
                await ReplyAsync("You received no points.");
            }
            else
            {
                await ReplyAsync($"Too bad, you lost {bet - newBalance} points...");
            }
        }
        [Command("randomuser")]
        [Alias("random")]
        [Summary("Chooses a random user")]
        private async Task RandomUser()
        {
            IGuild guild = await Context.Client.GetGuildAsync(Context.Guild.Id);
            var users = await guild.GetUsersAsync();
            List<IGuildUser> userList = new List<IGuildUser>(users);
            userList = RemoveBots(userList);
            int count = userList.Count;
            Random rnd = new Random();
            int num = rnd.Next(0, count);
            await ReplyAsync($"I choose... {userList[num].Mention}!");
        }
        [Command("randomuserlist")]
        [Alias("randoms", "randomusers")]
        [Summary("Lists the users in the server in a random order")]
        private async Task RandomUserList()
        {
            IGuild guild = await Context.Client.GetGuildAsync(Context.Guild.Id);
            var users = await guild.GetUsersAsync();
            List<IGuildUser> userList = new List<IGuildUser>(users);
            userList = RemoveBots(userList);
            int count = userList.Count;
            Random rnd = new Random();
            List<int> order = new List<int>();
            int num, placement = 1;
            for (int i = 0; i < count; i++)
            {
                do
                {
                    num = rnd.Next(0, count);
                } while (order.Contains(num));
                order.Add(num);
            }
            foreach (int i in order)
            {
                await ReplyAsync($"{placement}: {userList[i].Mention}");
                placement++;
            }
        }

        [Command("leaderboardposition")]
        [Alias("whereami", "placement", "rank", "pos")]
        [Summary("Shows the user's current position on the leaderboard")]
        private async Task LeaderboardPosition()
        {
            IGuild guild = await Context.Client.GetGuildAsync(Context.Guild.Id);
            IGuildUser user = await guild.GetUserAsync(Context.User.Id);
            var users = await guild.GetUsersAsync();
            List<IGuildUser> userList = new List<IGuildUser>(users);
            userList = CreateLeaderboard(userList, guild);
            int pos = userList.IndexOf(user);
            await ReplyAsync($"You are placed #{pos + 1} with {ReadMoney(guild, user)} points!");
        }

        [Command("leaderboard")]
        [Alias("top5", "topfive", "ranking")]
        [Summary("Shows the top 5 users with the most points")]
        private async Task Leaderboard()
        {
            IGuild guild = await Context.Client.GetGuildAsync(Context.Guild.Id);
            var users = await guild.GetUsersAsync();
            List<IGuildUser> userList = new List<IGuildUser>(users);
            userList = RemoveBots(userList);
            userList = CreateLeaderboard(userList, guild);
            for (int i = 0; i < 5 || i < userList.Count; i++)
            {
                await ReplyAsync($"{i + 1}: {userList[i].Mention} - {ReadMoney(guild, userList[i])}");
            }
        }


        [Command("tellmeajoke")]
        [Alias("joke")]
        [Summary("Tells one of 50 jokes.")]
        private async Task JokeTeller()
        {
            await GuildFilesCheck();
            IGuild guild = await Context.Client.GetGuildAsync(Context.Guild.Id);
            IGuildUser user = await guild.GetUserAsync(Context.User.Id);
            int balance = ReadMoney(guild, user);
            if (balance < 10)
            {
                await ReplyAsync($"You must have at least 10 points to recieve a joke.\nYou currently have {balance} points");
                return;
            }
            else
            {
                int newBalance = balance - 10;
                await ChangeMoney(user, 10, false);
                Random rnd = new Random();
                int num = rnd.Next(0, 50);
                await ReplyAsync(JokeList.jokes[num]);
                await ReplyAsync($"Your new balance is {newBalance}");
            }
        }

        [Command("kick")]
        [RequireUserPermission(GuildPermission.KickMembers, ErrorMessage = "Invalid Permissions")]
        [Summary("Kicks a specified user")]
        [Alias("test")]
        private async Task KickMember([Summary("Mentioned user to kick")] IGuildUser user = null, [Summary("Reason for kick - (Optional)")][Remainder] string reason = null)
        {
            if (user == null)
            {
                await ReplyAsync("Please mention the user you would like to kick by mentioning them after the command");
                return;
            }
            reason = reason ?? "Unspecified Reason";
            var guildID = Context.Guild.Id;
            IGuild guild = await Context.Client.GetGuildAsync(guildID);
            await user.KickAsync(reason);
            await ReplyAsync($"{user.Username} has been kicked from {guild.Name}\nReason: {reason}");
        }

        [Command("count")]
        [Summary("Tells you how many users are on the server")]
        private async Task CountUsers()
        {
            IGuild guild = await Context.Client.GetGuildAsync(Context.Guild.Id);
            var users = await guild.GetUsersAsync();
            var userCount = users.Count;
            await ReplyAsync($"There are {userCount} currently users in this server!");
            foreach (IGuildUser u in users)
            {
                await ReplyAsync(u.Username);
            }

        }
        [Command("ban")]
        [Summary("Bans the specified user")]
        [RequireUserPermission(GuildPermission.BanMembers, ErrorMessage = "Insufficient Privileges")]
        private async Task BanMember([Summary("Mentioned user to ban")] IGuildUser user = null, [Summary("Reason for ban - (Optional)")][Remainder] string reason = null)
        {
            if (user == null)
            {
                await ReplyAsync("Please mention the user you would like to ban by mentioning them after the command");
                return;
            }
            reason = reason ?? "Unspecified Reason";
            var guildID = Context.Guild.Id;
            IGuild guild = await Context.Client.GetGuildAsync(guildID);
            await Context.Guild.AddBanAsync(user, 0, reason);
            await ReplyAsync($"{user.Username} has been banned from {guild.Name}\nReason: {reason}");
        }

        [Command("getmoney")]
        [Summary("Shows the user the amount of money they have")]
        private async Task GetMoney([Summary("Mentioned user to retrieve points from")] IGuildUser user = null)
        {
            await GuildFilesCheck();
            IGuild guild = await Context.Client.GetGuildAsync(Context.Guild.Id);
            IGuildUser self = await guild.GetUserAsync(Context.User.Id);
            user = user ?? self;
            await ReplyAsync($"{user.Mention} has {ReadMoney(guild, user)} of unindentified points");
        }

        [Command("give")]
        [Alias("gift", "reward")]
        [Summary("Gives the specified user a specified amount of points")]
        private async Task GiveMoney([Summary("Mentioned user to give points to")] IGuildUser user = null, [Summary("Amount of points to give")][Remainder] int amount = -1995)
        {
            await GuildFilesCheck();
            if (user == null || amount == -1955)
            {
                await ReplyAsync("Please format your command like this:\n!give @Unit01 10");
                return;
            }
            else if (amount <= 0)
            {
                await ReplyAsync("The amount must be greater than 0");
                return;
            } else if (amount > 1000)
            {
                await ReplyAsync("Cannot give more than 1000 points at a time");
                return;
            }
            await ChangeMoney(user, amount, true);
            await ReplyAsync($"Gave {user.Mention} {amount} points.");
        }

        [Command("take")]
        [Summary("Subtracts a specified amount of points from a specified user")]
        [RequireUserPermission(GuildPermission.Administrator, ErrorMessage = "Insufficient Privileges")]
        private async Task TakeMoney([Summary("Mentioned user to take points from")] IGuildUser user = null, [Summary("Amount of points to take from user")][Remainder] int amount = -1995)
        {
            await GuildFilesCheck();
            if (user == null || amount == -1955)
            {
                await ReplyAsync("Please format your command like this:\n!take @Unit01 10");
                return;
            }
            else if (amount <= 0)
            {
                await ReplyAsync("The amount must be greater than 0");
                return;
            }
            else if (amount > 1000)
            {
                await ReplyAsync("Cannot give more than 1000 points at a time");
                return;
            }
            await ChangeMoney(user, amount, false);
            await ReplyAsync($"Took {amount} points from {user.Mention}.");
        }

        [Command("giveme")]
        [Alias("gimme")]
        [Summary("Gives the user a specified amount of points")]
        private async Task GiveYourselfMoney([Summary("Amount of points to give to yourself")] int amount = -1955)
        {
            await GuildFilesCheck();
            IGuild guild = await Context.Client.GetGuildAsync(Context.Guild.Id);
            IGuildUser self = await guild.GetUserAsync(Context.User.Id);
            if (amount == -1955)
            {
                await ReplyAsync("Please format your command like this:\n!gimme [amount]");
                return;
            }
            if (amount <= 0)
            {
                await ReplyAsync("The amount must be greater than 0");
                return;
            }
            else if (amount > 1000)
            {
                await ReplyAsync("Cannot give more than 1000 points at a time");
                return;
            }
            await ChangeMoney(self, amount, true);
            await ReplyAsync($"Gave {self.Mention} {amount} points.");
        }

        private List<IGuildUser> RemoveBots(List<IGuildUser> userList)
        {
            List<int> bots = new List<int>();
            for (int i = 0; i < userList.Count; i++)
            {
                IUser tmp = userList[i] as IUser;
                if (tmp.IsBot)
                {
                    userList.RemoveAt(i);
                }
            }
            return userList;
        }

        private List<IGuildUser> CreateLeaderboard(List<IGuildUser> users, IGuild guild)
        {

            List<int> userPoints = new List<int>();
            int pos = 0, tmp;
            IGuildUser tmpUser;
            foreach (IGuildUser u in users)
            {
                userPoints.Add(ReadMoney(guild, u));
                pos++;
            }
            int n = users.Count;
            for (int i = 0; i < n - 1; i++)
                for (int j = 0; j < n - i - 1; j++)
                    if (userPoints[j] < userPoints[j + 1])
                    {
                        // swap temp and arr[i] 
                        tmp = userPoints[j];
                        tmpUser = users[j];
                        users[j] = users[j + 1];
                        userPoints[j] = userPoints[j + 1];
                        users[j + 1] = tmpUser;
                        userPoints[j + 1] = tmp;
                        
                    }
            return users;
        }

        private async Task GuildFilesCheck()
        {
            IGuild guild = await Context.Client.GetGuildAsync(Context.Guild.Id);
            var users = await guild.GetUsersAsync();
            string folderName = @"C:\Users\Tim\Documents\Unit01Files\" + Context.Guild.Name;
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }
            foreach (IGuildUser u in users)
            {
                if (!File.Exists(folderName + @"\" + u.Username + ".txt"))
                {
                    using FileStream fs = File.Create(folderName + @"\" + u.Username + ".txt");
                    using var sr = new StreamWriter(fs);

                    sr.WriteLine("0");
                }
            }
        }

        private int ReadMoney(IGuild guild, IGuildUser user)
        {
            string fileName = @"C:\Users\Tim\Documents\Unit01Files\" + Context.Guild.Name + @"\" + user.Username + ".txt";
            StreamReader sr = new StreamReader(fileName);
            var money = sr.ReadLine();
            sr.Close();
            return Int32.Parse(money);
        }

        private async Task ChangeMoney(IGuildUser user, int amount, bool give)
        {
            
            IGuild guild = await Context.Client.GetGuildAsync(Context.Guild.Id);
            string fileName = @"C:\Users\Tim\Documents\Unit01Files\" + Context.Guild.Name + @"\" + user.Username + ".txt";
            int balance = ReadMoney(guild, user);
            int newBalance;
            if (give)
            {
                newBalance = balance + amount;
            }
            else
            {
                newBalance = balance - amount;
                if(newBalance < 0) { newBalance = 0; }
            }

            StreamReader reader = new StreamReader(fileName);
            string input = reader.ReadToEnd();
            reader.Close();

            using (StreamWriter writer = new StreamWriter(fileName))
            {
                {
                    string output = input.Replace(balance.ToString(), newBalance.ToString());
                    writer.Write(output);
                }
                writer.Close();
            }
        }
    }
}
