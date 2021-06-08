shader_type canvas_item;

uniform float hue_shift = 0.0f;
uniform float glow_power = 0.0f;
uniform float alpha_adjust = 0.0f;
uniform vec4 color_a: hint_color;
uniform vec4 color_b: hint_color;

const float TAU = 6.28318530717958647692f;

vec2 rotate2(vec2 v, float fi) {
	return v*mat2(vec2(cos(fi), -sin(fi)), vec2(sin(fi), cos(fi)));
}

// YIQ color rotation/hue shift
vec3 hueShiftYIQ(vec3 rgb, float hs) {
	float rotAngle = hs*-6.28318530718;
	
	const mat3 rgb2yiq = mat3(vec3(0.299, 0.596, 0.211),
	vec3(0.587, -0.274, -0.523),
	vec3(0.114, -0.322, 0.312));
	
	const mat3 yiq2rgb = mat3(vec3(1, 1, 1),
	vec3(0.956, -0.272, -1.106),
	vec3(0.621, -0.647, 1.703));
	vec3 yiq = rgb2yiq * rgb;
	
	yiq.yz *= rotate2(yiq.yz, rotAngle);
	
	return yiq2rgb * yiq;
}

void fragment()
{
	COLOR = texture(TEXTURE, UV);
	COLOR.rgb = mix(color_a.rgb, color_b.rgb, UV.y);
	
	COLOR.rgb = hueShiftYIQ(COLOR.rgb, hue_shift) * (max(1.0f, glow_power));
	
	if(COLOR.a > 0.0f)
		COLOR.a += alpha_adjust;
}

