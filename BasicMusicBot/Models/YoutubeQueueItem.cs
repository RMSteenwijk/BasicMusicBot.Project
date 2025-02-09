using Discord.WebSocket;

namespace BasicMusicBot.Models
{
    public class YoutubeQueueItem
    {
        public string VideoId { get; set; }
        public string Url { get; set; }
        public CancellationTokenSource CancellationToken { get; set; }
        public ulong UserId { get; set; }
        public SocketSlashCommand ChannelOfCommandExec { get; set; }
    }
}
