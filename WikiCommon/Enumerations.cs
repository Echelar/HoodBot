﻿// This namespace houses common elements between WallE and Robby. It's a bit of a kludge to do it this way, and for so few, I may go back to the double defines (i.e., enum members in Robby are defined as equivalent enum members from WallE and cast as needed). This has the problem of name overlap, though, requiring either that unique names are used, or that fully-qualified names are used in some instances.
namespace RobinHood70.WikiCommon
{
	using System;

	/// <summary>The types of backlinks.</summary>
	[Flags]
	public enum BacklinksTypes
	{
		/// <summary>Regular links, including redirects.</summary>
		Backlinks = 1,

		/// <summary>Transclusions</summary>
		EmbeddedIn = 1 << 1,

		/// <summary>File/image links</summary>
		ImageUsage = 1 << 2,

		/// <summary>All</summary>
		All = Backlinks | EmbeddedIn | ImageUsage
	}

	/// <summary>Block options. These represent options which can be set by the blocking user or returned by the wiki when retrieving block info.</summary>
	/// <remarks>This enumeration does not include values that can only be altered by the wiki itself, such as whether the block was automatic.</remarks>
	[Flags]
	public enum BlockFlags
	{
		/// <summary>Default value when unspecified</summary>
		None = 0,

		/// <summary>User is allowed to edit their own talk page.</summary>
		AllowUserTalk = 1,

		/// <summary>Only block IP when editing anonymously (i.e., force user to create an account or log in).</summary>
		AnonymousOnly = 1 << 1,

		/// <summary>Automatically block the IP address last used by this user, and any subsequent IPs they try to login from.</summary>
		AutoBlock = 1 << 2,

		/// <summary>Hide the user name from all lists and logs, unless the viewer has hideuser permissions.</summary>
		Hidden = 1 << 3,

		/// <summary>Disable account creation for user.</summary>
		NoCreate = 1 << 4,

		/// <summary>Disable e-mail function for user.</summary>
		NoEmail = 1 << 5,
	}

	/// <summary>The page groupings within a category.</summary>
	[Flags]
	public enum CategoryTypes
	{
		/// <summary>Regular pages</summary>
		Page = 1,

		/// <summary>Subcategories</summary>
		Subcat = 1 << 1,

		/// <summary>Files/images</summary>
		File = 1 << 2,

		/// <summary>All</summary>
		All = Page | Subcat | File
	}

	/// <summary>Represents a tristate filter, where the options are to show everything, show only the selected items, or hide the selected items.</summary>
	/// <remarks>This is an alias for Tristate, but is much clearer in its intent than having True/False/Unknown values.</remarks>
	public enum Filter
	{
		/// <summary>No filter.</summary>
		Any = Tristate.Unknown,

		/// <summary>Only include these results (e.g., redirects only).</summary>
		Only = Tristate.True,

		/// <summary>Filter out these results (e.g., everything except redirects).</summary>
		Exclude = Tristate.False,
	}

	/// <summary>The methods to use to purge pages.</summary>
	public enum PurgeMethod
	{
		/// <summary>Only purge the specified pages.</summary>
		Normal,

		/// <summary>Purge the specified pages and anything that links to them.</summary>
		LinkUpdate,

		/// <summary>Purge the specified pages and anything that links to or recursively transcludes them.</summary>
		RecursiveLinkUpdate
	}

	/// <summary>Options to show or hide for Recent Changes functions.</summary>
	[Flags]
	public enum RecentChangesFilters
	{
		/// <summary>Use default behaviour and show everything.</summary>
		None = 0,

		/// <summary>Filter edits based on whether they were made by an anonymous (IP) user or a logged-in user.</summary>
		Anonymous = 1,

		/// <summary>Filter edits based on whether they were made by a bot.</summary>
		Bot = 1 << 1,

		/// <summary>Filter edits based on whether they're minor.</summary>
		Minor = 1 << 2,

		/// <summary>Filter edits based on patrol status.</summary>
		Patrolled = 1 << 3,

		/// <summary>Filter edits based on whether they're redirects.</summary>
		Redirect = 1 << 4,
	}

	/// <summary>The types of entries that appear in Recent Changes.</summary>
	[Flags]
	public enum RecentChangesTypes
	{
		/// <summary>Default value when unspecified</summary>
		None = 0,

		/// <summary>A normal edit</summary>
		Edit = 1,

		/// <summary>An external edit</summary>
		External = 1 << 1,

		/// <summary>A new page</summary>
		New = 1 << 2,

		/// <summary>A log entry</summary>
		Log = 1 << 3,

		/// <summary>All</summary>
		All = Edit | External | New | Log
	}

	/// <summary>Represents a binary value which also allows for an unknown state.</summary>
	/// <remarks>This is used in preference to a <see cref="Nullable{Boolean}"/> due to the fact that it is both smaller and makes the code much clearer.</remarks>
	public enum Tristate
	{
		/// <summary>An unknown or unassigned value.</summary>
		Unknown = 0,

		/// <summary>The value is True.</summary>
		True,

		/// <summary>The value is false.</summary>
		False
	}

	/// <summary>Represents the possible search types.</summary>
	public enum WhatToSearch
	{
		/// <summary>Search titles only</summary>
		Title,

		/// <summary>Search text</summary>
		Text,

		/// <summary>Search text for near matches</summary>
		NearMatch
	}
}