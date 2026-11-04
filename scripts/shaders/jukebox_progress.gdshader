shader_type canvas_item;

uniform vec4 color_fill : source_color = vec4(1.0);
uniform vec4 color_background : source_color = vec4(0.0, 0.0, 0.0, 1.0);
uniform float progress : hint_range(0.0, 1.0) = 0;
uniform float margin : hint_range(0.0, 1.0) = 0;

void fragment() {
	float x = (SCREEN_UV.x - margin / 2.0) * (1.0 / (1.0 - margin));
	
	COLOR *= progress > x ? color_fill : color_background;
}