using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using log4net;
using Terraria;

namespace TerrariaTogether.Modules.Discord
{
	// sends messages to the webhook one at a time, in order, without blocking the game
	public class DiscordWebhook : IDisposable
	{
		public const string FileName = "TerrariaTogether_DiscordWebhook.txt";

		private readonly HttpClient http = new() { Timeout = TimeSpan.FromSeconds(10) };
		private readonly ILog logger;
		private readonly object chainLock = new();
		private Task chain = Task.CompletedTask;
		private string url;

		public DiscordWebhook(ILog logger)
		{
			this.logger = logger;
		}

		public static string FilePath => Path.Combine(Main.SavePath, "ModConfigs", FileName);

		public bool HasUrl => url != null;

		// reads the url from the file, only https discord webhooks are accepted
		public void LoadUrl()
		{
			url = null;
			if (!File.Exists(FilePath))
			{
				logger.Info($"discord: no webhook file at {FilePath}, nothing will be posted");
				return;
			}

			string text = File.ReadAllText(FilePath).Trim();
			if (!Uri.TryCreate(text, UriKind.Absolute, out Uri uri) || uri.Scheme != Uri.UriSchemeHttps
				|| !(uri.Host == "discord.com" || uri.Host == "discordapp.com") || !uri.AbsolutePath.StartsWith("/api/webhooks/"))
			{
				logger.Warn($"discord: {FileName} doesn't contain a discord webhook url, nothing will be posted");
				return;
			}
			url = text;
			logger.Info("discord: webhook loaded");
		}

		// one embed per post, so each message gets its own colored box with discord showing the date and time
		public void Post(string text, int color)
		{
			if (url == null)
				return;

			// mentions are switched off so nobody can ping @everyone from game chat
			string json = JsonSerializer.Serialize(new
			{
				embeds = new[] { new { description = text, color, timestamp = DateTime.UtcNow.ToString("o") } },
				allowed_mentions = new { parse = Array.Empty<string>() },
			});
			string target = url;
			// ponytail: one message per post, no batching or retry on rate limits (discord allows ~30 a minute)
			lock (chainLock)
				chain = chain.ContinueWith(_ => Send(target, json)).Unwrap();
		}

		private async Task Send(string target, string json)
		{
			try
			{
				using var body = new StringContent(json, Encoding.UTF8, "application/json");
				using HttpResponseMessage response = await http.PostAsync(target, body);
				if (!response.IsSuccessStatusCode)
					logger.Warn($"discord: webhook returned {(int)response.StatusCode}");
			}
			catch (Exception e)
			{
				// never let a network problem reach the game
				logger.Warn($"discord: post failed, {e.Message}");
			}
		}

		// stops player names and chat from turning into discord formatting
		public static string Escape(string text)
		{
			return Regex.Replace(text, @"([\\*_~`|>\[\]])", @"\$1");
		}

		public void Dispose()
		{
			http.Dispose();
		}
	}
}
