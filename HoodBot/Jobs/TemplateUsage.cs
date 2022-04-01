﻿namespace RobinHood70.HoodBot.Jobs
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using RobinHood70.CommonCode;

	using RobinHood70.Robby;
	using RobinHood70.Robby.Design;
	using RobinHood70.Robby.Parser;
	using RobinHood70.WikiCommon;
	using RobinHood70.WikiCommon.Parser;

	public class TemplateUsage : WikiJob
	{
		#region Fields
		private readonly string saveLocation;
		private readonly IReadOnlyList<string> originalTemplateNames;
		private readonly bool respectRedirects;
		private readonly List<string> headerOrder = new();
		#endregion

		#region Constructors
		[JobInfo("Template Usage")]
		public TemplateUsage(
			JobManager jobManager,
			IEnumerable<string> templateNames,
			[JobParameter(DefaultValue = true)] bool respectRedirects,
			[JobParameterFile(Overwrite = true, DefaultValue = @"%BotData%\%templateName%.txt")] string location)
			: base(jobManager)
		{
			location.ThrowNull();
			this.respectRedirects = respectRedirects;
			List<string> allTemplateNames = new();
			foreach (var templateName in templateNames.NotNull())
			{
				allTemplateNames.AddRange(templateName.Split(TextArrays.Pipe));
			}

			this.saveLocation = location.Replace("%templateName%", Globals.SanitizeFilename(allTemplateNames[0]), StringComparison.Ordinal);
			this.originalTemplateNames = allTemplateNames;
			this.ProgressMaximum = 2;
		}
		#endregion

		#region Protected Override Methods
		protected override void Main()
		{
			// TODO: Handle case where a redirect was provided rather than the base...doesn't seem to be working right now. (Should it? If not, at least spit out an error.)
			// CONSIDER: Adapt this and/or the parser to handle relative templates like {{/Template}} and {{../Template}}.
			TitleCollection templates = new(this.Site, MediaWikiNamespaces.Template, this.originalTemplateNames);
			TitleCollection allTemplateNames;
			if (this.respectRedirects)
			{
				this.StatusWriteLine("Loading template redirects");
				templates = new TitleCollection(this.Site, this.FollowRedirects(templates));
				allTemplateNames = BuildRedirectList(templates);
				this.ProgressMaximum++;
				this.Progress++;
			}
			else
			{
				allTemplateNames = templates;
			}

			this.StatusWriteLine("Loading pages");
			PageCollection results = PageCollection.Unlimited(this.Site);
			results.GetPageTranscludedIn(templates);
			this.Progress++;
			this.StatusWriteLine("Exporting");
			results.Sort();
			this.ExportTemplates(allTemplateNames, results);
			this.Progress++;
		}
		#endregion

		#region Private Static Methods
		private static TitleCollection BuildRedirectList(TitleCollection titles)
		{
			TitleCollection retval = new(titles.Site, titles);

			// Loop until nothing new is added.
			HashSet<Title> titlesToCheck = new(titles);
			HashSet<Title> alreadyChecked = new();
			do
			{
				foreach (var title in titlesToCheck)
				{
					retval.GetBacklinks(title.FullPageName, BacklinksTypes.Backlinks, true, Filter.Only);
				}

				alreadyChecked.UnionWith(titlesToCheck);
				titlesToCheck.Clear();
				titlesToCheck.UnionWith(retval);
				titlesToCheck.ExceptWith(alreadyChecked);
			}
			while (titlesToCheck.Count > 0);

			return retval;
		}

		private PageCollection FollowRedirects(TitleCollection titles)
		{
			PageCollection originalsFollowed = PageCollection.Unlimited(this.Site, PageModules.None, true);
			originalsFollowed.GetTitles(titles);

			return originalsFollowed;
		}

		#endregion

		#region Private Methods
		private void ExportTemplates(IReadOnlyCollection<Title> allNames, PageCollection pages)
		{
			var templates = this.ExtractTemplates(allNames, pages);
			if (templates.Count == 0)
			{
				this.StatusWriteLine("No template calls found!");
				return;
			}

			try
			{
				this.WriteFile(templates, this.saveLocation);
				this.StatusWriteLine("File saved to " + this.saveLocation);
			}
			catch (IOException e)
			{
				this.StatusWriteLine("Couldn't save file to " + this.saveLocation);
				this.StatusWriteLine(e.Message);
			}
		}

		private List<(Title Page, ITemplateNode Template)> ExtractTemplates(IReadOnlyCollection<Title> allNames, PageCollection pages)
		{
			List<(Title Page, ITemplateNode Template)> templates = new();
			Dictionary<string, string> paramTranslator = new(StringComparer.Ordinal); // TODO: Empty dictionary for now, but could be pre-populated to translate synonyms to a consistent name. Similarly, name comparison can be case-sensitive or not. Need to find a useful way to do those.
			foreach (var page in pages)
			{
				ContextualParser parser = new(page);
				foreach (var template in parser.FindAll<SiteTemplateNode>())
				{
					if (allNames.Contains(template.TitleValue))
					{
						templates.Add((page, template));
						foreach (var (name, _) in template.GetResolvedParameters())
						{
							if (paramTranslator.TryAdd(name, name))
							{
								this.headerOrder.Add(name);
							}
						}
					}
				}
			}

			return templates;
		}

		private void WriteFile(List<(Title Page, ITemplateNode Template)> results, string location)
		{
			CsvFile csvFile = new() { EmptyFieldText = " " };
			List<string> output = new(this.headerOrder.Count + 2)
			{
				"Page",
				"Template Name"
			};
			output.AddRange(this.headerOrder);
			csvFile.Header = output;

			foreach (var template in results)
			{
				var row = csvFile.Add(template.Page.FullPageName, template.Template.GetTitleText());
				foreach (var (name, parameter) in template.Template.GetResolvedParameters())
				{
					// For now, we're assuming that trimming trailing lines from anon parameters is desirable, but could be made optional if needed.
					var value = parameter.Value.ToRaw();
					row[name] = parameter.Anonymous ? value.TrimEnd(TextArrays.NewLineChars) : value.Trim();
				}
			}

			csvFile.WriteFile(location);
		}
		#endregion
	}
}