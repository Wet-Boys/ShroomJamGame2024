#[vertex]
#version 450 core

layout(location = 0) in vec3 iVertex;

void main() {
    vec4 vertex = vec4(iVertex, 1.0);

    gl_Position = vertex;
}

#[fragment]
#version 450 core

layout(set = 0, binding = 0) uniform sampler2D uFragColorTexture;

layout(location = 0) out vec4 oFragColor;

void main() {
    vec2 texelSize = 1.0 / vec2(textureSize(uFragColorTexture, 0));
    vec2 screenTexCoords = gl_FragCoord.xy * texelSize;

    vec4 color = texture(uFragColorTexture, screenTexCoords);

    oFragColor = color;
}