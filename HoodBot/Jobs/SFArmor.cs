﻿namespace RobinHood70.HoodBot.Jobs
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Text;
	using RobinHood70.CommonCode;
	using RobinHood70.HoodBot.Jobs.JobModels;
	using RobinHood70.Robby;
	using RobinHood70.Robby.Design;
	using RobinHood70.Robby.Parser;

	internal sealed class SFArmor : CreateOrUpdateJob<List<CsvRow>>
	{
		#region Constructors
		[JobInfo("SF Armor")]
		public SFArmor(JobManager jobManager)
			: base(jobManager)
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
		}
		#endregion

		#region Protected Override Properties
		protected override string? Disambiguator => "armor";

		protected override string EditSummary => "Create armor page";
		#endregion

		#region Protected Override Methods
		protected override bool IsValid(ContextualParser parser, List<CsvRow> item) => parser.FindSiteTemplate("Item Summary") is not null;

		protected override IDictionary<Title, List<CsvRow>> LoadItems()
		{
			var items = new Dictionary<Title, List<CsvRow>>();
			var csv = new CsvFile();
			csv.Load(LocalConfig.BotDataSubPath("Starfield/Armors.csv"), true, Encoding.GetEncoding(1252));
			foreach (var row in csv)
			{
				var name = row["Name"];
				var title = TitleFactory.FromUnvalidated(this.Site, "Starfield:" + name);
				if (name.Length > 0 && !name.Contains('_', StringComparison.Ordinal))
				{
					var itemList = items.TryGetValue(title, out var list) ? list : new List<CsvRow>();
					itemList.Add(row);
					items[title] = itemList;
				}
			}

			return items;
		}

		protected override string NewPageText(Title title, List<CsvRow> itemList)
		{
			var sb = new StringBuilder();
			string? firstType = null;
			foreach (var item in itemList)
			{
				var nameEdid = item["Name"] + '/' + item["EditorID"].Trim();
				var itemType =
					nameEdid.Contains("Clothes", StringComparison.Ordinal) ? "Apparel" :
					nameEdid.Contains("Spacesuit", StringComparison.Ordinal) ? "Spacesuit" :
					nameEdid.Contains("Pack", StringComparison.Ordinal) ? "Pack" :
					nameEdid.Contains("Helmet", StringComparison.Ordinal) ? "Helmet" :
					null;

				if (itemType is null)
				{
					Debug.WriteLine(nameEdid);
				}

				firstType ??= itemType;
				sb
					.Append("{{NewLine}}\n")
					.Append("{{Item Summary\n")
					.Append($"|objectid={item["FormID"][2..]}\n")
					.Append($"|editorid={item["EditorID"].Trim()}\n")
					.Append($"|type={itemType}\n")
					.Append("|image=\n")
					.Append("|imgdesc=\n")
					.Append($"|weight={item["Weight"]}\n")
					.Append($"|value={item["Value"]}\n")
					.Append("|physical={{Huh}}\n")
					.Append("|energy={{Huh}}\n")
					.Append("|electromagnetic={{Huh}}\n")
					.Append("|radiation={{Huh}}\n")
					.Append("|thermal={{Huh}}\n")
					.Append("|airborne={{Huh}}\n")
					.Append("|corrosive={{Huh}}\n")
					.Append("}}\n");
			}

			var link = firstType switch
			{
				"Apparel" => "piece of [[Starfield:Apparel|apparel]]",
				"Helmet" => "[[Starfield:Helmet|helmet]]",
				"Pack" => "[[Starfield:Pack|pack]]",
				"Spacesuit" => "[[Starfield:Spacesuit|spacesuit]]",
				_ => null,
			};

			return $"{{{{Trail|Items|{firstType}}}}}" + sb.ToString()[12..] + $"The [[Starfield:{title.PageName}|]] is a {link}.\n\n{{{{Stub|{firstType}}}}}";
		}
		#endregion
	}
}