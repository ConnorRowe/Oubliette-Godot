shader_type canvas_item;

void fragment()
{
	COLOR = texture(TEXTURE, UV);
	
	float brightness = (COLOR.r + COLOR.g + COLOR.b) / 3.0f;
	brightness = clamp(brightness, 1.1f, 5.0f);
	vec4 highlight_colour = mix(COLOR, vec4(1.0f), 0.2f) * brightness;
	
	vec2 uv_adjust = 1.0f / vec2(textureSize(TEXTURE, 0));

	bool left_empty = texture(TEXTURE, UV + vec2(-uv_adjust.x, 0.0f)).a == 0.0f;
	bool up_empty = texture(TEXTURE, UV + vec2(0, -uv_adjust.y)).a == 0.0f;
	
	if(COLOR.a > 0.1f && (left_empty || up_empty))
	{
		COLOR = highlight_colour;
	}
}