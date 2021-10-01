shader_type canvas_item;

uniform vec4 overlay_colour: hint_color = vec4(1.0);
uniform float glow_scalar = 4.0;

void fragment()
{
	// Just makes the colour flat
	COLOR = overlay_colour * glow_scalar;
	COLOR.a = texture(TEXTURE, UV).a;
}