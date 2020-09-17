#pragma once

using namespace System;

namespace GrimBuilding
{
	namespace Codecs
	{
		public ref class WebP
		{
		public:
			static void EncodeRGB(cli::array<Byte> ^bytes, const int width, const int height, const int stride, const float quality, [System::Runtime::InteropServices::Out] cli::array<Byte> ^%output);
			static void EncodeRGBA(cli::array<Byte> ^bytes, const int width, const int height, const int stride, const float quality, [System::Runtime::InteropServices::Out] cli::array<Byte> ^%output);

			static bool Decode(cli::array<Byte> ^bytes, [System::Runtime::InteropServices::Out] bool %hasAlpha,
				[System::Runtime::InteropServices::Out] int %width, [System::Runtime::InteropServices::Out] int %height, [System::Runtime::InteropServices::Out] int %stride,
				[System::Runtime::InteropServices::Out] cli::array<Byte> ^%output);
		};
	}
}