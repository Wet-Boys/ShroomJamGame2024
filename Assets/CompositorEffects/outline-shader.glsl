#[vertex]
#version 450 core

layout(location = 0) in vec3 iVertex;

layout(set = 0, binding = 0) uniform Uniforms {
    mat4 InvProjMat;
} uniforms;

layout(location = 0) out vec3 oViewSpaceDir;

void main() {
    vec4 vertex = vec4(iVertex, 1.0);

    oViewSpaceDir = (uniforms.InvProjMat * vertex).xyz;

    gl_Position = vertex;
}

#[fragment]
#version 450 core

layout(location = 0) in vec3 iViewSpaceDir;

layout(set = 1, binding = 0) uniform sampler2D uColorTexture;
layout(set = 1, binding = 1) uniform sampler2D uDepthTexture;
layout(set = 1, binding = 2) uniform sampler2D uNormalRoughnessTexture;

layout(push_constant, std430) uniform Variables {
	vec4 Time;
} vars;

layout(set = 1, binding = 3) uniform Params {
	float Scale;
    float DepthThreshold;
	float NormalThreshold;
    float DepthNormalThreshold;
    float DepthNormalThresholdScale;
    float ShowOnlyOutlines;
    float _padding2;
    float _padding3;
    vec4 EdgeColor;
} params;

layout(location = 0) out vec4 oFragColor;

void main() {
    vec2 texelSize = 1.0 / vec2(textureSize(uColorTexture, 0));
    vec2 texCoords = gl_FragCoord.xy * texelSize;

    float dirPosX = (dot(gl_FragCoord.xy, vec2(1.0, 0.0)) + 1) / 2;
    float dirPosY = (dot(gl_FragCoord.xy, vec2(0.0, 1.0)) + 1) / 2;

    float aspect = texelSize.y / texelSize.x;
    float lineSize = 0.1;

    // float lines = (step(fract(dirPosX * lineSize), 0.5) * 0.65) * (step(fract(dirPosY * lineSize), 0.5) * 0.65);

    float lineShift = 0.005;

    // texCoords += vec2(lineShift * lines, 0);

    vec4 color = texture(uColorTexture, texCoords);

    float halfScaleFloor = floor(params.Scale * 0.5);
    float halfScaleCeil = ceil(params.Scale * 0.5);

    vec2 bottomLeftUv = texCoords - vec2(texelSize.x, texelSize.y) * halfScaleFloor;
    vec2 topRightUv = texCoords + vec2(texelSize.x, texelSize.y) * halfScaleCeil;
    vec2 bottomRightUv = texCoords + vec2(texelSize.x * halfScaleCeil, -texelSize.y * halfScaleFloor);
    vec2 topLeftUv = texCoords + vec2(-texelSize.x * halfScaleFloor, texelSize.y * halfScaleCeil);

    float depth0 = texture(uDepthTexture, bottomLeftUv).r;
    float depth1 = texture(uDepthTexture, topRightUv).r;
    float depth2 = texture(uDepthTexture, bottomRightUv).r;
    float depth3 = texture(uDepthTexture, topLeftUv).r;

    float depthFiniteDiff0 = depth1 - depth0;
    float depthFiniteDiff1 = depth3 - depth2;

    float edgeDepth = sqrt(pow(depthFiniteDiff0, 2) + pow(depthFiniteDiff1, 2)) * 100;

    vec3 normal0 = texture(uNormalRoughnessTexture, bottomLeftUv).rgb;
    vec3 normal1 = texture(uNormalRoughnessTexture, topRightUv).rgb;
    vec3 normal2 = texture(uNormalRoughnessTexture, bottomRightUv).rgb;
    vec3 normal3 = texture(uNormalRoughnessTexture, topLeftUv).rgb;

    vec3 normalFiniteDiff0 = normal1 - normal0;
    vec3 normalFiniteDiff1 = normal3 - normal2;

    float edgeNormal = sqrt(dot(normalFiniteDiff0, normalFiniteDiff0) + dot(normalFiniteDiff1, normalFiniteDiff1));
    edgeNormal = edgeNormal > params.NormalThreshold ? 1 : 0;

    vec3 viewNormal = normal0 * 2 - 1;
    float NdotV = 1 - dot(viewNormal, -iViewSpaceDir);

    float normalThreshold01 = clamp((NdotV - params.DepthNormalThreshold) / (1 - params.DepthNormalThreshold), 0, 1);
    float normalThreshold = normalThreshold01 * params.DepthNormalThresholdScale + 1;

    float depthThreshold = params.DepthThreshold * depth0 * normalThreshold;
    edgeDepth = edgeDepth > depthThreshold ? 1 : 0;

    float edge = max(edgeDepth, edgeNormal);

    // edge = edge * lines;

    vec4 edgeColor = vec4(params.EdgeColor.rgb, params.EdgeColor.a * edge);

    color = mix(color, vec4(0, 0, 0, 0), params.ShowOnlyOutlines);

    oFragColor = mix(color, edgeColor, edge);
    // oFragColor = vec4(lines, lines, lines, 1.0);
}