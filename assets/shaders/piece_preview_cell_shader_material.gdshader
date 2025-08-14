shader_type canvas_item;

void vertex() {
	// Called for every vertex the material is visible on.
}

uniform bool enabled;
uniform float lightness_factor : hint_range(0.0, 1.0, 0.01);

void fragment() {
    COLOR = texture(TEXTURE, UV);
	if(enabled){
		float avg = (COLOR.r + COLOR.g + COLOR.b) / 3.0;
    	COLOR.rgb = vec3(avg) * lightness_factor;
	}

}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
