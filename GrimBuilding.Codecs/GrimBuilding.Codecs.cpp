#include "pch.h"

#include "GrimBuilding.Codecs.h"
#include "webp/encode.h"
#include "webp/decode.h"

using namespace System::Runtime::InteropServices;

void GrimBuildingCodecs::WebP::EncodeRGB(cli::array<Byte> ^bytes, const int width, const int height, const int stride, const float quality, cli::array<Byte> ^%output)
{
	pin_ptr<Byte> bytesPointer = &bytes[0];

	Byte *outputPointer;
	auto len = WebPEncodeRGB(bytesPointer, width, height, stride, quality, &outputPointer);

	if (!len)
	{
		output = nullptr;
		return;
	}

	output = gcnew cli::array<Byte>(len);
	Marshal::Copy(IntPtr(outputPointer), output, 0, len);
	WebPFree(outputPointer);
}

void GrimBuildingCodecs::WebP::EncodeRGBA(cli::array<Byte> ^bytes, const int width, const int height, const int stride, const float quality, cli::array<Byte> ^%output)
{
	pin_ptr<Byte> bytesPointer = &bytes[0];

	Byte *outputPointer;
	auto len = WebPEncodeRGBA(bytesPointer, width, height, stride, quality, &outputPointer);

	if (!len)
	{
		output = nullptr;
		return;
	}

	output = gcnew cli::array<Byte>(len);
	Marshal::Copy(IntPtr(outputPointer), output, 0, len);
	WebPFree(outputPointer);
}

bool GrimBuildingCodecs::WebP::Decode(cli::array<Byte> ^bytes, [System::Runtime::InteropServices::Out] bool %hasAlpha,
	[System::Runtime::InteropServices::Out] int %width, [System::Runtime::InteropServices::Out] int %height, [System::Runtime::InteropServices::Out] int %stride,
	[System::Runtime::InteropServices::Out] cli::array<Byte> ^%output)
{
	pin_ptr<Byte> bytesPointer = &bytes[0];

	WebPDecoderConfig config{};
	if (!WebPInitDecoderConfig(&config)) return false;

	if (WebPGetFeatures(bytesPointer, bytes->Length, &config.input) != VP8_STATUS_OK) return false;
	config.output.colorspace = config.input.has_alpha ? MODE_RGBA : MODE_RGB;
	config.options.use_threads = 1;
	// config.options.no_fancy_upsampling = 1;

	if (WebPDecode(bytesPointer, bytes->Length, &config) != VP8_STATUS_OK) return false;

	output = gcnew cli::array<Byte>(config.output.u.RGBA.size);
	Marshal::Copy(IntPtr(config.output.u.RGBA.rgba), output, 0, output->Length);

	hasAlpha = config.output.colorspace == MODE_RGBA;
	width = config.output.width;
	height = config.output.height;
	stride = config.output.u.RGBA.stride;

	WebPFreeDecBuffer(&config.output);

	return true;
}
