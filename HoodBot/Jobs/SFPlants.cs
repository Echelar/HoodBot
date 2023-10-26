namespace RobinHood70.HoodBot.Jobs
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using RobinHood70.CommonCode;
	using RobinHood70.HoodBot.Jobs.JobModels;
	using RobinHood70.Robby;
	using RobinHood70.Robby.Design;
	using RobinHood70.Robby.Parser;
	using RobinHood70.WikiCommon.Parser;

	internal sealed class SFPlants : CreateOrUpdateJob<SFPlants.Plant>
	{
		#region Constructors
		[JobInfo("Plants", "Starfield")]
		public SFPlants(JobManager jobManager)
			: base(jobManager)
		{
		}
		#endregion

		#region Protected Override Properties
		protected override string? Disambiguator => "flora";
		#endregion		

		#region Protected Override Methods
		protected override string GetEditSummary(Page page) => "Create/update flora page";

		protected override bool IsValid(ContextualParser parser, Plant item) => parser.FindSiteTemplate("Flora Summary") is not null;

		protected override IDictionary<Title, Plant> LoadItems()
		{
			return this.ReadFile(this.GetTitleMap());
		}

		protected override void PageLoaded(ContextualParser parser, Plant item)
		{
			var cs = parser.FindSiteTemplate("Flora Summary");
			if (cs is not null)
			{
				// TODO: Updates
			}

			parser.UpdatePage();
		}

		protected override string NewPageText(Title title, Plant item)
		{
			var sb = new StringBuilder();
			sb
				.Append("{{Flora Summary\n")
				.Append("|formid=\n")
				.Append("|editorid=\n")
				.Append("|planets=\n")
				.Append("|biomes=\n")
				.Append("|species=\n")
				.Append("|image=\n")
				.Append("|imgdesc=\n")
				.Append("}}\n")
				.Append("{{NewLine}}\n")
				.Append("{{Stub|Flora}}\n")
				;

			return sb.ToString();
		}
		#endregion

		#region Private Static Methods
		private static void UpdateTemplate(SiteTemplateNode template, CsvRow row)
		{
			template.Update("planet", row["Planet"]);
			template.Update("biomes", "\n* " + row["Biomes"].Replace(", ", "\n* ", StringComparison.Ordinal));
			template.Update("resource", row["Resource"].Split(" (", 2, StringSplitOptions.None)[0]);
		}

		private static void AddVariants(ContextualParser parser, Plant item)
		{
			// TODO: Add variants
		}
		#endregion

		#region Private Methods

		private Dictionary<string, Title> GetTitleMap()
		{
			var titleMap = new Dictionary<string, Title>(StringComparer.Ordinal);
			var existing = new PageCollection(this.Site);
			existing.GetBacklinks("Template:Flora Summary");
			foreach (var page in existing)
			{
				var parser = new ContextualParser(page);
				var template = parser.FindSiteTemplate("Flora Summary");
				if (template is not null)
				{
					var name = template.Find("titlename")?.Value.ToRaw() ?? page.Title.LabelName();
					titleMap.Add(name, page.Title);
				}
			}

			return titleMap;
		}

		/// <summary>
		/// Reads data file [StarfieldPlants.csv] and produces the dictionary of records to process.
		/// Duplicate plants on subsequent rows are added to the Variants.
		/// </summary>
		/// <param name="titleMap">Title Map from wiki of existing flora pages.</param>
		/// <returns>Dictionary of <see cref="Title"/> => <see cref="Plant"/>.</returns>
		private Dictionary<Title, Plant> ReadFile(Dictionary<string, Title> titleMap)
		{
			var file = new CsvFile();
			file.Load(LocalConfig.BotDataSubPath(@"Starfield\StarfieldPlants.csv"), true);

			var plants = new Dictionary<Title, Plant>();

			foreach (var row in file)
			{
				var name = row["Plant"];
				if (!titleMap.TryGetValue(name, out var title))
				{
					title = TitleFactory.FromUnvalidated(this.Site, "Starfield:" + name);
				}

				Plant? plant = null;
				if (!plants.ContainsKey(title))
				{
					plant = new Plant(new List<PlantVariant>());
					plants.Add(title, plant);
				}
				else
				{
					plant = plants[title];
				}

				plant.Variants.Add(new PlantVariant(
					row["Planet"],
					row["Biomes"].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
					row["Resource"]));
			}

			return plants;
		}
		#endregion

		#region Internal Records
		internal sealed record Plant(List<PlantVariant> Variants);

		internal sealed record PlantVariant(string Planet, string[] Biomes, string Resource);
		#endregion
	}
}
