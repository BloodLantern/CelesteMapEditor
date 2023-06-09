#pragma once

namespace celeste
{
	struct PlayerInventory
	{
		static const PlayerInventory Prologue;
		static const PlayerInventory Default;
		static const PlayerInventory OldSite;
		static const PlayerInventory CH6End;
		static const PlayerInventory TheSummit;
		static const PlayerInventory Core;
		static const PlayerInventory Farewell;

		int dashes;
		bool dreamDash;
		bool backpack;
		bool noRefills;

		PlayerInventory(const int dashes = 1, const bool dreamDash = true, const bool backpack = true, const bool noRefills = false);
	};
}
