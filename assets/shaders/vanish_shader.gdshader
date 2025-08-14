// warning: this shader is extremely janky, not mine and it doesnt work like i wanted it to. boohoo


shader_type canvas_item;

uniform sampler2D sprite_texture;
uniform float progress : hint_range(0.25,0.45);
const float pixel_thickness = 0.1;
uniform bool enabled;

vec2 rand(vec2 v)
{
	return vec2(fract(sin(dot(v.xy ,vec2(12.9898,78.233))) * 43758.5453),
                fract(cos(dot(v.yx ,vec2(31.6245,22.723))) * 63412.9227)) - 0.5;
}


vec4 explode(vec2 p, float time, in float blast, in vec2 uv)
{
	float t = clamp(time-2.0, 0.0, 4.0);

	p = p/pixel_thickness;

    vec2 r = rand(p);
	vec2 delta = 2.0*r * vec2(0.4*(3.0-t), 1.0) - vec2(0.0, t*0.9);

	p -= blast*delta*t;

	if (clamp(p.x, 0.0, 37.0) != p.x || clamp(p.y, 0.0, 32.0) != p.y) return vec4(0.0);
	p.y= 32.0-p.y;

	return smoothstep(0.0, 0.2, time)*smoothstep(2.0, 1.2, t)*texture(sprite_texture, uv);
}

#define dur 5.0
#define spd 4.0

void fragment() {
	if(enabled){
		vec2 iResolution = 1.0 / TEXTURE_PIXEL_SIZE;

		vec2 p = UV;

	    float blast = 50.0;
	    float t = spd*progress*2.0;

		vec4 col = explode(p, mod(t, dur), blast, UV);
		col = mix(col, explode(p, mod(t - 0.05, dur), blast, UV), 0.3);
		col = mix(col, explode(p, mod(t - 0.10, dur), blast, UV), 0.2);
		col = mix(col, explode(p, mod(t - 0.15, dur), blast, UV), 0.1);

	    vec2 uv = FRAGCOORD.xy / iResolution.xy;

		COLOR = col;
	}
}
