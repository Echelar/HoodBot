namespace HoodBotTestProject
{
	using System.Diagnostics;
	using RobinHood70.Robby;
	using RobinHood70.Robby.Design;
	using RobinHood70.WallE.Test;
	using RobinHood70.WikiCommon;
	using Xunit;

	public class TitleParserUnitTest
	{
		#region Fields
		private static readonly Site Site = new Site(new WikiAbstractionLayer());
		#endregion

		#region Static Constructor
		static TitleParserUnitTest() => Site.Login("RobinHood70", string.Empty);
		#endregion

		#region Public Static Properties
		public static TheoryData<string, string?, int, string, string?> Data =>
			new TheoryData<string, string?, int, string, string?>
			{
				{ "Test", null, MediaWikiNamespaces.Main, "Test", null },
				{ "File:Test.png", null, MediaWikiNamespaces.File, "Test.png", null },
				{ "Image:Test.png", null, MediaWikiNamespaces.File, "Test.png", null },
				{ "Talk:File:Test.png", null, MediaWikiNamespaces.Talk, "File:Test.png", null },
				{ "en", null, MediaWikiNamespaces.Main, "en", null },
				{ ":en", null, MediaWikiNamespaces.Main, "en", null },
				{ ":en:", "en", MediaWikiNamespaces.Main, "Main Page", null },
				{ "en:Test", "en", MediaWikiNamespaces.Main, "Test", null },
				{ "en:Image:Test.png", "en", MediaWikiNamespaces.File, "Test.png", null },
				{ ":en::Image:Test.png#Everything", "en", MediaWikiNamespaces.Main, "Image:Test.png", "Everything" },
				{ "MediaWikiWiki:File:Test.png", "MediaWikiWiki", MediaWikiNamespaces.Main, "File:Test.png", null },
			};
		#endregion

		#region Public Methods
		[Theory]
		[MemberData(nameof(Data))]
		public void Test1(string input, string? iw, int ns, string page, string? fragment)
		{
			Debug.WriteLine(input);
			var test = new TitleParser(Site, input);
			Assert.Equal(iw, test.Interwiki?.Prefix);
			Assert.Equal(ns, test.Namespace.Id);
			Assert.Equal(page, test.PageName);
			Assert.Equal(fragment, test.Fragment);
		}
		#endregion
	}
}