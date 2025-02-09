using CliWrap;
using Discord.Audio;
using Discord.WebSocket;
using BasicMusicBot.Models;
using Serilog;
using System.Text;

namespace BasicMusicBot.Extensions
{
    public static class YoutubeQueueItemExtensions
    {

        public static async Task PlayYoutubeAudio(this YoutubeQueueItem item, SocketVoiceChannel voiceChannel, IAudioClient audioClient)
        {
            Log.Information($"[BOT], Attempted Playing: {item.VideoId}");
            using var audioStream = audioClient.CreatePCMStream(AudioApplication.Mixed);
            using var fileStream = new FileStream($".\\CLIApps\\{item.VideoId}.opus", FileMode.Open);
            using var tempMemoryStream = new MemoryStream();

            await Cli.Wrap("CLIApps\\ffmpeg")
                .WithArguments($"-hide_banner -loglevel panic -ac 2 -f s16le -ar 48000 pipe:1 -i pipe:.mp3")
                .WithStandardInputPipe(PipeSource.FromStream(fileStream))
                .WithStandardOutputPipe(PipeTarget.ToStream(tempMemoryStream))
                .ExecuteAsync(item.CancellationToken.Token);

            await item.ChannelOfCommandExec.FollowupAsync($"Starting to play {item.VideoId}");

            tempMemoryStream.Seek(0, SeekOrigin.Begin);

            try { await tempMemoryStream.CopyToAsync(audioStream); }
            finally 
            { 
                await audioStream.FlushAsync();
                Log.Information($"[BOT], Finished Playing: {item.VideoId}");

                await tempMemoryStream.DisposeAsync();
                await fileStream.FlushAsync();
                await fileStream.DisposeAsync();

                File.Delete($"CLIApps\\{item.VideoId}.opus");
            }
        }

        public static async Task DownloadYoutubeAudio(this YoutubeQueueItem item)
        {
            Log.Information($"[BOT], Attempted Download: {item.VideoId}");
            var stdOutBuffer = new StringBuilder();
            var stdErrBuffer = new StringBuilder();

            var command = Cli.Wrap(".\\CLIApps\\yt-dlp.exe")
                .WithArguments($"-x {item.Url} --no-keep-video --output \".\\CLIApps\\%(id)s.%(ext)s\" --ffmpeg-location .\\CLIApps\\ffmpeg.exe")
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer)); //Niet weghalen wij gebruiken de output niet maar die cli app wordt pissig als je ze niet uitleest ofzo

            try
            {
                await command.ExecuteAsync(item.CancellationToken.Token);

                Log.Information($"[BOT], Finished Download: {item.VideoId}");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[BOT], Issue downloading video");
                throw;
            }
        }
    }
}
