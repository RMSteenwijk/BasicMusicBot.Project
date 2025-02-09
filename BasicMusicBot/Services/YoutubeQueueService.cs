using Discord.Audio;
using Discord.WebSocket;
using BasicMusicBot.Models;
using BasicMusicBot.Extensions;
using Serilog;

namespace BasicMusicBot.Services
{
    public class YoutubeQueueService : IBotService
    {
        public List<YoutubeQueueItem> YoutubeQueueItems { get; set; } = new List<YoutubeQueueItem>();

        public SocketVoiceChannel TargetVoiceChannel { get; set; }

        public bool IsPlaying { get; set; }

        private Timer _timer;

        private IAudioClient _audioClient;

        public async Task StartAsync()
        {
            Log.Information("[BOT], YTQueuing Started");
            _timer = new Timer(doWork, null, 0, 1000);
            await Task.CompletedTask;
        }

        private async void doWork(object? state)
        {
            try
            {
                if (TargetVoiceChannel == null)
                    return;
                if (IsPlaying)
                    return;

                if (YoutubeQueueItems.Count == 0
                    && TargetVoiceChannel.ConnectedUsers.Where(c => c.IsBot).Any())
                {
                    _audioClient.Dispose();
                    await TargetVoiceChannel.DisconnectAsync();
                    return;
                }

                if (YoutubeQueueItems.Count == 0)
                    return;

                IsPlaying = true;

                var nextItem = YoutubeQueueItems.First();

                if (!TargetVoiceChannel.ConnectedUsers.Where(c => c.IsBot).Any())
                    _audioClient = await TargetVoiceChannel.ConnectAsync();

                try
                {
                    await nextItem.DownloadYoutubeAudio();
                    await nextItem.PlayYoutubeAudio(TargetVoiceChannel, _audioClient);
                }
                catch (OperationCanceledException)
                {
                    //Do nothing, song is being skipped
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[BOT], Error");
                    await nextItem.ChannelOfCommandExec.FollowupAsync("Issue playing song, contact roy");
                    throw;
                }
                finally
                {
                    YoutubeQueueItems.Remove(nextItem);
                    IsPlaying = false;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[BOT], Error");
                throw;
            }
        }

        public async Task StopAsync()
        {
            Log.Information("[BOT], YTQueuing Stopping");
            _timer.Dispose();
            await Task.CompletedTask;
        }
        public void Dispose() => Task.Factory.StartNew(async () => await StopAsync());
    }
}
