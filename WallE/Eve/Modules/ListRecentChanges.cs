﻿#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member (no intention to document this file)
namespace RobinHood70.WallE.Eve.Modules
{
	using System.Collections.Generic;
	using Newtonsoft.Json.Linq;
	using RobinHood70.WallE.Base;
	using RobinHood70.WikiCommon.RequestBuilder;
	using static RobinHood70.WikiCommon.Globals;

	internal class ListRecentChanges : ListModule<RecentChangesInput, RecentChangesItem>, IGeneratorModule
	{
		#region Static Fields
		private static readonly HashSet<string> KnownProps = new HashSet<string>
		{
			"actionhidden", "anon", "bot", "comment", "commenthidden", "logaction", "logid", "logtype", "minor", "new", "newlen", "ns", "old_revid", "oldlen", "pageid", "parsedcomment", "patroltoken", "patrolled", "rcid", "redirect", "revid", "suppressed", "tags", "timestamp", "title", "type", "user", "userhidden", "userid",
		};
		#endregion

		#region Constructors
		public ListRecentChanges(WikiAbstractionLayer wal, RecentChangesInput input)
			: this(wal, input, null)
		{
		}

		public ListRecentChanges(WikiAbstractionLayer wal, RecentChangesInput input, IPageSetGenerator? pageSetGenerator)
			: base(wal, input, pageSetGenerator)
		{
		}
		#endregion

		#region Protected Override Properties
		public override int MinimumVersion => 109;

		public override string Name => "recentchanges";
		#endregion

		#region Protected Override Methods
		protected override string Prefix => "rc";
		#endregion

		#region Public Static Methods
		public static ListRecentChanges CreateInstance(WikiAbstractionLayer wal, IGeneratorInput input, IPageSetGenerator pageSetGenerator) =>
			input is RecentChangesInput listInput
				? new ListRecentChanges(wal, listInput, pageSetGenerator)
				: throw InvalidParameterType(nameof(input), nameof(RecentChangesInput), input.GetType().Name);
		#endregion

		#region Protected Override Methods
		protected override void BuildRequestLocal(Request request, RecentChangesInput input)
		{
			ThrowNull(request, nameof(request));
			ThrowNull(input, nameof(input));
			request
				.Add("start", input.Start)
				.Add("end", input.End)
				.AddIf("dir", "newer", input.Start < input.End || input.SortAscending)
				.Add("namespace", input.Namespaces)
				.AddIfNotNull(input.ExcludeUser ? "excludeuser" : "user", input.User)
				.AddIfNotNull("tag", input.Tag)
				.AddFlags("type", input.Types)
				.AddFilterPiped("show", "anon", input.FilterAnonymous)
				.AddFilterPiped("show", "bot", input.FilterBots)
				.AddFilterPiped("show", "minor", input.FilterMinor)
				.AddFilterPiped("show", "patrolled", input.FilterPatrolled)
				.AddFilterPiped("show", "redirect", input.FilterRedirects)
				.AddFlags("prop", input.Properties)
				.AddIf("token", TokensInput.Patrol, input.GetPatrolToken && this.SiteVersion < 124)
				.Add("toponly", input.TopOnly)
				.Add("limit", this.Limit);
		}

		protected override RecentChangesItem? GetItem(JToken result)
		{
			// Same bug as ListLogEvents
			if (result == null || result["type"] == null)
			{
				return null;
			}

			var item = new RecentChangesItem(
				ns: (int)result.MustHave("ns"),
				title: result.MustHaveString("title"),
				flags: result.GetFlags(
					("bot", RecentChangesFlags.Bot),
					("minor", RecentChangesFlags.Minor),
					("new", RecentChangesFlags.New),
					("redirect", RecentChangesFlags.Redirect)),
				id: (long?)result["rcid"] ?? 0,
				newLength: (int?)result["newlen"] ?? 0,
				oldLength: (int?)result["oldlen"] ?? 0,
				oldRevisionId: (long?)result["old_revid"] ?? 0,
				patrolToken: (string?)result["patroltoken"],
				recentChangeType: (string?)result["type"],
				revisionId: (long?)result["revid"] ?? 0,
				tags: result["tags"].ToReadOnlyList<string>());
			result.ParseLogEvent(item, "log", KnownProps, false);

			return item;
		}
		#endregion
	}
}