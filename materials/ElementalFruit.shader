shader_type canvas_item;

uniform float time_factor = 1.0;
uniform vec4 red_channel_colour_lerp_a : hint_color = vec4(1.0f, 0.0f, 0.0f, 1.0f);
uniform vec4 red_channel_colour_lerp_b : hint_color = vec4(1.0f, 0.0f, 0.0f, 1.0f);
uniform vec4 red_channel_colour_lerp_c : hint_color = vec4(1.0f, 0.0f, 0.0f, 1.0f);

uniform vec4 green_channel_colour : hint_color = vec4(0.0f, 1.0f, 0.0f, 1.0f);
uniform vec4 blue_channel_colour : hint_color = vec4(0.0f, 0.0f, 1.0f, 1.0f);
uniform sampler2D highlight_texture;

varying vec4 red_channel_colour;

float sat(float val)
{
	return clamp(val, 0.0, 1.0);
}

vec4 lerp_3_vec4(vec4 a, vec4 b, vec4 c, float time)
{
	float t = (sin(time) + 1.0) * time_factor;
	return mix(mix(a, b, sat(t - 1.0f)), c, sat(t - 2.0));
}

void vertex()
{
	red_channel_colour = lerp_3_vec4(red_channel_colour_lerp_a, red_channel_colour_lerp_b, red_channel_colour_lerp_c, TIME);
}

void fragment()
{
	vec4 base_colour = texture(TEXTURE, UV);
	
	COLOR = mix(mix(mix(mix(vec4(0.0,0.0,0.0,1.0), red_channel_colour,  base_colour.r),
	 green_channel_colour, base_colour.g),
	 blue_channel_colour, base_colour.b),
	 vec4(1.0), texture(highlight_texture, UV).a);
	COLOR.a = base_colour.a;
}