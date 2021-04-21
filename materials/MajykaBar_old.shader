shader_type canvas_item;

const vec4 mask = vec4(1.0, 0.0, 1.0, 1.0);
uniform vec4 colour1: hint_color;
uniform vec4 colour2: hint_color;
uniform sampler2D noise;
uniform vec2 screen_uv_factor = vec2(1.0, 1.0);

void fragment()
{
	vec4 col = texture(TEXTURE, UV);
	
	if(col == mask)
	{
		COLOR = mix(colour1, colour2, cos(TIME + (texture(noise, SCREEN_UV * screen_uv_factor) * (4.0))));
	}
	else
	{
		COLOR = col;
	}
}