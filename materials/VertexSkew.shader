shader_type canvas_item;

uniform float skew_amount = 0.0;
uniform float max_skew = 1.0;

// works nicely but only with 4 vertices like a sprite
void vertex()
{	
	if(VERTEX == vec2(0.0, 0.0)) // TOP-LEFT
	{
		VERTEX += vec2(skew_amount * 0.5, skew_amount)
	}
	else if(VERTEX.x > VERTEX.y) // TOP-RIGHT
	{
		VERTEX += vec2(- ((max_skew - skew_amount) * 0.5), max_skew - skew_amount)
	}
	else if(VERTEX.x == VERTEX.y) // BOTTOM-RIGHT
	{
		VERTEX += vec2(- ((max_skew - skew_amount) * 0.5), - (max_skew - skew_amount))
	}
	else if(VERTEX.y > VERTEX.x) // BOTTOM-LEFT
	{
		VERTEX += vec2(skew_amount * 0.5, -skew_amount)
	}
}