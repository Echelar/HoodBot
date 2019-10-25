﻿#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member (no intention to document this file)
namespace RobinHood70.WallE.Eve.Modules
{
	using System.Collections.Generic;
	using Newtonsoft.Json.Linq;
	using RobinHood70.WallE.Base;
	using RobinHood70.WallE.Design;
	using RobinHood70.WikiCommon.RequestBuilder;
	using static RobinHood70.WikiCommon.Globals;

	// TODO: Monitor the links below and see if this is ultimately implemented as a list or with Special:UploadStash/$key as a valid page title, then adapt code as needed.
	// This behaves more like a List module, but does not support limits or continuation, and is therefore internally treated as just a normal query module. It is not (and should not be made into) a property module internally. The entire PHP version of the module will likely be re-written in the future.
	// See https://phabricator.wikimedia.org/T38220 and https://phabricator.wikimedia.org/T89971.
	internal class PropStashImageInfo : QueryModule<StashImageInfoInput, IList<ImageInfoItem>>
	{
		#region Constructors
		public PropStashImageInfo(WikiAbstractionLayer wal, StashImageInfoInput input)
			: base(wal, input, null)
		{
		}
		#endregion

		#region Public Override Properties
		public override int MinimumVersion => 117;

		public override string Name => "stashimageinfo";
		#endregion

		#region Protected Override Properties
		protected override string ModuleType => "prop";

		protected override string Prefix => "sii";
		#endregion

		#region Protected Override Methods
		protected override void BuildRequestLocal(Request request, StashImageInfoInput input)
		{
			ThrowNull(request, nameof(request));
			ThrowNull(input, nameof(input));
			var prop = FlagFilter
				.Check(this.SiteVersion, input.Properties)
				.FilterBefore(123, StashImageProperties.CanonicalTitle | StashImageProperties.CommonMetadata | StashImageProperties.ExtMetadata)
				.Value;
			request
				.AddFlags("prop", prop)
				.Add(this.SiteVersion < 118 ? "sessionkey" : "filekey", input.FileKeys)
				.AddIf("urlwidth", input.UrlWidth, prop.HasFlag(StashImageProperties.Url) && input.UrlWidth > 0)
				.AddIf("urlheight", input.UrlHeight, prop.HasFlag(StashImageProperties.Url) && input.UrlHeight > 0)
				.AddIfNotNullIf("urlparam", input.UrlParameter, this.SiteVersion >= 118);
		}

		protected override void DeserializeResult(JToken result)
		{
			ThrowNull(result, nameof(result));
			this.Output = new List<ImageInfoItem>();
			foreach (var item in result)
			{
				var imageInfo = JTokenImageInfo.ParseImageInfo(item, new ImageInfoItem());
				this.Output.Add(imageInfo);
			}
		}
		#endregion
	}
}
