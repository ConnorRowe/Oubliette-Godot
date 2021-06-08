shader_type canvas_item;

uniform int charges_remaining = 5;

void fragment()
{
	COLOR = texture(TEXTURE, UV);
	
	if(UV.x * 16.0f > (float(charges_remaining) * (16.0f / 5.0f)))
	{
		COLOR.a = 0.0;
	}
}