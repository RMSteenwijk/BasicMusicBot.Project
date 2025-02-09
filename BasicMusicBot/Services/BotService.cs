using Discord.WebSocket;
using BasicMusicBot.Commands;
using BasicMusicBot.Factories;
using BasicMusicBot.Settings;
using Serilog;

namespace BasicMusicBot.Services
{
    public class BotService : IBotService
    {
        private readonly DiscordSocketClient _client;
        private readonly BotSettings _settings;
        private readonly YoutubeQueueService _queueService;

        public BotService(DiscordSocketClient discord, BotSettings settings, YoutubeQueueService queueService) 
        {
            _client = discord;
            _settings = settings;
            _queueService = queueService;
        }

        public async Task StartAsync()
        {
            SerilogFactory.CreateDefault(_client);

            _client.Ready += _botFactorySetup;
            _client.SlashCommandExecuted += _client_SlashCommandExecuted;

            await _client.LoginAsync(Discord.TokenType.Bot, _settings.ApiKey);
            await _client.StartAsync();
            Log.Information("[BOT], BotService Started");
            await Task.CompletedTask;
        }

        private async Task _botFactorySetup()
        {
            await BotSlashCommandFactory.CreateDefault(_client);
        }

        private async Task _client_SlashCommandExecuted(SocketSlashCommand command)
        {
            switch (command.Data.Name)
            {
                case "play":
                    await YoutubeCommand.ExecuteYoutubeUrl(command, _client, _queueService);
                    break;
            }
            await Task.CompletedTask;
        }

        public async Task StopAsync()
        {
            Log.Information("[BOT], BotService Stopping");
            await _client.LogoutAsync();
            await _client.StopAsync();
            _client.Dispose();
            await Task.CompletedTask;
        }


        public void Dispose() => Task.Factory.StartNew(async () => await StopAsync());
    }
}
