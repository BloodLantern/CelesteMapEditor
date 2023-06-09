#include "player_inventory.hpp"

const celeste::PlayerInventory celeste::PlayerInventory::Prologue(0, false);
const celeste::PlayerInventory celeste::PlayerInventory::Default;
const celeste::PlayerInventory celeste::PlayerInventory::OldSite(1, false);
const celeste::PlayerInventory celeste::PlayerInventory::CH6End(2);
const celeste::PlayerInventory celeste::PlayerInventory::TheSummit(2, true, false);
const celeste::PlayerInventory celeste::PlayerInventory::Core(2, true, true, true);
const celeste::PlayerInventory celeste::PlayerInventory::Farewell(1, true, false);

celeste::PlayerInventory::PlayerInventory(const int dashes, const bool dreamDash, const bool backpack, const bool noRefills)
	: dashes(dashes), dreamDash(dreamDash), backpack(backpack), noRefills(noRefills)
{
}
