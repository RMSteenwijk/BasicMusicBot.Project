using Discord;
using Discord.WebSocket;

namespace BasicMusicBot.Factories
{
    public class BotSlashCommandFactory
    {
        public static async Task CreateDefault(DiscordSocketClient discordClient)
        {
            var playCommand = new SlashCommandBuilder()
                .WithName("play")
                .WithDescription("Play a song with a url")
                .AddOption("youtube-url", ApplicationCommandOptionType.String, "Please enter a url with the following format: [https://www.youtube.com/watch?v=]")
                .Build();

            var allCommands = await discordClient.GetGlobalApplicationCommandsAsync();

            if (!allCommands.Any(c => c.Name == "play"))
                await discordClient.CreateGlobalApplicationCommandAsync(playCommand);
        }
    }
}
