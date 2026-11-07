shader_type canvas_item;

void fragment() {
	float alpha = (0.5 - abs(UV.x - 0.5)) * 3.0;

	COLOR = vec4(COLOR.rgb, alpha / 1.5);
}