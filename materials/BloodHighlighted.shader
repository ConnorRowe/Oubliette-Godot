shader_type canvas_item;

uniform vec4 blood_colour: hint_color = vec4(1.0, 0.0, 0.0, 1.0);
uniform vec4 highlight_colour: hint_color = vec4(2.0, 2.0, 2.0, 2.0);

void fragment()
{
	vec4 sample = texture(TEXTURE, UV);
	COLOR = sample;
	COLOR.rgb *= blood_colour.rgb;
	COLOR.a *= sample.r;
	
	
	// Highlight 
	float highlight_width = 1.0;
	float w = highlight_width * 1.0 / float(textureSize(TEXTURE, 0).x);
	float h = highlight_width * 1.0 / float(textureSize(TEXTURE, 0).y);
	
	float alpha = -2.0 * COLOR.a;
	alpha += texture(TEXTURE, UV + vec2(w, 0.0)).a;
	alpha += texture(TEXTURE, UV + vec2(0.0, h)).a;
	
	
	COLOR = mix(COLOR, highlight_colour, clamp(alpha, 0.0, 1.0));
}