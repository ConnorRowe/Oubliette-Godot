shader_type particles;

float rand_from_seed(inout uint seed)
{
	int k;
	int s = int(seed);
	if(s == 0)
		s = 305420679;
	k = s / 127773;
	s = 16807 * (s - k * 127773) - 2836 * k;
	if(s < 0)
		s += 2147483647;
	seed = uint(s);
	return float(seed % uint(65536)) / 65536.0;
}

void vertex() {
	uint seed = NUMBER + RANDOM_SEED;
	if(RESTART) {
		vec3 direction = vec3(rand_from_seed(seed) - 0.5, rand_from_seed(seed) - 0.5, 0);
		float speed = rand_from_seed(seed) * 32.0;
		VELOCITY = direction * speed;
	}
	else
	{
		VELOCITY.y += 0.2;
	}
	
	COLOR.a = max(COLOR.a - DELTA, 0.0);
}