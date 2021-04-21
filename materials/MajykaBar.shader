shader_type canvas_item;

uniform float fill_percent: hint_range(0.0, 1.0) = 0.5;
uniform vec4 colour1: hint_color;
uniform vec4 colour2: hint_color;
uniform float contrast = 4.0;
uniform float time_scale = 1.0;
uniform float pan_speed = 1.0;
uniform vec2 pan_direction = vec2(0.0, 1.0);
uniform sampler2D noise;
uniform vec2 tex_scale = vec2(256.0, 256.0);

void fragment()
{
	vec4 col = texture(TEXTURE, UV);
	vec2 pan = pan_direction * TIME * pan_speed;
	
	if(col == vec4(1.0) && UV.x < fill_percent)
	{
		col = mix(colour1, colour2, cos((TIME * time_scale) + (texture(noise, ((UV + pan) / TEXTURE_PIXEL_SIZE) / tex_scale) * contrast)));
	}
	else
	{
		col = vec4(0.0);
	}
	
	COLOR = col;
}