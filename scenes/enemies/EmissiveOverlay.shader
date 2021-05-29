shader_type canvas_item;

uniform sampler2D emission_texture;
uniform vec4 emission_tint: hint_color = vec4(1.0);
uniform float intensity;

void fragment()
{
	vec4 emissive_sample = texture(emission_texture, UV);
	COLOR = mix(emissive_sample * emission_tint * intensity, texture(TEXTURE, UV), 1.0-emissive_sample.a);
}