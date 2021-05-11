shader_type canvas_item;


uniform sampler2D fill_texture;
uniform float time_factor = 1.0;
uniform vec4 colour_lerp_a : hint_color = vec4(1.0f, 0.0f, 0.0f, 1.0f);
uniform vec4 colour_lerp_b : hint_color = vec4(0.0f, 1.0f, 0.0f, 1.0f);
uniform vec4 colour_lerp_c : hint_color = vec4(0.0f, 0.0f, 1.0f, 1.0f);

varying vec4 colour_lerp;

float sat(float val)
{
	return clamp(val, 0.0, 1.0);
}

vec4 lerp_3_vec4(vec4 a, vec4 b, vec4 c, float time)
{
	float t = (sin(time * time_factor) + 1.0) * 2.0;
	return mix(mix(a, b, sat(t - 1.0f)), c, sat(t - 2.0));
}

void vertex()
{
	colour_lerp = lerp_3_vec4(colour_lerp_a, colour_lerp_b, colour_lerp_c, TIME);
}

void fragment()
{
	vec4 fill_sample = texture(fill_texture, UV) * colour_lerp;
	COLOR = mix(texture(TEXTURE, UV), fill_sample, fill_sample.a);
}