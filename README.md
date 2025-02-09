# BasicMusicBot

This project was created to allow users to easily stream music into their discord channels when hanging out with their buddies.

# Requirements
You require a couple of things to run this project locally.
- [Dotnet SDK 8.0](https://dotnet.microsoft.com/en-us/download)
- The .dll files of [opus.dll](https://github.com/xiph/opus) & [libsodium.dll](https://doc.libsodium.org/installation#pre-built-libraries)
- The Command Line Applications "[ffmpeg.exe](https://www.ffmpeg.org/download.html)" & "[yt-dlp.exe](https://github.com/yt-dlp/yt-dlp/releases/)"

# Getting Started

1. Download & install Dotnet SDK 8.0
2. Ensure Dotnet is properly install by running dotnet --version in the Terminal
3. Place the 'opus.dll' & 'libsodium.dll' inside the BasicMusicBot folder
4. Create a folder called 'CLIApps' and place the 'ffmpeg.exe' & 'yt-dlp' inside that folder.
5. Open the appsettings.json and fill in your Discord Bot API Key
6. Next open a terminal run: ```dotnet run -start```
7. ???
8. Profit
