#[compute]
#version 450

// Invocations in the (x, y, z) dimension
layout(local_size_x = 8, local_size_y = 8, local_size_z = 1) in;

layout(rgba16f, set = 0, binding = 0) uniform image2D uSceneColorTexture;
layout(rgba16f, set = 0, binding = 1) uniform image2D uCapturedFrameTexture;
layout(rgba16f, set = 0, binding = 2) uniform image2D uVelocityTexture;
layout(rgba16f, set = 0, binding = 3) uniform image2D uFragOut;

layout(push_constant, std430) uniform Params {
	vec2 raster_size;
	float enable_moshing;
	float blend;
} params;

void main() {
	ivec2 uv = ivec2(gl_GlobalInvocationID.xy);
	ivec2 size = ivec2(params.raster_size);

	if (uv.x >= size.x || uv.y >= size.y) {
		return;
	}

    vec2 velocity = imageLoad(uVelocityTexture, uv).rg * params.enable_moshing;
    ivec2 moshedUv = uv + ivec2(velocity * vec2(size));

    vec4 sceneColor = imageLoad(uSceneColorTexture, uv);

    if (moshedUv.x >= size.x || moshedUv.x < 0 || moshedUv.y >= size.y || moshedUv.y < 0) {
        imageStore(uCapturedFrameTexture, uv, sceneColor);
        imageStore(uFragOut, uv, sceneColor);
        return;
    }

    vec4 capturedFrameColor = imageLoad(uCapturedFrameTexture, moshedUv);
    imageStore(uCapturedFrameTexture, uv, capturedFrameColor);

    vec4 resultColor = mix(sceneColor, capturedFrameColor, params.blend);

	imageStore(uFragOut, uv, resultColor);
}