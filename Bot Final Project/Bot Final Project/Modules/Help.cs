using Discord;

using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bot_Final_Project.Modules
{
    public class Help : GuildCommands
    {
        private readonly CommandService _commandService;
        public Help(CommandService commandService)
        {
            _commandService = commandService;
        }

        [Summary("Show all available commands")]
        [Command("help")]
        public async Task HelpCommand()
        {
            List<CommandInfo> commands = _commandService.Commands.ToList();
            EmbedBuilder embedBuilder = new EmbedBuilder();

            foreach (CommandInfo command in commands)
            {
                // Get the command Summary attribute information
                string embedFieldText = command.Summary ?? "haven't made this one yet\n";

                embedBuilder.AddField(command.Name, embedFieldText);
            }

            await ReplyAsync("Here's a list of commands and their description: ", false, embedBuilder.Build());
        }

        [Summary("Show help for a specific command with its as a paramter")]
        [Command("help")]
        public async Task HelpCommand([Summary("module name")] string requestedCommand)
        {
            List<CommandInfo> commands = _commandService.Commands.ToList();
            List<ModuleInfo> modules = _commandService.Modules.ToList();
            foreach (CommandInfo c in commands)
            {
                if (c.Name.Equals(requestedCommand))
                {
                    string summary = c.Summary ?? "No command summary available";
                    await ReplyAsync($"{c.Name} - {c.Summary}\nParameters:");

                    foreach (ParameterInfo param in c.Parameters)
                    {
                        var value = param.Summary ?? "No parameter summary available";
                        await ReplyAsync($"{ param.Name}: {value}");
                    }
                    return;
                }
            }
        }
            
    }
}