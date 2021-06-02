shader_type canvas_item;

uniform sampler2D text_sampler;

uniform vec2 clip_rect_min = vec2(7.0, 7.0);
uniform vec2 clip_rect_max = vec2(132.0, 31.0);

void fragment()
{
	vec4 bg = texture(TEXTURE, UV);
	vec4 text = texture(text_sampler, UV);
	
	COLOR = mix(bg, text, text.a);	
	
	// clip to rect
	
	vec2 tex_size = vec2(textureSize(TEXTURE, 0));
	vec4 clip_rect = vec4(clip_rect_min, clip_rect_max);
	clip_rect.xz /= tex_size.x;
	clip_rect.yw /= tex_size.y;

	if(!(UV.x >= clip_rect.x && UV.x <= clip_rect.z &&
	   UV.y >= clip_rect.y && UV.y <= clip_rect.w))
	{
		COLOR.a = 0.0;
	}
}