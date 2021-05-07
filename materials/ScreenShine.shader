shader_type canvas_item;

uniform sampler2D shineTex;

void fragment()
{
	COLOR = texture(TEXTURE, UV);
	
	vec2 uv_adjust = SCREEN_UV;
	float time_adjust = fract(TIME / 2.0);
	uv_adjust.x -= time_adjust;
	
	COLOR.rgb = (COLOR + texture(shineTex, uv_adjust)).rgb;
}