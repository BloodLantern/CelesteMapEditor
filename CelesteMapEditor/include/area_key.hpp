#pragma once

#include <string>

#include "area_mode.hpp"

namespace celeste
{
	struct AreaKey
	{
		static const AreaKey None;
		static const AreaKey Default;

		int id;
		AreaMode mode;

		AreaKey() = default;
		AreaKey(const int id, const AreaMode mode = AreaMode::Normal);

		operator std::string();
	};

	inline bool operator==(AreaKey left, AreaKey right);
	inline bool operator!=(AreaKey left, AreaKey right);
}
