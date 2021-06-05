shader_type canvas_item;

uniform vec4 overlay_colour: hint_color = vec4(1.0);

void fragment()
{
	// Just makes the colour flat
	COLOR = overlay_colour;
	COLOR.a = texture(TEXTURE, UV).a;
}