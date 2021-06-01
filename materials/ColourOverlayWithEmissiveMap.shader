shader_type canvas_item;

uniform vec4 colour_override : hint_color;
uniform float override_lerp: hint_range(0, 1) = 0f;
uniform sampler2D emission_texture; // has to fit same texcoord as sprite texture
uniform vec4 emission_tint: hint_color = vec4(1.0);
uniform float intensity;

void fragment()
{
	vec4 tex = texture(TEXTURE, UV);
	vec4 base_colour = mix(tex, vec4(colour_override.rgb, tex.a * colour_override.a), override_lerp);
		
	vec4 emissive_sample = texture(emission_texture, UV);
	
	COLOR = mix(base_colour, emissive_sample * emission_tint * intensity, emissive_sample.a);
}