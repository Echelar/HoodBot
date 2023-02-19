﻿namespace RobinHood70.HoodBot.Jobs
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using RobinHood70.HoodBot.Uesp;
	using RobinHood70.Robby;
	using RobinHood70.Robby.Design;
	using RobinHood70.Robby.Parser;
	using RobinHood70.WikiCommon;
	using RobinHood70.WikiCommon.Parser;

	internal sealed class LoreLinkResolver : ParsedPageJob
	{
		#region Fields
		private readonly UespNamespaceList nsList;
		private readonly PageCollection targetPages;
		private readonly PageCollection backlinkPages;

		#endregion

		#region Constructors
		[JobInfo("LoreLink Resolver")]
		public LoreLinkResolver(JobManager jobManager)
			: base(jobManager)
		{
			this.nsList = new UespNamespaceList(this.Site);
			this.backlinkPages = new PageCollection(this.Site, PageModules.Info);
			this.targetPages = new PageCollection(this.Site, PageModules.Info);
		}
		#endregion

		#region Protected Override Properties
		protected override string EditSummary => "Update FutureLink/LoreLink";
		#endregion

		#region Protected Override Methods
		protected override void BeforeLoadPages()
		{
			var limits =
				from ns in this.nsList
				where ns.IsGameSpace && !ns.IsPseudoNamespace
				select ns.BaseNamespace.Id;

			var linkTitles = new TitleCollection(this.Site);
			linkTitles.SetLimitations(LimitationType.OnlyAllow, limits);
			linkTitles.GetBacklinks("Template:Future Link", BacklinksTypes.EmbeddedIn);
			linkTitles.GetBacklinks("Template:Lore Link", BacklinksTypes.EmbeddedIn);

			// Book Header gets all book variants in one request.
			var books = new TitleCollection(this.Site);
			books.GetBacklinks("Template:Book Header", BacklinksTypes.EmbeddedIn);
			linkTitles.Remove(books);

			var pages = linkTitles.Load(PageModules.Default | PageModules.TranscludedIn);
			this.backlinkPages.AddRange(this.FromTransclusions(pages));
			for (var i = pages.Count - 1; i >= 0; i--)
			{
				var page = pages[i];
				if (!this.NamespaceCheck(page, page.Backlinks, new TitleCollection(this.Site)))
				{
					pages.RemoveAt(i);
				}
			}

			this.Pages.AddRange(pages);

			var findTitles = new TitleCollection(this.Site);
			foreach (var page in this.Pages)
			{
				var parser = new ContextualParser(page);
				foreach (var linkTemplate in parser.FindSiteTemplates("Lore Link"))
				{
					var ns = this.GetNamespace(linkTemplate, page);
					if (linkTemplate.Find($"{ns.Id}link")?.Value is NodeCollection overridden)
					{
						findTitles.TryAdd(TitleFactory.FromUnvalidated(this.Site, overridden.ToRaw().Trim()));
					}
					else if (linkTemplate.Find(1)?.Value is NodeCollection nodes)
					{
						var pageName = nodes.ToRaw();
						findTitles.TryAdd(TitleFactory.FromUnvalidated(ns.BaseNamespace, pageName));
						findTitles.TryAdd(TitleFactory.FromUnvalidated(ns.Parent, pageName));
						findTitles.TryAdd(TitleFactory.FromUnvalidated(this.Site[UespNamespaces.Lore], pageName));
					}
				}
			}

			this.targetPages.GetTitles(findTitles);
			this.targetPages.RemoveExists(false); // If commented out, changes everything to red links
		}

		protected override void LoadPages()
		{
			foreach (var page in this.Pages)
			{
				this.PageLoaded(page);
			}
		}

		protected override void Main()
		{
			this.Pages.RemoveChanged(false);
			foreach (var page in this.Pages)
			{
				Debug.WriteLine(page.AsLink());
			}

			base.Main();
		}

		protected override void ParseText(ContextualParser parser) => parser.Replace(node => this.LinkReplace(node, parser), false);
		#endregion

		#region Private Static Methods
		private Title? ResolveLink(params Title[] titles)
		{
			foreach (var title in titles)
			{
				if (this.targetPages!.Contains(title))
				{
					return title;
				}
			}

			return null;
		}

		private Title? ResolveTemplate(SiteTemplateNode linkTemplate, UespNamespace ns)
		{
			if (linkTemplate.Find($"{ns.Id}link")?.Value is NodeCollection overridden)
			{
				return TitleFactory.FromUnvalidated(this.Site, overridden.ToRaw());
			}

			if (linkTemplate.Find(1)?.Value is NodeCollection nodes)
			{
				var pageName = nodes.ToRaw();
				var fullName = TitleFactory.FromUnvalidated(this.Site, ns.Full + pageName);
				var parentName = TitleFactory.FromUnvalidated(this.Site, ns.Parent.DecoratedName + pageName);
				var loreName = TitleFactory.FromUnvalidated(this.Site, "Lore:" + pageName);
				return linkTemplate.TitleValue.PageNameEquals("Future Link") && ns.BaseNamespace == UespNamespaces.Lore
						? (Title)fullName
						: this.ResolveLink(fullName, parentName, loreName);
			}

			throw new InvalidOperationException("Template has no valid values.");
		}
		#endregion

		#region Private Methods

		private PageCollection FromTransclusions(IEnumerable<Title> titles)
		{
			var fullSet = PageCollection.Unlimited(this.Site, PageModules.Info | PageModules.TranscludedIn, true);
			var nextTitles = new TitleCollection(this.Site, titles);
			do
			{
				Debug.WriteLine($"Loading {nextTitles.Count} transclusion pages.");
				var loadPages = nextTitles.Load(PageModules.Info | PageModules.TranscludedIn);
				fullSet.AddRange(loadPages);
				nextTitles.Clear();
				foreach (var page in loadPages)
				{
					var ns = this.nsList.FromTitle(page);
					foreach (var backlink in page.Backlinks)
					{
						// Once we have a page that's out of the desired namespace, we don't need to follow it anymore, so don't try to load it.
						var title = backlink.Key;
						if (title.Namespace.IsSubjectSpace &&
							title.Namespace == ns.BaseNamespace &&
							!fullSet.Contains(title))
						{
							nextTitles.TryAdd(title);
						}
					}
				}
			}
			while (nextTitles.Count > 0);

			return fullSet;
		}

		private UespNamespace GetNamespace(SiteTemplateNode linkTemplate, Title title)
		{
			if (linkTemplate.Find("ns_base", "ns_id") is IParameterNode nsBase)
			{
				var lookup = nsBase.Value.ToValue();
				return this.nsList.GetAnyBase(lookup)
					?? throw new InvalidOperationException("ns_base invalid in " + WikiTextVisitor.Raw(linkTemplate));
			}

			return this.nsList.FromTitle(title);
		}

		private NodeCollection? LinkReplace(IWikiNode node, ContextualParser parser)
		{
			if (node is not SiteTemplateNode linkTemplate ||
				linkTemplate.TitleValue is not Title callTitle)
			{
				return null;
			}

			// Only assign one to a variable since it's boolean Lore/Future after this.
			var isLoreLink = callTitle.PageNameEquals("Lore Link");
			if (!isLoreLink && !callTitle.PageNameEquals("Future Link"))
			{
				return null;
			}

			var page = parser.Page;
			var linkNode = linkTemplate.Find(1);
			if (linkNode is null)
			{
				throw new InvalidOperationException($"Malformed link node {WikiTextVisitor.Raw(linkTemplate)} on page {page.FullPageName}.");
			}

			var ns = this.GetNamespace(linkTemplate, page);

			// If link doesn't resolve to anything OR
			// if this is a Future Link outside of Lore space that resolves to something IN Lore space, do nothing.
			if (
				this.ResolveTemplate(linkTemplate, ns) is not Title link ||
				(!isLoreLink && link.Namespace == UespNamespaces.Lore &&
				page.Namespace != UespNamespaces.Lore))
			{
				return null;
			}

			var displayText = linkTemplate.PrioritizedFind($"{ns.Id}display", "display", "2") is IParameterNode displayNode
				? displayNode.Value.ToRaw()
				: Title.ToLabelName(linkNode.Value.ToRaw());
			return new NodeCollection(parser.Factory, parser.Factory.LinkNodeFromParts(link.LinkName, displayText));
		}

		private bool NamespaceCheck(Page page, IReadOnlyDictionary<Title, BacklinksTypes> backlinks, TitleCollection titlesChecked)
		{
			var ns = this.nsList.FromTitle(page);
			foreach (var backlink in backlinks)
			{
				var title = backlink.Key;
				if (!titlesChecked.Contains(title))
				{
					titlesChecked.Add(title);
					if ((title.Namespace != ns.BaseNamespace && title.Namespace != MediaWikiNamespaces.User) ||
						(this.backlinkPages.TryGetValue(title, out var newBacklinks) &&
						!this.NamespaceCheck(page, newBacklinks.Backlinks, titlesChecked)))
					{
						return false;
					}
				}
			}

			return true;
		}
		#endregion
	}
}
