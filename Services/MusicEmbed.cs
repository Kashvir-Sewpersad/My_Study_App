using System.Text.RegularExpressions;
using System.Web;

namespace My_Study_App.Services
{
    public static class MusicEmbed
    {
        private static readonly Regex IdPattern = new(@"^[A-Za-z0-9_-]{6,64}$");
        private static readonly Regex SpotifyPath =
            new(@"/(playlist|album|track|episode|show|artist)/([A-Za-z0-9]{22})");

        public static (string Provider, string EmbedUrl)? TryParse(string? input)
        {
            if (!Uri.TryCreate(input?.Trim(), UriKind.Absolute, out var uri)) return null;
            if (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp) return null;

            var host = uri.Host.ToLowerInvariant();
            var query = HttpUtility.ParseQueryString(uri.Query);

            // YouTube and YouTube Music
            if (host is "youtube.com" or "www.youtube.com" or "m.youtube.com" or "music.youtube.com")
            {
                var list = query["list"];
                if (list != null && IdPattern.IsMatch(list))
                    return ("YouTube", $"https://www.youtube.com/embed/videoseries?list={list}");

                var v = query["v"];
                if (v != null && IdPattern.IsMatch(v))
                    return ("YouTube", $"https://www.youtube.com/embed/{v}");

                return null;
            }

            if (host == "youtu.be")
            {
                var id = uri.AbsolutePath.Trim('/');
                var list = query["list"];
                if (list != null && IdPattern.IsMatch(list))
                    return ("YouTube", $"https://www.youtube.com/embed/videoseries?list={list}");
                if (IdPattern.IsMatch(id))
                    return ("YouTube", $"https://www.youtube.com/embed/{id}");
                return null;
            }

            // Spotify
            if (host == "open.spotify.com")
            {
                var m = SpotifyPath.Match(uri.AbsolutePath);
                if (m.Success)
                    return ("Spotify", $"https://open.spotify.com/embed/{m.Groups[1].Value}/{m.Groups[2].Value}");
                return null;
            }

            // SoundCloud
            if (host is "soundcloud.com" or "www.soundcloud.com")
            {
                var clean = uri.GetLeftPart(UriPartial.Path);
                return ("SoundCloud", $"https://w.soundcloud.com/player/?url={Uri.EscapeDataString(clean)}");
            }

            return null;
        }
    }
}