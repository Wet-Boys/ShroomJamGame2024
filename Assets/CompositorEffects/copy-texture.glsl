#[compute]
#version 450

// Invocations in the (x, y, z) dimension
layout(local_size_x = 8, local_size_y = 8, local_size_z = 1) in;

layout(rgba16f, set = 0, binding = 0) uniform image2D src_image;
layout(rgba16f, set = 0, binding = 1) uniform image2D dst_image;

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

	vec4 color = imageLoad(src_image, uv);
	imageStore(dst_image, uv, color);
}