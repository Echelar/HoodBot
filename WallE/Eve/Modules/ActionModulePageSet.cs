﻿#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member (no intention to document this file)
namespace RobinHood70.WallE.Eve.Modules
{
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Text.RegularExpressions;
	using Newtonsoft.Json.Linq;
	using RobinHood70.WallE.Base;
	using RobinHood70.WallE.Design;
	using RobinHood70.WikiCommon;
	using RobinHood70.WikiCommon.RequestBuilder;
	using static RobinHood70.WikiCommon.Globals;

	public abstract class ActionModulePageSet<TInput, TOutput> : ActionModule, IPageSetGenerator
		where TInput : PageSetInput
		where TOutput : ITitle
	{
		#region Static Fields
		private static readonly Regex TooManyFinder = new Regex(@"Too many values .*?'(?<parameter>.*?)'.*?limit is (?<sizelimit>[0-9]+)", RegexOptions.Compiled);
		#endregion

		#region Fields
		private readonly HashSet<long> badRevisionIds = new HashSet<long>();
		private readonly Dictionary<string, string> converted = new Dictionary<string, string>();
		private readonly Dictionary<string, InterwikiTitleItem> interwiki = new Dictionary<string, InterwikiTitleItem>();
		private readonly Dictionary<string, string> normalized = new Dictionary<string, string>();
		private readonly Dictionary<string, PageSetRedirectItem> redirects = new Dictionary<string, PageSetRedirectItem>();
		private int offset;
		#endregion

		#region Constructors
		protected ActionModulePageSet(WikiAbstractionLayer wal)
			: base(wal)
		{
		}
		#endregion

		#region Public Properties
		public IGeneratorModule? Generator { get; protected set; }
		#endregion

		#region Protected Properties
		protected ContinueModule? ContinueModule { get; set; }

		protected int MaximumListSize { get; set; }

		protected bool PageSetDone { get; set; }
		#endregion

		#region Protected Virtual Properties
		protected virtual int CurrentListSize => this.MaximumListSize;
		#endregion

		#region Public Methods
		public virtual PageSetResult<TOutput> Submit(TInput input)
		{
			ThrowNull(input, nameof(input));
			this.MaximumListSize = this.Wal.MaximumPageSetSize;
			if (input.GeneratorInput != null)
			{
				this.Generator = this.Wal.ModuleFactory.CreateGenerator(input.GeneratorInput, this);
			}

			this.Wal.ClearWarnings();
			this.BeforeSubmit();
			this.ContinueModule = this.Wal.ModuleFactory.CreateContinue();
			this.ContinueModule.BeforePageSetSubmit(this);
			this.offset = 0;

			var pages = new List<TOutput>();
			do
			{
				var request = this.CreateRequest(input);
				var response = this.Wal.SendRequest(request);
				this.ParseResponse(response, pages);
				while (this.ContinueModule.Continues)
				{
					request = this.CreateRequest(input);
					response = this.Wal.SendRequest(request);
					this.ParseResponse(response, pages);
				}
			}
			while (!this.PageSetDone);

			return this.CreatePageSet(pages);
		}
		#endregion

		#region Protected Static Methods
		protected static string? FakeTitleFromId(long? pageId) => pageId == null ? null : '#' + pageId.Value.ToStringInvariant();
		#endregion

		#region Protected Methods
		protected void BuildRequest(Request request, TInput input)
		{
			ThrowNull(request, nameof(request));
			ThrowNull(input, nameof(input));
			request.Prefix = string.Empty;
			if (this.Generator != null)
			{
				request.Add("generator", this.Generator.Name);
				this.Generator.BuildRequest(request);
			}

			if (input.Values?.Count > this.offset)
			{
				var listSize = input.Values.Count - this.offset;
				this.PageSetDone = listSize <= this.CurrentListSize;
				if (!this.PageSetDone)
				{
					listSize = this.CurrentListSize;
				}

				var currentGroup = new List<string>(listSize);
				for (var i = 0; i < listSize; i++)
				{
					currentGroup.Add(input.Values[this.offset + i]);
				}

				request.Add(input.TypeName, currentGroup);
			}
			else
			{
				this.PageSetDone = true;
			}

			request
				.AddIf("converttitles", input.ConvertTitles, input.GeneratorInput != null || input.ListType == ListType.Titles)
				.AddIf("redirects", input.Redirects, input.ListType != ListType.RevisionIds);

			this.BuildRequestPageSet(request, input);
			request.Prefix = string.Empty;
			this.ContinueModule?.BuildRequest(request);
		}

		protected PageSetResult<TOutput> CreatePageSet(IReadOnlyList<TOutput> pages) => new PageSetResult<TOutput>(
			titles: pages,
			badRevisionIds: new List<long>(this.badRevisionIds),
			converted: this.converted,
			interwiki: this.interwiki,
			normalized: this.normalized,
			redirects: this.redirects);

		protected void GetPageSetNodes(JToken result)
		{
			ThrowNull(result, nameof(result));
			if (result["badrevids"] is JToken node)
			{
				foreach (var item in node)
				{
					if (item.First?["revid"] is JToken revid)
					{
						this.badRevisionIds.Add((long?)revid ?? 0);
					}
				}
			}

			AddToDictionary(result["converted"], this.converted);
			var links = result["interwiki"].GetInterwikiLinks();
			foreach (var link in links)
			{
				this.interwiki.Add(link.Title, link);
			}

			AddToDictionary(result["normalized"], this.normalized);
			var redirects = result["redirects"].GetRedirects(this.Wal.InterwikiPrefixes, this.SiteVersion);
			foreach (var item in redirects)
			{
				this.redirects.Add(item.Key, item.Value);
			}
		}

		protected void ParseResponse(string? response, IList<TOutput> pages)
		{
			var jsonResponse = ToJson(response);
			if (jsonResponse.Type == JTokenType.Object)
			{
				this.Deserialize(jsonResponse, pages);
			}
			else if (!(jsonResponse is JArray array && array.Count == 0))
			{
				throw new InvalidDataException();
			}
		}
		#endregion

		#region Protected Abstract Methods
		protected abstract void BuildRequestPageSet(Request request, TInput input);

		protected abstract TOutput GetItem(JToken result);
		#endregion

		#region Protected Override Methods
		protected override void DeserializeParent(JToken parent)
		{
			ThrowNull(parent, nameof(parent));
			base.DeserializeParent(parent);
			if (this.ContinueModule != null)
			{
				this.ContinueModule = this.ContinueModule.Deserialize(this.Wal, parent);

				// Was: !this.PageSetDone && !this.ContinueModule.BatchComplete && !this.ContinueModule.Continues, but that seems wrong. Maybe have been the result of the faulty BatchComplete in ContinueModule2.
				if (!this.PageSetDone && this.ContinueModule.BatchComplete)
				{
					this.offset += this.CurrentListSize;
				}
			}

			this.GetPageSetNodes(parent);
		}

		protected override bool HandleWarning(string from, string text)
		{
			if (from == this.Name)
			{
				var match = TooManyFinder.Match(text);
				if (match.Success)
				{
					var parameter = match.Groups["parameter"].Value;
					if (PageSetInput.AllTypes.Contains(parameter))
					{
						this.PageSetDone = false;
						this.MaximumListSize = int.Parse(match.Groups["sizelimit"].Value, CultureInfo.InvariantCulture);
						this.offset = this.MaximumListSize;
						return true;
					}
				}
			}

			return base.HandleWarning(from, text);
		}
		#endregion

		#region Protected Virtual Methods
		protected virtual void DeserializeResult(JToken result, IList<TOutput> pages)
		{
			ThrowNull(result, nameof(result));
			ThrowNull(pages, nameof(pages));
			foreach (var item in result)
			{
				pages.Add(this.GetItem(item));
			}
		}
		#endregion

		#region Private Static Methods
		private static void AddToDictionary(JToken? token, IDictionary<string, string> dict)
		{
			if (token != null)
			{
				foreach (var item in token)
				{
					dict.Add(item.MustHaveString("from"), item.MustHaveString("to"));
				}
			}
		}
		#endregion

		#region Private Methods
		private Request CreateRequest(TInput input)
		{
			ThrowNull(input, nameof(input));
			var request = this.CreateBaseRequest();
			request.Prefix = this.Prefix;
			this.BuildRequest(request, input);
			request.Prefix = string.Empty;

			return request;
		}

		private void Deserialize(JToken parent, IList<TOutput> pages)
		{
			this.DeserializeParent(parent);
			if (parent[this.Name] is JToken result && result.Type != JTokenType.Null)
			{
				this.DeserializeResult(result, pages);
			}
			else
			{
				throw WikiException.General("no-result", "The expected result node, " + this.Name + ", was not found.");
			}
		}
		#endregion
	}
}