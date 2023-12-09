shader_type canvas_item;

uniform float interpolation_value = 0 ;
uniform vec4 interpolation_color = vec4( 1.0, 1.0, 1.0, 1.0 ) ; // 青
uniform vec4 modulate_color = vec4( 1.0, 1.0, 1.0, 1.0 ) ;


void fragment()
{
    vec4 color = texture( TEXTURE, UV ) ;
	COLOR = modulate_color * mix( color, vec4( interpolation_color.rgb, color.a * interpolation_color.a ), interpolation_value ) ;
}
