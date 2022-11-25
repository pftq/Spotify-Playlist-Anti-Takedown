# Spotify-Playlist-Anti-Takedown
Short script for restoring the title/description if a bot is repeatedly taking down your playlist.  A bot kept taking down my Spotify playlist, so I made another bot to keep bringing it back.

Instructions: Executable file is located in bin/release folder (just double-click the exe).

Right now, someone can just report your playlist, and it'll instantly lose its title and description with no recourse.  It's a problem that's been around as described here:
https://community.spotify.com/t5/Live-Ideas/Playlists-Solution-to-false-abusive-reporting/idi-p/4928676

This script will continuously monitor all playlists on your Spotify account and restore the playlist if any of them lose their title/description.  It undoes the damage/symptom, not the core problem, but that's ultimately up to Spotify to resolve.

At this time, the script uses the 1-hour token, which means you need to re-enter a new token (link provided in the program console) once an hour, but I've found that the bot stops trying after about that much time so I haven't needed to implement token-refresh code.  I'll look into it if it becomes needed, but others are also welcome to implement it since the script is open-source.
