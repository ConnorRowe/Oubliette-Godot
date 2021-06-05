shader_type particles;

uniform vec3 emission_box_extents;

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

float rand_from_seed_m1_p1(inout uint seed) {
	return rand_from_seed(seed) * 2.0 - 1.0;
}

uint hash(uint x) {
	x = ((x >> uint(16)) ^ x) * uint(73244475);
	x = ((x >> uint(16)) ^ x) * uint(73244475);
	x = (x >> uint(16)) ^ x;
	return x;
}

void vertex()
{
	uint alt_seed = hash(NUMBER + uint(1) + RANDOM_SEED);
	vec3 base_scale = vec3(0.0);
	
	if(RESTART)
	{
		TRANSFORM[3].xyz = vec3(rand_from_seed(alt_seed) * 2.0 - 1.0, rand_from_seed(alt_seed) * 2.0 - 1.0, 0.0) * emission_box_extents;
		base_scale = TRANSFORM[1].xyz;
		COLOR.rgb += vec3(-0.2f, 0.5f, rand_from_seed_m1_p1(alt_seed));
	}
	else
	{
		VELOCITY.y += 1.96;
		TRANSFORM[0].xyz *= (0.996f);
		TRANSFORM[1].xyz *= (1.02f);
	}

}