shader_type canvas_item;

uniform vec4 red_channel_colour : hint_color = vec4(1.0f, 0.0f, 0.0f, 1.0f);
uniform vec4 green_channel_colour : hint_color = vec4(0.0f, 1.0f, 0.0f, 1.0f);
uniform vec4 blue_channel_colour : hint_color = vec4(0.0f, 0.0f, 1.0f, 1.0f);
uniform sampler2D highlight_texture;

void fragment()
{
	vec4 base_colour = texture(TEXTURE, UV);
	
	COLOR = mix(mix(mix(mix(vec4(0.0,0.0,0.0,1.0), red_channel_colour,  base_colour.r),
	 green_channel_colour, base_colour.g),
	 blue_channel_colour, base_colour.b),
	 vec4(1.0), texture(highlight_texture, UV).a);
	COLOR.a = base_colour.a;
}