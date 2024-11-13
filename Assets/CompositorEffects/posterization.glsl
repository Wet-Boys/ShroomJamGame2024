#[compute]
#version 450

// Invocations in the (x, y, z) dimension
layout(local_size_x = 8, local_size_y = 8, local_size_z = 1) in;

layout(rgba16f, set = 0, binding = 0) uniform image2D uSceneColorTexture;

layout(push_constant, std430) uniform Params {
	vec2 raster_size;
	vec2 reserved;
} params;

void main() {
	ivec2 uv = ivec2(gl_GlobalInvocationID.xy);
	ivec2 size = ivec2(params.raster_size);

	if (uv.x >= size.x || uv.y >= size.y) {
		return;
	}

    vec4 color = imageLoad(uSceneColorTexture, uv);

    float greyscale = max(color.r, max(color.g, color.b));

    float lower = floor(greyscale * params.reserved.x) / params.reserved.x;
    float lowerDiff = abs(greyscale - lower);

    float upper = ceil(greyscale * params.reserved.x) / params.reserved.x;
    float upperDiff = abs(upper - greyscale);

    float level = lowerDiff <= upperDiff ? lower : upper;
    float adjustment = level / greyscale;

    color.rgb *= adjustment;

	imageStore(uSceneColorTexture, uv, color);
}