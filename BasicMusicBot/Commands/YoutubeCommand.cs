using Discord.WebSocket;
using BasicMusicBot.Models;
using BasicMusicBot.Services;
using Serilog;

namespace BasicMusicBot.Commands
{
    public class YoutubeCommand
    {
        public static async Task ExecuteYoutubeUrl(SocketSlashCommand command, DiscordSocketClient discord, YoutubeQueueService queueService)
        {
            var query = command.Data.Options.First().Value as string;

            if (string.IsNullOrWhiteSpace(query))
            {
                await command.RespondAsync("Please provide a url.");
                return;
            }

            var videoId = query.Substring(query.IndexOf("?v=") + 3);
            var userId = command.User.Id;
            var guildId = command.GuildId;
            Log.Information($"[BOT], User requested: {videoId}");

            if (!guildId.HasValue)
            {
                await command.RespondAsync("Cannot find guild user is in...");
                return;
            }
            var channelOfUser = discord.GetGuild(guildId ?? 0).GetUser(userId).VoiceChannel;

            if (channelOfUser == null)
            {
                await command.RespondAsync("User must be in a voice channel.");
                return;
            }
        
            await command.RespondAsync($"Inserting '{videoId}' into queue");

            //Add here to general queue for playlist
            queueService.YoutubeQueueItems.Add(new YoutubeQueueItem
            {
                VideoId = videoId,
                Url = query,
                CancellationToken = new CancellationTokenSource(),
                UserId = userId,
                ChannelOfCommandExec = command
            });
            queueService.TargetVoiceChannel = channelOfUser;

            await Task.CompletedTask;
        }
    }
}
