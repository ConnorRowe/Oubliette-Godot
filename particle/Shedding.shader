shader_type particles;
uniform vec3 emission_box_extents = vec3(4, 10, 0);
uniform float base_speed = 1.0f;
uniform vec2 x_dir_range = vec2(-1,1);
uniform vec2 y_dir_range = vec2(0,0);
uniform float gravity = 0.05;
uniform vec4 base_colour: hint_color = vec4(1,1,1,1);

float rand_from_seed(inout uint seed) {
		int k;
	int s = int(seed);
	if (s == 0)
	s = 305420679;
	k = s / 127773;
	s = 16807 * (s - k * 127773) - 2836 * k;
	if (s < 0)
		s += 2147483647;
	seed = uint(s);
	return float(seed % uint(65536)) / 65535.0;
}

uint hash(uint x) {
	x = ((x >> uint(16)) ^ x) * uint(73244475);
	x = ((x >> uint(16)) ^ x) * uint(73244475);
	x = (x >> uint(16)) ^ x;
	return x;
}

void vertex()
{
	uint seed = hash(NUMBER + RANDOM_SEED);

	if (RESTART) {
		vec3 direction = vec3(mix(x_dir_range.x, x_dir_range.y, rand_from_seed(seed)), mix(y_dir_range.x, y_dir_range.y, rand_from_seed(seed)), 0);
		float speed = rand_from_seed(seed) * base_speed;
		VELOCITY = direction * speed;
		TRANSFORM[3].xyz = vec3(rand_from_seed(seed) * 2.0 - 1.0, rand_from_seed(seed) * 2.0 - 1.0, rand_from_seed(seed) * 2.0 - 1.0) * emission_box_extents;
		COLOR = base_colour;
	}
	else
	{
		VELOCITY.y += gravity;
	}
	
	COLOR.a = max(COLOR.a - DELTA, 0.0);
}