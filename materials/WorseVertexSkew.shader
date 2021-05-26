shader_type canvas_item;

uniform float max_skew = 2.5;
uniform float skew_factor = 0.0;

// can't get it to work as good as the other one but it works with more than 4 vertices
void vertex()
{
	float skew = max_skew * skew_factor;
	
	vec2 skew_amount = TEXTURE_PIXEL_SIZE * VERTEX;
	
	vec2 final_skew = vec2( mix(skew * 0.5, - ((max_skew - skew) * 0.5), skew_amount.x), mix(max_skew - skew,  skew, skew_amount.x));
	
	final_skew.y *= mix(0.0, 2.0, skew_amount.y);
	
	VERTEX += final_skew;
}