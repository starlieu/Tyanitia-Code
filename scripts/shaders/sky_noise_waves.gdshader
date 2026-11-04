shader_type sky;

uniform bool sine_color_swap = false;
uniform float opacity : hint_range(0.0, 1.0) = 1.0;
uniform float coverage : hint_range(0.0, 1.0) = 1.0;
uniform vec3 background_color : source_color = vec3(0.0);
uniform sampler2D image_a;
uniform sampler2D image_b;
uniform float image_lerp : hint_range(0.0, 1.0) = 0.0;
uniform float image_lod : hint_range(0.0, 8.0, 0.1) = 5.0;

//	Simplex 3D Noise
//	by Ian McEwan, Stefan Gustavson (https://github.com/stegu/webgl-noise)
//	Reformatted by nyuwaea (seriously, this was harder to read than it should've been)

vec4 permute(vec4 x) {
	return mod(((x * 34.0) + 1.0) * x, 289.0);
}

vec4 taylorInvSqrt(vec4 r) {
	return 1.79284291400159 - 0.85373472095314 * r;
}

float snoise(vec3 v) {
	const vec2 C = vec2(1.0 / 6.0, 1.0 / 3.0);
	const vec4 D = vec4(0.0, 0.5, 1.0, 2.0);

	// First corner
	vec3 i = floor(v + dot(v, C.yyy));
	vec3 x0 = v - i + dot(i, C.xxx);

	// Other corners
	vec3 g = step(x0.yzx, x0.xyz);
	vec3 l = 1.0 - g;
	vec3 i1 = min(g.xyz, l.zxy);
	vec3 i2 = max(g.xyz, l.zxy);

	// x0 = x0 - 0. + 0.0 * C
	vec3 x1 = x0 - i1 + 1.0 * C.xxx;
	vec3 x2 = x0 - i2 + 2.0 * C.xxx;
	vec3 x3 = x0 - 1.0 + 3.0 * C.xxx;

	// Permutations
	i = mod(i, 289.0);
	vec4 p = permute(permute(permute(i.z + vec4(0.0, i1.z, i2.z, 1.0)) + i.y + vec4(0.0, i1.y, i2.y, 1.0)) + i.x + vec4(0.0, i1.x, i2.x, 1.0));

	// Gradients
	// (N*N points uniformly over a square, mapped onto an octahedron.)
	float n = 1.0 / 7.0;
	vec3 ns = n * D.wyz - D.xzx;

	vec4 j = p - 49.0 * floor(p * ns.z * ns.z);

	vec4 x_ = floor(j * ns.z);
	vec4 y_ = floor(j - 7.0 * x_);

	vec4 x = x_ * ns.x + ns.yyyy;
	vec4 y = y_ * ns.x + ns.yyyy;
	vec4 h = 1.0 - abs(x) - abs(y);

	vec4 b0 = vec4(x.xy, y.xy);
	vec4 b1 = vec4(x.zw, y.zw);

	vec4 s0 = floor(b0) * 2.0 + 1.0;
	vec4 s1 = floor(b1) * 2.0 + 1.0;
	vec4 sh = -step(h, vec4(0.0));

	vec4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
	vec4 a1 = b1.xzyw + s1.xzyw * sh.zzww;

	vec3 p0 = vec3(a0.xy, h.x);
	vec3 p1 = vec3(a0.zw, h.y);
	vec3 p2 = vec3(a1.xy, h.z);
	vec3 p3 = vec3(a1.zw, h.w);

	// Normalise gradients
	vec4 norm = taylorInvSqrt(vec4(dot(p0, p0), dot(p1, p1), dot(p2, p2), dot(p3, p3)));
	p0 *= norm.x;
	p1 *= norm.y;
	p2 *= norm.z;
	p3 *= norm.w;

	// Mix final noise value
	vec4 m = max(0.6 - vec4(dot(x0, x0), dot(x1, x1), dot(x2, x2), dot(x3, x3)), 0.0);
	m *= m;

	return 42.0 * dot(m * m, vec4(dot(p0, x0), dot(p1, x1), dot(p2, x2), dot(p3, x3)));
}

void sky() {
	vec3 pos = EYEDIR;

	pos.y = abs(pos.y);
	pos.y += TIME / 70.0;

	float noiseValue = (snoise(pos) + 1.0) / 2.0;
	noiseValue = clamp(noiseValue, 0.0, 1.0);

	bool primaryWaves = mod(floor(noiseValue * 96.0), 20.0) == 0.0;
	bool secondaryWaves = mod(floor(noiseValue * 384.0), 20.0) == 0.0;
	float t = primaryWaves ? 1.0 : secondaryWaves ? 0.2 : 0.0;
	float sine = sin(TIME / 70.0) / 2.0 + 0.5;

	vec2 sample = vec2(noiseValue, sine) * 0.9 + vec2(0.05);
	vec4 imageColor = mix(textureLod(image_b, sample, image_lod), textureLod(image_a, sample, image_lod), image_lerp);
	vec3 teal = vec3(0.0, 0.78, 0.72);
	vec3 pink = vec3(0.96, 0.12, 0.62);
	vec3 color = mix(mix(teal, pink, noiseValue), imageColor.rgb * 1.5, imageColor.a);
	color = mix(background_color, color, (t + (1.0 - pow(noiseValue, 100.0)) / 50.0) * opacity * coverage);

	if (sine_color_swap) {
		color = mix(color, color.bgr, sin(TIME / 10.0) / 2.0 + 0.5);
	}

	color = mix(vec3(0.0), color, min(1.0, 2.0 * coverage - abs(EYEDIR.y)));

	COLOR = color;
}
