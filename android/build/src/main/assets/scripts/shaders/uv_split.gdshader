shader_type canvas_item;

uniform float split : hint_range(0.0, 1.0) = 0.5;
uniform float shift : hint_range(0.0, 1.0) = 0.0;
uniform float angle : hint_range(0.0, 360.0) = 0.0;

vec2 rotate(vec2 v, float rad) {
	float x = v.x * cos(rad) - v.y * sin(rad);
	float y = v.x * sin(rad) + v.y * cos(rad);
	
	return vec2(x, y);
}

void fragment() {
	vec2 uv = UV;
	float rad = angle * (PI / 180.0);
	
	uv = rotate(uv - vec2(0.5), rad) + vec2(0.5);
	
	float dir = uv.x < split ? -1.0 : 1.0;
	float offset = dir * (1.0 - shift) / 2.0;
	
	uv.x += offset;
	uv = rotate(uv - vec2(0.5), -rad) + vec2(0.5);
	
	COLOR = texture(TEXTURE, uv);
}